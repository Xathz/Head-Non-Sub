using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Statistics;

namespace HeadNonSub.Clients.Discord {

    public abstract class BetterModuleBase : ModuleBase<SocketCommandContext> {

        public BetterModuleBase() { }

        /// <summary>
        /// Get a socket user from a user id.
        /// </summary>
        /// <param name="userId">User id to convert to a socket user.</param>
        public SocketUser UserFromUserId(ulong userId) {
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
        public string BetterUserFormat(SocketUser user = null, bool useGrave = true) {
            string formatChar = "`";
            if (!useGrave) { formatChar = ""; }

            if (user is null) {
                if (Context.User is SocketGuildUser contextUser) {
                    return $"{(!string.IsNullOrWhiteSpace(contextUser.Nickname) ? contextUser.Nickname : contextUser.Username)} {formatChar}{contextUser.ToString()}{formatChar}";
                }
            }

            if (user is SocketGuildUser guildUser) {
                return $"{(!string.IsNullOrWhiteSpace(guildUser.Nickname) ? guildUser.Nickname : guildUser.Username)} {formatChar}{guildUser.ToString()}{formatChar}";
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
            IUserMessage sentMessage = await Context.Channel.SendMessageAsync(message, false, null, null);

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
            IUserMessage sentMessage = await Context.Channel.SendMessageAsync(null, false, embed, null);

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
            IUserMessage sentMessage = await Context.Channel.SendMessageAsync(message, false, embed, null);

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
            IUserMessage sentMessage = await Context.Message.Channel.SendFileAsync(stream, fileName, message);

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
            IUserMessage sentMessage = await Context.Message.Channel.SendFileAsync(filePath, message);

            if (!Context.IsPrivate) {
                StatisticsManager.InsertCommand(Context.Message.CreatedAt.DateTime, Context.Guild.Id, Context.Channel.Id,
                    Context.User.Id, Context.User.ToString(), DisplayName(), Context.Message.Id, Context.Message.Content, command, parameters, sentMessage.Id);
            }

            return sentMessage;
        }

        /// <summary>
        /// Force track an event manually.
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

        private string DisplayName() {
            if (Context.User is SocketGuildUser contextUser) {
                return (!string.IsNullOrWhiteSpace(contextUser.Nickname) ? contextUser.Nickname : "");
            } else {
                return string.Empty;
            }
        }

    }

}
