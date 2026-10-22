using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Commands;
using HeadNonSub.Settings;

namespace HeadNonSub.Clients.Discord {

    /// <summary>
    /// Text commands with the ! prefix, like "!ttsays hello". Each one is an alias for a slash command, slash commands stay the main way to use the bot.
    /// </summary>
    public class PrefixCommands {

        /// <summary>
        /// Prefix for text commands.
        /// </summary>
        public const string Prefix = "!";

        // Aliases from each command module, add new ones to this list
        private static readonly IReadOnlyDictionary<string, PrefixAlias> _Aliases = new IEnumerable<PrefixAlias>[] {
            Says.PrefixAliases
        }.SelectMany(x => x).ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

        private readonly DiscordSocketClient _DiscordClient;

        public PrefixCommands(DiscordSocketClient discordClient) {
            _DiscordClient = discordClient;
            _DiscordClient.MessageReceived += MessageReceived;
        }

        private Task MessageReceived(SocketMessage socketMessage) {
            if (!(socketMessage is SocketUserMessage message)) { return Task.CompletedTask; }
            if (message.Source != MessageSource.User) { return Task.CompletedTask; }
            if (!(message.Channel is SocketTextChannel channel) || !(message.Author is SocketGuildUser user)) { return Task.CompletedTask; }

            // Unknown names are ignored, other bots use ! too
            if (!TryParse(message.Content, out PrefixAlias alias, out string name, out string text)) { return Task.CompletedTask; }

#if DEBUG
            if (user.Id != Constants.XathzUserId) { return Task.CompletedTask; }
#endif

            // Blacklisted users are ignored, like mention replies
            if (SettingsManager.Configuration.DiscordBlacklist.Any(x => x.Key == channel.Guild.Id & x.Value.Contains(user.Id))) { return Task.CompletedTask; }

            // Keep commands off the gateway thread
            _ = Task.Run(async () => {
                try {
                    await alias.RunAsync(new PrefixContext(channel, message, user, name, text));
                } catch (Exception ex) {
                    LoggingManager.Log.Error(ex, $"Command {Prefix}{name} failed");
                }
            });

            return Task.CompletedTask;
        }

        /// <summary>
        /// Split a message like "!ttsays hello" into the alias and text.
        /// </summary>
        /// <param name="content">Message content.</param>
        /// <param name="alias">Alias that was used.</param>
        /// <param name="name">Name that was used, lowercase.</param>
        /// <param name="text">Text after the name, empty if there is none.</param>
        /// <returns>True if the message is a registered alias.</returns>
        public static bool TryParse(string content, out PrefixAlias alias, out string name, out string text) {
            alias = null;
            name = null;
            text = null;

            if (string.IsNullOrEmpty(content) || !content.StartsWith(Prefix, StringComparison.Ordinal)) { return false; }

            string[] parts = content.Substring(Prefix.Length).Split((char[])null, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0 || !_Aliases.TryGetValue(parts[0], out alias)) { return false; }

            name = parts[0].ToLowerInvariant();
            text = parts.Length > 1 ? parts[1].Trim() : string.Empty;
            return true;
        }

    }

}
