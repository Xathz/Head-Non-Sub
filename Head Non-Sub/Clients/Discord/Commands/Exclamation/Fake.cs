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

        [Command("dating"), Alias("speeddating", "datenight")]
        public async Task Dating() {
            IUserMessage message = await BetterReplyAsync($"Haha {Context.User.Username}, you are alone.");
            IEmote emote = Context.Guild.Emotes.FirstOrDefault(x => x.Id == 461043064979456012);

            if (emote is IEmote) {
                await message.AddReactionAsync(emote);
            }
        }

        [Command("executie")]
        [Cooldown(300)]
        [SubscriberOnly]
        public async Task Executie(SocketUser user = null, [Remainder]string reason = "") {
            await BetterReplyAsync("Executie has been disabled until you idiots use it properly.");
            return;

            if (user == null) {
                await BetterReplyAsync("You must mention a user to executie them.");
                return;
            }

            await BetterReplyAsync($"{user.Mention} you have 10 seconds to say last words before you are executie'd.", parameters: $"{user.ToString()}, {reason}");
            await Task.Delay(10000);

            await ReplyAsync($"10 seconds over. Now executing {user.ToString()} for reason: `{reason}`...");

            await Task.Delay(1000);
            await ReplyAsync("3");
            await Task.Delay(1000);
            await ReplyAsync("2");
            await Task.Delay(1000);
            await ReplyAsync("1");

            await Task.Delay(200);
            IUserMessage message = await ReplyAsync($"The target, `{user.ToString()}` has been hugged and their messages from the past 69 days have also been hugged. :heart:");

            await message.AddReactionAsync(new Emoji("🍆"));
        }

    }

}
