using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Clients.Discord.Commands;
using HeadNonSub.Settings;
using HeadNonSub.Statistics;

namespace HeadNonSub.Clients.Discord {

    /// <summary>
    /// The old text commands like "!ttsays hello", an alias for /says with the same checks and cooldowns.
    /// </summary>
    public class SaysAliases {

        private const string _Prefix = "!";

        private readonly DiscordSocketClient _DiscordClient;

        public SaysAliases(DiscordSocketClient discordClient) {
            _DiscordClient = discordClient;
            _DiscordClient.MessageReceived += MessageReceived;
        }

        private Task MessageReceived(SocketMessage socketMessage) {
            if (!(socketMessage is SocketUserMessage message)) { return Task.CompletedTask; }
            if (message.Source != MessageSource.User) { return Task.CompletedTask; }
            if (!(message.Channel is SocketTextChannel channel) || !(message.Author is SocketGuildUser user)) { return Task.CompletedTask; }

            if (!TryParse(message.Content, out string alias, out Says.Template who, out string text)) { return Task.CompletedTask; }

#if DEBUG
            if (user.Id != Constants.XathzUserId) { return Task.CompletedTask; }
#endif

            // Rendering is slow, keep it off the gateway thread
            _ = Task.Run(async () => {
                try {
                    await RunAsync(channel, message, user, alias, who, text);
                } catch (Exception ex) {
                    LoggingManager.Log.Error(ex, $"Command !{alias} failed");
                }
            });

            return Task.CompletedTask;
        }

        /// <summary>
        /// Split a message like "!ttsays hello" into the template and text.
        /// </summary>
        /// <param name="content">Message content.</param>
        /// <param name="alias">Alias that was used, like ttsays.</param>
        /// <param name="who">Template the alias is for.</param>
        /// <param name="text">Text after the alias, empty if there is none.</param>
        /// <returns>True if the message is a says alias.</returns>
        public static bool TryParse(string content, out string alias, out Says.Template who, out string text) {
            alias = null;
            who = default;
            text = null;

            if (string.IsNullOrEmpty(content) || !content.StartsWith(_Prefix, StringComparison.Ordinal)) { return false; }

            string[] parts = content.Substring(_Prefix.Length).Split((char[])null, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0 || !Says.Aliases.TryGetValue(parts[0], out who)) { return false; }

            alias = parts[0].ToLowerInvariant();
            text = parts.Length > 1 ? parts[1].Trim() : string.Empty;
            return true;
        }

        private static async Task RunAsync(SocketTextChannel channel, SocketUserMessage message, SocketGuildUser user, string alias, Says.Template who, string text) {
            // Same checks as /says, blacklisted users are ignored like mention replies
            if (SettingsManager.Configuration.DiscordBlacklist.Any(x => x.Key == channel.Guild.Id & x.Value.Contains(user.Id))) { return; }

            if (!WubbysFunHouse.IsSubscriber(user)) {
                await ReplyAsync(channel, message, SubscriberOnly.Message);
                return;
            }

            if (string.IsNullOrWhiteSpace(text)) {
                await ReplyAsync(channel, message, $"What should they say? `!{alias} <text>`, or use `/says`.");
                return;
            }

            (string cooldownKey, int cooldownSeconds, string statisticsName) = Says.Describe(who);

            string cooldown = Cooldowns.Check(user, cooldownKey, cooldownSeconds, true);
            if (cooldown != null) {
                await ReplyAsync(channel, message, cooldown);
                return;
            }

            using IDisposable typing = channel.EnterTypingState();

            (MemoryStream stream, string fileName) = Says.Render(who, text);
            using (stream) {
                if (stream is null) {
                    LoggingManager.Log.Error($"No file to send for !{alias}: {fileName}");
                    await ReplyAsync(channel, message, "Sorry, that file could not be found.");
                    return;
                }

                IUserMessage sentMessage = await channel.SendFileAsync(stream, fileName, $"● {BetterModuleBase.FormatUser(user)}",
                    allowedMentions: DiscordClient.UserMentionsOnly, messageReference: new MessageReference(message.Id));

                StatisticsManager.InsertCommand(message.CreatedAt.UtcDateTime, channel.Guild.Id, channel.Id, user.Id, user.ToString(), BetterModuleBase.FormatUser(user, nicknameOnly: true),
                    message.Id, message.Content, statisticsName, text, sentMessage.Id);
            }
        }

        private static async Task ReplyAsync(SocketTextChannel channel, SocketUserMessage message, string reply) =>
            await channel.SendMessageAsync(reply, allowedMentions: DiscordClient.UserMentionsOnly, messageReference: new MessageReference(message.Id));

    }

}
