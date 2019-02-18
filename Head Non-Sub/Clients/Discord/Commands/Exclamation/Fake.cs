using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    // https://discordapp.com/developers/docs/resources/channel#embed-limits

    public class Fake : ModuleBase<SocketCommandContext> {

        [Command("dating")]
        [Alias("speeddating", "datenight")]
        [RequireContext(ContextType.Guild)]
        public Task DatingAsync() {
            IUserMessage message = ReplyAsync($"Haha {Context.User.Username}, you are alone.").Result;
            IEmote emote = Context.Guild.Emotes.FirstOrDefault(x => x.Id == 461043064979456012);

            if (emote is IEmote) {
                message.AddReactionAsync(emote);
            }

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, message.Id);
            return Task.CompletedTask;
        }

        [Command("executie", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [Cooldown(120)]
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
