using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace HeadNonSub.Clients.Discord {

    /// <summary>
    /// Text commands with the ! prefix, like "!ttsays hello". Each one is an alias for a slash command and is checked
    /// the same way, slash commands stay the main way to use the bot.
    /// </summary>
    public class PrefixCommands {

        /// <summary>
        /// Prefix for text commands.
        /// </summary>
        public const string Prefix = "!";

        /// <summary>
        /// Aliases from each command module, add new ones to this list.
        /// </summary>
        public static IEnumerable<PrefixAlias> Aliases => new IEnumerable<PrefixAlias>[] {
            Says.PrefixAliases
        }.SelectMany(x => x);

        private readonly DiscordSocketClient _DiscordClient;
        private readonly IServiceProvider _Services;
        private readonly IReadOnlyDictionary<string, (PrefixAlias Alias, SlashCommandInfo Command)> _Commands;

        /// <summary>
        /// Start handling ! commands. Call after the slash command modules are loaded.
        /// </summary>
        /// <param name="services">Services with the <see cref="DiscordSocketClient"/> and <see cref="InteractionService"/>.</param>
        public PrefixCommands(IServiceProvider services) {
            _Services = services;
            _DiscordClient = services.GetRequiredService<DiscordSocketClient>();
            _Commands = MatchSlashCommands(services.GetRequiredService<InteractionService>());

            _DiscordClient.MessageReceived += MessageReceived;
        }

        /// <summary>
        /// Find the slash command for each alias.
        /// </summary>
        /// <param name="interactions">Interaction service with the modules loaded.</param>
        public static IReadOnlyDictionary<string, (PrefixAlias Alias, SlashCommandInfo Command)> MatchSlashCommands(InteractionService interactions) {
            Dictionary<string, (PrefixAlias Alias, SlashCommandInfo Command)> commands = new Dictionary<string, (PrefixAlias Alias, SlashCommandInfo Command)>(StringComparer.OrdinalIgnoreCase);

            foreach (PrefixAlias alias in Aliases) {
                // Fail at startup so a typo is caught right away
                SlashCommandInfo command = interactions.SlashCommands.FirstOrDefault(x => FullName(x) == alias.SlashCommand)
                    ?? throw new InvalidOperationException($"{Prefix}{alias.Name} is an alias for /{alias.SlashCommand}, which does not exist");

                if (!commands.TryAdd(alias.Name, (alias, command))) {
                    throw new InvalidOperationException($"{Prefix}{alias.Name} is registered more than once");
                }
            }

            return commands;
        }

        /// <summary>
        /// Full name of a slash command including its groups, like blacklist add.
        /// </summary>
        /// <param name="command">Slash command.</param>
        public static string FullName(SlashCommandInfo command) {
            List<string> names = new List<string> { command.Name };

            for (ModuleInfo module = command.Module; module != null; module = module.Parent) {
                if (module.IsSlashGroup) {
                    names.Insert(0, module.SlashGroupName);
                }
            }

            return string.Join(" ", names);
        }

        private Task MessageReceived(SocketMessage socketMessage) {
            if (!(socketMessage is SocketUserMessage message)) { return Task.CompletedTask; }
            if (message.Source != MessageSource.User) { return Task.CompletedTask; }
            if (!(message.Channel is SocketTextChannel channel) || !(message.Author is SocketGuildUser user)) { return Task.CompletedTask; }

            // Unknown names are ignored, other bots use ! too
            if (!TrySplit(message.Content, out string name, out string text) || !_Commands.TryGetValue(name, out (PrefixAlias Alias, SlashCommandInfo Command) entry)) {
                return Task.CompletedTask;
            }

            // Keep commands off the gateway thread
            _ = Task.Run(() => RunAsync(new PrefixContext(_DiscordClient, channel, message, user, name, text), entry.Alias, entry.Command));
            return Task.CompletedTask;
        }

        private async Task RunAsync(PrefixContext context, PrefixAlias alias, SlashCommandInfo command) {
            try {
#if DEBUG
                if (context.User.Id != Constants.XathzUserId) {
                    await context.ReplyAsync(SlashCommands.DebugMessage);
                    return;
                }
#endif

                // Same preconditions as the slash command, like blacklist, subscriber only and cooldowns
                PreconditionResult result = await command.CheckPreconditionsAsync(context, _Services);
                if (!result.IsSuccess) {
                    LoggingManager.Log.Warn($"Command {context.Command}: {result.ErrorReason}");
                    await context.ReplyAsync(result.ErrorReason);
                    return;
                }

                try {
                    // Show the bot is typing while it works, like the thinking message for slash commands
                    using (context.Channel.EnterTypingState()) {
                        await alias.RunAsync(context);
                    }
                } catch (Exception ex) {
                    LoggingManager.Log.Error(ex, $"Command {context.Command} failed");
                    await context.ReplyAsync(SlashCommands.ErrorMessage);
                    return;
                }

                // The result names the user, so the command message is not needed anymore
                if (context.Completed) {
                    await DiscordClient.DeleteQuietlyAsync(context.Message);
                }
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

        /// <summary>
        /// Split a message like "!ttsays hello" into the name and text.
        /// </summary>
        /// <param name="content">Message content.</param>
        /// <param name="name">Name after the prefix, lowercase.</param>
        /// <param name="text">Text after the name, empty if there is none.</param>
        /// <returns>True if the message starts with the prefix and a name.</returns>
        public static bool TrySplit(string content, out string name, out string text) {
            name = null;
            text = null;

            if (string.IsNullOrEmpty(content) || !content.StartsWith(Prefix, StringComparison.Ordinal)) { return false; }

            string[] parts = content.Substring(Prefix.Length).Split((char[])null, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) { return false; }

            name = parts[0].ToLowerInvariant();
            text = parts.Length > 1 ? parts[1].Trim() : string.Empty;
            return true;
        }

    }

}
