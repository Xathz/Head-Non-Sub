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
                UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, sentMessage.Id);
                StatisticsManager.InsertCommand(Context.Message.CreatedAt.DateTime, Context.Guild.Id, Context.Channel.Id,
                    Context.User.Id, Context.User.ToString(), DisplayName(), Context.Message.Id, Context.Message.Content, command, parameters);
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
                UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, sentMessage.Id);
                StatisticsManager.InsertCommand(Context.Message.CreatedAt.DateTime, Context.Guild.Id, Context.Channel.Id,
                    Context.User.Id, Context.User.ToString(), DisplayName(), Context.Message.Id, Context.Message.Content, command, parameters);
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
                UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, sentMessage.Id);
                StatisticsManager.InsertCommand(Context.Message.CreatedAt.DateTime, Context.Guild.Id, Context.Channel.Id,
                    Context.User.Id, Context.User.ToString(), DisplayName(), Context.Message.Id, Context.Message.Content, command, parameters);
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
                UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, sentMessage.Id);
                StatisticsManager.InsertCommand(Context.Message.CreatedAt.DateTime, Context.Guild.Id, Context.Channel.Id,
                    Context.User.Id, Context.User.ToString(), DisplayName(), Context.Message.Id, Context.Message.Content, command, parameters);
            }

            return sentMessage;
        }

        /// <summary>
        /// Force track an event manually.
        /// </summary>
        /// <param name="parameters">Additional parameters passed to the command.</param>
        /// <param name="command">Name of who called this method.</param>
        public void TrackStatistics(string parameters = "", [CallerMemberName]string command = "") {
            if (!Context.IsPrivate) {
                StatisticsManager.InsertCommand(Context.Message.CreatedAt.DateTime, Context.Guild.Id, Context.Channel.Id,
                    Context.User.Id, Context.User.ToString(), DisplayName(), Context.Message.Id, Context.Message.Content, command, parameters);
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
