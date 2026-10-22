using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using HeadNonSub.Extensions;
using HeadNonSub.Statistics;

namespace HeadNonSub.Clients.Discord {

    public abstract class BetterModuleBase : InteractionModuleBase<SocketInteractionContext> {

        protected BetterModuleBase() { }

        private string DisplayName() => Context.User is SocketGuildUser contextUser ? contextUser.DisplayName : Context.User.GlobalName ?? Context.User.Username;

        /// <summary>
        /// The slash command as text, for statistics.
        /// </summary>
        private string CommandText() {
            if (Context.Interaction is SocketSlashCommand slashCommand) {
                StringBuilder builder = new StringBuilder($"/{slashCommand.Data.Name}");
                AppendOptions(builder, slashCommand.Data.Options);
                return builder.ToString();
            }

            return string.Empty;
        }

        private static void AppendOptions(StringBuilder builder, IEnumerable<SocketSlashCommandDataOption> options) {
            foreach (SocketSlashCommandDataOption option in options) {
                if (option.Type == ApplicationCommandOptionType.SubCommand || option.Type == ApplicationCommandOptionType.SubCommandGroup) {
                    builder.Append($" {option.Name}");
                    AppendOptions(builder, option.Options);
                } else {
                    builder.Append($" {option.Name}:{option.Value}");
                }
            }
        }

        /// <summary>
        /// Formats a user's display name and username.
        /// </summary>
        /// <param name="user">Optional user, if <see langword="null" /> use the user running the command.</param>
        /// <param name="nicknameOnly">Only return the display name.</param>
        /// <param name="formatChar">Character to wrap the username in.</param>
        public string BetterUserFormat(IUser user = null, bool nicknameOnly = false, string formatChar = "`") => FormatUser(user ?? Context.User, nicknameOnly, formatChar);

        /// <summary>
        /// Formats a user's display name and username, for code outside a command module.
        /// </summary>
        /// <param name="user">User to format.</param>
        /// <param name="nicknameOnly">Only return the display name.</param>
        /// <param name="formatChar">Character to wrap the username in.</param>
        public static string FormatUser(IUser user, bool nicknameOnly = false, string formatChar = "`") {
            // Server nickname, then global display name, then username
            string displayName = user is SocketGuildUser guildUser ? guildUser.DisplayName : user.GlobalName ?? user.Username;

            return nicknameOnly ? displayName : $"{displayName} {formatChar}{user}{formatChar}";
        }

        /// <summary>
        /// Reply to the command, or send a follow up if it was already replied to or deferred.
        /// </summary>
        /// <param name="message">Contents of the message.</param>
        /// <param name="parameters">Additional parameters passed to the command.</param>
        /// <param name="command">Name of who called this method.</param>
        public Task<IUserMessage> BetterReplyAsync(string message, string parameters = "", [CallerMemberName] string command = "") => SendAsync(message, null, parameters, command);

        /// <summary>
        /// Reply to the command, or send a follow up if it was already replied to or deferred.
        /// </summary>
        /// <param name="embed">An embed to be displayed.</param>
        /// <param name="parameters">Additional parameters passed to the command.</param>
        /// <param name="command">Name of who called this method.</param>
        public Task<IUserMessage> BetterReplyAsync(Embed embed, string parameters = "", [CallerMemberName] string command = "") => SendAsync(null, embed, parameters, command);

        /// <summary>
        /// Reply only the user running the command can see. Not tracked in statistics.
        /// </summary>
        /// <param name="message">Contents of the message.</param>
        public Task EphemeralReplyAsync(string message) => SlashCommands.EphemeralReplyAsync(Context.Interaction, message);

