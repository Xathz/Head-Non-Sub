using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [BlacklistEnforced]
    [RequireContext(ContextType.Guild)]
    public class Fake : BetterModuleBase {

        [Command("dating")]
        [Alias("speeddating", "datenight")]
        public Task Dating() {
            IUserMessage message = BetterReplyAsync($"Haha {Context.User.Username}, you are alone.").Result;
            IEmote emote = Context.Guild.Emotes.FirstOrDefault(x => x.Id == 461043064979456012);

            if (emote is IEmote) {
                message.AddReactionAsync(emote);
            }

            return Task.CompletedTask;
        }

        [Command("executie")]
        [Cooldown(300)]
        public Task Executie(SocketUser user = null, [Remainder]string reason = "") {

            BetterReplyAsync($"{user.Mention} you have 10 seconds to say last words before you are executie'd.").Wait();
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
