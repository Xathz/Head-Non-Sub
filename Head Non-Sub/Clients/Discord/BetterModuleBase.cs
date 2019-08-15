using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Extensions;
using HeadNonSub.Statistics;

namespace HeadNonSub.Clients.Discord {

    public abstract class BetterModuleBase : ModuleBase<SocketCommandContext> {

        public BetterModuleBase() { }

        private string DisplayName() {
            if (Context.User is SocketGuildUser contextUser) {
                return !string.IsNullOrWhiteSpace(contextUser.Nickname) ? contextUser.Nickname : "";
            } else {
                return string.Empty;
            }
        }

        /// <summary>
        /// Get a socket user from a user id.
        /// </summary>
        /// <param name="userId">User id to convert to a socket user.</param>
        public SocketGuildUser UserFromUserId(ulong userId) {
            try {
                return Context.Guild.GetUser(userId);
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
                return null;
            }
        }

        /// <summary>
        /// Format a log string containing this context  info.
        /// </summary>
        public string BetterLogFormat() {
            if (Context.IsPrivate) {
                return $"{Context.User.ToString()}/{Context.User.Id} in DM/{Context.Channel.Name} ({Context.Channel.Id})";
            } else {
                if (Context.User is SocketGuildUser guildUser) {
                    return $"{(!string.IsNullOrWhiteSpace(guildUser.Nickname) ? guildUser.Nickname : guildUser.Username)} ({guildUser.ToString()}/{guildUser.Id}) in {Context.Guild.Name}/{Context.Channel.Name} ({Context.Guild.Id}/{Context.Channel.Id})";
                } else {
                    return $"{Context.User.ToString()}/{Context.User.Id} in {Context.Guild.Name}/{Context.Channel.Name} ({Context.Guild.Id}/{Context.Channel.Id})";
                }
            }
        }

        /// <summary>
        /// Formats a user's display name and username.
        /// </summary>
        /// <param name="user">Optional user, if <see langword="null" /> use <see cref="CommandContext.User"/>.</param>
        public string BetterUserFormat(SocketUser user = null, bool nicknameOnly = false, string formatChar = "`") {
            if (user is null) {
                if (Context.User is SocketGuildUser contextUser) {
                    if (nicknameOnly) {
                        return $"{(string.IsNullOrWhiteSpace(contextUser.Nickname) ? contextUser.Username : contextUser.Nickname)}";
                    } else {
                        return $"{(string.IsNullOrWhiteSpace(contextUser.Nickname) ? contextUser.Username : contextUser.Nickname)} {formatChar}{contextUser.ToString()}{formatChar}";
                    }
                }
            }

            if (user is SocketGuildUser guildUser) {
                if (nicknameOnly) {
                    return $"{(string.IsNullOrWhiteSpace(guildUser.Nickname) ? guildUser.Username : guildUser.Nickname)}";
                } else {
                    return $"{(string.IsNullOrWhiteSpace(guildUser.Nickname) ? guildUser.Username : guildUser.Nickname)} {formatChar}{guildUser.ToString()}{formatChar}";
                }
            }

            return $"{Context.User.Username} {formatChar}{Context.User.ToString()}{formatChar}";
        }

        /// <summary>
        /// Sends a message to the source channel.
        /// </summary>
        /// <param name="message">Contents of the message.</param>
        /// <param name="parameters">Additional parameters passed to the command.</param>
        /// <param name="command">Name of who called this method.</param>
        public async Task<IUserMessage> BetterReplyAsync(string message, string parameters = "", [CallerMemberName]string command = "") {
            IUserMessage sentMessage = await Context.Channel.SendMessageAsync(message, false, null, null).ConfigureAwait(false);

            if (!Context.IsPrivate) {
                StatisticsManager.InsertCommand(Context.Message.CreatedAt.DateTime, Context.Guild.Id, Context.Channel.Id,
                    Context.User.Id, Context.User.ToString(), DisplayName(), Context.Message.Id, Context.Message.Content, command, parameters, sentMessage.Id);
            }

            return sentMessage;
        }

        /// <summary>
        /// Sends a message to the source channel.
        /// </summary>
        /// <param name="embed">An embed to be displayed.</param>
        /// <param name="parameters">Additional parameters passed to the command.</param>
        /// <param name="command">Name of who called this method.</param>
        public async Task<IUserMessage> BetterReplyAsync(Embed embed, string parameters = "", [CallerMemberName]string command = "") {
            IUserMessage sentMessage = await Context.Channel.SendMessageAsync(null, false, embed, null).ConfigureAwait(false);

            if (!Context.IsPrivate) {
                StatisticsManager.InsertCommand(Context.Message.CreatedAt.DateTime, Context.Guild.Id, Context.Channel.Id,
                    Context.User.Id, Context.User.ToString(), DisplayName(), Context.Message.Id, Context.Message.Content, command, parameters, sentMessage.Id);
            }

            return sentMessage;
        }

