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
            if (user == null) {
                await BetterReplyAsync("You must mention a user to executie them.");
                return;
            }

            if (user is SocketGuildUser guildUser) {
                if (guildUser.Roles.Count == 0) {
                    await BetterReplyAsync("You can not executie a non-sub. Executie is for someone you like and no one likes a non-sub.");
                    return;
                }  else if (guildUser.Roles.Any(x => x.Id == WubbysFunHouse.NonSubRoleId)) {
                    await BetterReplyAsync("You can not executie a non-sub. Executie is for someone you like and no one likes a non-sub.");
                    return;
                } else if (guildUser.Roles.Any(x => x.Id == WubbysFunHouse.MutedRoleId)) {
                    await BetterReplyAsync("You can not executie a muted user. They are to be ignored and given no affection.");
                    return;
                }
            } else {
                await BetterReplyAsync($"{user.ToString()} is not a guild member. This is a rare error message so you win I guess...");
                return;
            }

            await BetterReplyAsync($"{user.Mention} you have 10 seconds to say last words before you are executie'd.", parameters: $"{user.ToString()}, {reason}");
            await Task.Delay(10000);

            await ReplyAsync($"10 seconds over. Now executie-ing {user.ToString()} for reason: `{reason}`...");

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
