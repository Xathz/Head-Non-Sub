using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace HeadNonSub.Clients.Discord.Commands {

    public class ToolCommands : ModuleBase<SocketCommandContext> {

        // https://discordapp.com/developers/docs/resources/channel#embed-limits

        [Command("ping")]
        [RequireContext(ContextType.Guild)]
        public Task PingAsync() {
            DateTime now = DateTime.Now.ToUniversalTime();

            ulong reply = ReplyAsync($"{now.Subtract(Context.Message.CreatedAt.DateTime).TotalMilliseconds.ToString("N0")}ms" +
                $"```Ping: {Context.Message.CreatedAt.DateTime.ToString(Constants.DateTimeFormat)}{Environment.NewLine}Pong: {now.ToString(Constants.DateTimeFormat)}```").Result.Id;

            DiscordMessageTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);

            return Task.CompletedTask;
        }

        [Command("undo")]
        [RequireContext(ContextType.Guild)]
        public Task UndoAsync() {
            ulong? reply = DiscordMessageTracker.MostRecentReply(Context.Guild.Id, Context.Channel.Id, Context.User.Id);
            ulong? message = DiscordMessageTracker.MostRecentMessage(Context.Guild.Id, Context.Channel.Id, Context.User.Id);

            Context.Message.DeleteAsync().Wait();

            if (reply.HasValue) {
                Context.Channel.DeleteMessageAsync(reply.Value);
                DiscordMessageTracker.Untrack(reply.Value);
            }

            if (message.HasValue) {
                Context.Channel.DeleteMessageAsync(message.Value);
                DiscordMessageTracker.Untrack(message.Value);
            }

            return Task.CompletedTask;
        }

    }

}
