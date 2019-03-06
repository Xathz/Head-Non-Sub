using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Statistics;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [RequireContext(ContextType.Guild)]
    public class Fake : ModuleBase<SocketCommandContext> {

        [Command("dating")]
        [Alias("speeddating", "datenight")]
        public Task DatingAsync() {
            IUserMessage message = ReplyAsync($"Haha {Context.User.Username}, you are alone.").Result;
            IEmote emote = Context.Guild.Emotes.FirstOrDefault(x => x.Id == 461043064979456012);

            if (emote is IEmote) {
                message.AddReactionAsync(emote);
            }

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, message.Id);
            StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
            return Task.CompletedTask;
        }

        [Command("executie")]
        [Cooldown(300)]
        public Task ExecutieAsync(SocketUser user = null, [Remainder]string reason = "") {

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

            StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
            return Task.CompletedTask;
        }

    }

}
