using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using HeadNonSub.Statistics;

namespace HeadNonSub.Clients.Discord {

    /// <summary>
    /// A ! text command being run, with helpers to reply like <see cref="BetterModuleBase"/>.
    /// It is also the context for the slash command's preconditions, there is no interaction.
    /// </summary>
    public sealed class PrefixContext : IInteractionContext {

        public PrefixContext(DiscordSocketClient client, SocketTextChannel channel, SocketUserMessage message, SocketGuildUser user, string name, string text) {
            Client = client;
            Channel = channel;
            Message = message;
            User = user;
            Name = name;
            Text = text;
        }

        public DiscordSocketClient Client { get; }
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
        /// True once the command posted its result, the command message is then removed.
        /// </summary>
        public bool Completed { get; private set; }

        IDiscordClient IInteractionContext.Client => Client;
        IGuild IInteractionContext.Guild => Channel.Guild;
        IMessageChannel IInteractionContext.Channel => Channel;
        IUser IInteractionContext.User => User;
        IDiscordInteraction IInteractionContext.Interaction => null;

        /// <summary>
        /// Reply to the message, for errors and hints. Not tracked in statistics and the message is kept.
        /// </summary>
        /// <param name="message">Contents of the message.</param>
        public async Task<IUserMessage> ReplyAsync(string message) =>
            await Channel.SendMessageAsync(message, allowedMentions: DiscordClient.UserMentionsOnly, messageReference: new MessageReference(Message.Id));

        /// <summary>
        /// Post the command's text result and track it in statistics. The command message is removed after, so the result should name the user.
        /// </summary>
        /// <param name="message">Contents of the message.</param>
        /// <param name="statisticsName">Command name recorded in statistics, use the same one as the slash command.</param>
        /// <param name="parameters">Additional parameters passed to the command.</param>
        public async Task<IUserMessage> SendAsync(string message, string statisticsName, string parameters = "") {
            IUserMessage sentMessage = await Channel.SendMessageAsync(message, allowedMentions: DiscordClient.UserMentionsOnly);

            Complete(statisticsName, parameters, sentMessage.Id);
            return sentMessage;
        }

        /// <summary>
        /// Post the command's file result and track it in statistics. The command message is removed after, so the result should name the user.
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

            // Not a reply, the command message is removed once this is sent
            IUserMessage sentMessage = await Channel.SendFileAsync(stream, fileName, message, allowedMentions: DiscordClient.UserMentionsOnly);

            Complete(statisticsName, parameters, sentMessage.Id);
            return sentMessage;
        }

        private void Complete(string statisticsName, string parameters, ulong replyMessageId) {
            Completed = true;
            TrackStatistics(statisticsName, parameters, replyMessageId);
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
