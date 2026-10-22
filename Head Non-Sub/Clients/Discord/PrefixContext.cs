using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using HeadNonSub.Statistics;

namespace HeadNonSub.Clients.Discord {

    /// <summary>
    /// A ! text command being run, with helpers to reply like <see cref="BetterModuleBase"/>.
    /// </summary>
    public sealed class PrefixContext {

        public PrefixContext(SocketTextChannel channel, SocketUserMessage message, SocketGuildUser user, string name, string text) {
            Channel = channel;
            Message = message;
            User = user;
            Name = name;
            Text = text;
        }

        public SocketTextChannel Channel { get; }
        public SocketUserMessage Message { get; }
        public SocketGuildUser User { get; }

        /// <summary>
        /// Name that was used, lowercase, like ttsays.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Everything after the name, empty if there is nothing.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// The command as typed, like !ttsays.
        /// </summary>
        public string Command => $"{PrefixCommands.Prefix}{Name}";

        /// <summary>
        /// Reply to the message. Not tracked in statistics.
        /// </summary>
        /// <param name="message">Contents of the message.</param>
        public async Task<IUserMessage> ReplyAsync(string message) =>
            await Channel.SendMessageAsync(message, allowedMentions: DiscordClient.UserMentionsOnly, messageReference: new MessageReference(Message.Id));

        /// <summary>
        /// Reply to the message with a file and track it in statistics.
        /// </summary>
        /// <param name="stream">The stream of the file to be sent.</param>
        /// <param name="fileName">The name of the attachment.</param>
        /// <param name="message">The message to be sent.</param>
        /// <param name="statisticsName">Command name recorded in statistics, use the same one as the slash command.</param>
        /// <param name="parameters">Additional parameters passed to the command.</param>
        public async Task<IUserMessage> SendFileAsync(Stream stream, string fileName, string message, string statisticsName, string parameters = "") {
            if (stream is null) {
                LoggingManager.Log.Error($"No file to send for {Command}: {fileName}");
                await ReplyAsync("Sorry, that file could not be found.");
                return null;
            }

            IUserMessage sentMessage = await Channel.SendFileAsync(stream, fileName, message, allowedMentions: DiscordClient.UserMentionsOnly, messageReference: new MessageReference(Message.Id));

            TrackStatistics(statisticsName, parameters, sentMessage.Id);
            return sentMessage;
        }

        /// <summary>
        /// Track the command in statistics.
        /// </summary>
        /// <param name="statisticsName">Command name recorded in statistics, use the same one as the slash command.</param>
        /// <param name="parameters">Additional parameters passed to the command.</param>
        /// <param name="replyMessageId">Reply message id.</param>
        public void TrackStatistics(string statisticsName, string parameters = "", ulong? replyMessageId = null) =>
            StatisticsManager.InsertCommand(Message.CreatedAt.UtcDateTime, Channel.Guild.Id, Channel.Id, User.Id, User.ToString(), BetterModuleBase.FormatUser(User, nicknameOnly: true),
                Message.Id, Message.Content, statisticsName, parameters, replyMessageId);

    }

}
