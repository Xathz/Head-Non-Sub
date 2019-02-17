using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace HeadNonSub.Clients.Discord.Commands {

    // https://discordapp.com/developers/docs/resources/channel#embed-limits

    public class Exclamation : ModuleBase<SocketCommandContext> {

        [Command("gimme")]
        [RequireContext(ContextType.Guild)]
        public Task GimmeAsync() {
            ulong reply = ReplyAsync("<:wubbydrugs:361993520040640516>").Result.Id;

            DiscordMessageTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            return Task.CompletedTask;
        }

        [Command("rnk")]
        [RequireContext(ContextType.Guild)]
        public Task RnkAsync() {
            ulong reply = Context.Message.Channel.SendFileAsync(Path.Combine(Constants.ContentDirectory, "rnk.png")).Result.Id;

            DiscordMessageTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            return Task.CompletedTask;
        }

        [Command("executie", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        public Task ExecuteAsync(SocketUser user = null, [Remainder]string reason = "") {

            ReplyAsync($"{user.Mention} you have 10 seconds to say last words before you are executie'd.").Wait();
            Task.Delay(10000).Wait();

            ReplyAsync($"10 seconds over. Now executing {user.ToString()} for reason: `{reason}`...").Wait();

            Task.Delay(1000).Wait();
            ReplyAsync("3");
            Task.Delay(1000).Wait();
            ReplyAsync("2");
            Task.Delay(1000).Wait();
            ReplyAsync("1").Wait();

            Task.Delay(200).Wait();
            IUserMessage message = ReplyAsync($"The target, `{user.ToString()}` has been hugged and their messages from the past 69 days have also been hugged. :heart:").Result;

            message.AddReactionAsync(new Emoji("🍆"));

            return Task.CompletedTask;
        }

    }

}