        /// <summary>
        /// Sends a message to the source channel.
        /// </summary>
        /// <param name="message">Contents of the message.</param>
        /// <param name="embed">An embed to be displayed.</param>
        /// <param name="parameters">Additional parameters passed to the command.</param>
        /// <param name="command">Name of who called this method.</param>
        public async Task<IUserMessage> BetterReplyAsync(string message, Embed embed, string parameters = "", [CallerMemberName]string command = "") {
            IUserMessage sentMessage = await Context.Channel.SendMessageAsync(message, false, embed, null).ConfigureAwait(false);

            if (!Context.IsPrivate) {
                StatisticsManager.InsertCommand(Context.Message.CreatedAt.DateTime, Context.Guild.Id, Context.Channel.Id,
                    Context.User.Id, Context.User.ToString(), DisplayName(), Context.Message.Id, Context.Message.Content, command, parameters, sentMessage.Id);
            }

            return sentMessage;
        }

        /// <summary>
        /// Sends a file to the source channel.
        /// </summary>
        /// <param name="stream">The System.IO.Stream of the file to be sent.</param>
        /// <param name="fileName">The name of the attachment.</param>
        /// <param name="message">The message to be sent.</param>
        /// <param name="parameters">Additional parameters passed to the command.</param>
        /// <param name="command">Name of who called this method.</param>
        public async Task<IUserMessage> BetterSendFileAsync(Stream stream, string fileName, string message, string parameters = "", [CallerMemberName]string command = "") {
            IUserMessage sentMessage = await Context.Message.Channel.SendFileAsync(stream, fileName, message).ConfigureAwait(false);

            if (!Context.IsPrivate) {
                StatisticsManager.InsertCommand(Context.Message.CreatedAt.DateTime, Context.Guild.Id, Context.Channel.Id,
                    Context.User.Id, Context.User.ToString(), DisplayName(), Context.Message.Id, Context.Message.Content, command, parameters, sentMessage.Id);
            }

            return sentMessage;
        }

        /// <summary>
        /// Sends a file to the source channel.
        /// </summary>
        /// <param name="filePath">The file path of the file.</param>
        /// <param name="message">The message to be sent.</param>
        /// <param name="parameters">Additional parameters passed to the command.</param>
        /// <param name="command">Name of who called this method.</param>
        public async Task<IUserMessage> BetterSendFileAsync(string filePath, string message, string parameters = "", [CallerMemberName]string command = "") {
            IUserMessage sentMessage = await Context.Message.Channel.SendFileAsync(filePath, message).ConfigureAwait(false);

            if (!Context.IsPrivate) {
                StatisticsManager.InsertCommand(Context.Message.CreatedAt.DateTime, Context.Guild.Id, Context.Channel.Id,
                    Context.User.Id, Context.User.ToString(), DisplayName(), Context.Message.Id, Context.Message.Content, command, parameters, sentMessage.Id);
            }

            return sentMessage;
        }

        /// <summary>
        /// Track an event manually.
        /// </summary>
        /// <param name="replyMessageId">Reply message id.</param>
        /// <param name="parameters">Additional parameters passed to the command.</param>
        /// <param name="command">Name of who called this method.</param>
        public void TrackStatistics(ulong? replyMessageId = null, string parameters = "", [CallerMemberName]string command = "") {
            if (!Context.IsPrivate) {
                StatisticsManager.InsertCommand(Context.Message.CreatedAt.DateTime, Context.Guild.Id, Context.Channel.Id,
                    Context.User.Id, Context.User.ToString(), DisplayName(), Context.Message.Id, Context.Message.Content, command, parameters, replyMessageId);
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

                        await channel.TriggerTypingAsync();

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

                        return await channel.SendMessageAsync(embed: builder.Build()).ConfigureAwait(false);
                    }
                }

                return null;
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
                return null;
            }
        }

        /// <summary>
        /// Log a message to <see cref="WubbysFunHouse.ModLogsChannelId"/>.
        /// </summary>
        public async Task<List<IUserMessage>> LogMessageAsync(string title, string info) {
            try {
                if (Context.Guild.Id == WubbysFunHouse.ServerId) {
                    if (Context.Guild.GetChannel(WubbysFunHouse.ModLogsChannelId) is IMessageChannel channel) {

                        await channel.TriggerTypingAsync();

                        // Max length per-message is 2,000 characters
                        List<string> chunks = info.SplitIntoChunksPreserveNewLines(1998 - title.Length);

                        List<IUserMessage> sentMessages = new List<IUserMessage>();

                        foreach (string chunk in chunks) {
                            sentMessages.Add(await channel.SendMessageAsync($"{title} ```{chunk}```").ConfigureAwait(false));
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