        /// <summary>
        /// Reply to the command with a file, or send it as a follow up if it was already replied to or deferred.
        /// </summary>
        /// <param name="stream">The stream of the file to be sent.</param>
        /// <param name="fileName">The name of the attachment.</param>
        /// <param name="message">The message to be sent.</param>
        /// <param name="parameters">Additional parameters passed to the command.</param>
        /// <param name="command">Name of who called this method.</param>
        public async Task<IUserMessage> BetterSendFileAsync(Stream stream, string fileName, string message, string parameters = "", [CallerMemberName] string command = "") {
            if (stream is null) {
                LoggingManager.Log.Error($"No file to send for {command}: {fileName}");
                await EphemeralReplyAsync("Sorry, that file could not be found.");
                return null;
            }

            IUserMessage sentMessage;

            if (Context.Interaction.HasResponded) {
                sentMessage = await FollowupWithFileAsync(stream, fileName, message, allowedMentions: DiscordClient.UserMentionsOnly);
            } else {
                await RespondWithFileAsync(stream, fileName, message, allowedMentions: DiscordClient.UserMentionsOnly);
                sentMessage = await GetOriginalResponseAsync();
            }

            TrackStatistics(sentMessage.Id, parameters, command);
            return sentMessage;
        }

        private async Task<IUserMessage> SendAsync(string message, Embed embed, string parameters, string command) {
            IUserMessage sentMessage;

            if (Context.Interaction.HasResponded) {
                sentMessage = await FollowupAsync(message, embed: embed, allowedMentions: DiscordClient.UserMentionsOnly);
            } else {
                await RespondAsync(message, embed: embed, allowedMentions: DiscordClient.UserMentionsOnly);
                sentMessage = await GetOriginalResponseAsync();
            }

            TrackStatistics(sentMessage.Id, parameters, command);
            return sentMessage;
        }

        /// <summary>
        /// Track an event manually.
        /// </summary>
        /// <param name="replyMessageId">Reply message id.</param>
        /// <param name="parameters">Additional parameters passed to the command.</param>
        /// <param name="command">Name of who called this method.</param>
        public void TrackStatistics(ulong? replyMessageId = null, string parameters = "", [CallerMemberName] string command = "") {
            if (Context.Guild is SocketGuild guild) {
                StatisticsManager.InsertCommand(Context.Interaction.CreatedAt.UtcDateTime, guild.Id, Context.Channel.Id,
                    Context.User.Id, Context.User.ToString(), DisplayName(), Context.Interaction.Id, CommandText(), command, parameters, replyMessageId);
            }
        }

        /// <summary>
        /// Log a message embed to <see cref="WubbysFunHouse.ModLogsChannelId"/>.
        /// </summary>
        /// <param name="title">Log message title line.</param>
        /// <param name="info">General event information.</param>
        /// <param name="user">User the action was performed on if applicable.</param>
        public async Task<IUserMessage> LogMessageEmbedAsync(string title, string info = "", IUser user = null) {
            try {
                if (Context.Guild.Id == WubbysFunHouse.ServerId) {
                    if (Context.Guild.GetChannel(WubbysFunHouse.ModLogsChannelId) is IMessageChannel channel) {
                        EmbedBuilder builder = new EmbedBuilder() {
                            Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                            Title = title
                        };

                        builder.AddField("Moderator", Context.User.Mention, true);

                        if (user is IUser) {
                            builder.AddField("User", user.Mention, true);
                        }

                        if (!string.IsNullOrWhiteSpace(info)) {
                            builder.AddField("Info", info, true);
                        }

                        return await channel.SendMessageAsync(embed: builder.Build(), allowedMentions: DiscordClient.UserMentionsOnly);
                    }
                }

                return null;
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
                return null;
            }
        }

        /// <summary>
        /// Log a message to <see cref="WubbysFunHouse.UserLogsChannelId"/>.
        /// </summary>
        public async Task<List<IUserMessage>> LogUserMessageAsync(string title, string info) {
            try {
                if (Context.Guild.Id == WubbysFunHouse.ServerId) {
                    if (Context.Guild.GetChannel(WubbysFunHouse.UserLogsChannelId) is IMessageChannel channel) {

                        // Max length per-message is 2,000 characters
                        List<string> chunks = info.SanitizeForMarkdown().SplitIntoChunksPreserveNewLines(1998 - title.Length);

                        List<IUserMessage> sentMessages = new List<IUserMessage>();

                        foreach (string chunk in chunks) {
                            sentMessages.Add(await channel.SendMessageAsync($"{title} ```{chunk}```", allowedMentions: DiscordClient.UserMentionsOnly));
                        }

                        return sentMessages;
                    }
                }

                return new List<IUserMessage>();
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
                return new List<IUserMessage>();
            }
        }

    }

}
