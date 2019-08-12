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
                await BetterReplyAsync("You must mention a user to executie them.", parameters: $"user null; {reason}");
                return;
            }

            if (user is SocketGuildUser guildUser) {
                if (guildUser.Roles.Count == 0) {
                    await BetterReplyAsync("You can not executie a non-sub. Executie is for someone you like and no one likes a non-sub.", parameters: $"{user.ToString()} ({user.Id}); {reason}");
                    return;
                }  else if (guildUser.Roles.Any(x => x.Id == WubbysFunHouse.NonSubRoleId)) {
                    await BetterReplyAsync("You can not executie a non-sub. Executie is for someone you like and no one likes a non-sub.", parameters: $"{user.ToString()} ({user.Id}); {reason}");
                    return;
                } else if (guildUser.Roles.Any(x => x.Id == WubbysFunHouse.MutedRoleId)) {
                    await BetterReplyAsync("You can not executie a muted user. They are to be ignored and given no affection.", parameters: $"{user.ToString()} ({user.Id}); {reason}");
                    return;
                }
            } else {
                await BetterReplyAsync($"{user.ToString()} is not a guild member. This is a rare error message so you win I guess...", parameters: $"{user.ToString()} ({user.Id}); {reason}");
                return;
            }

            await BetterReplyAsync($"{user.Mention} you have 10 seconds to say last words before you are executie'd.", parameters: $"{user.ToString()} ({user.Id}); {reason}");
            await Task.Delay(10000);

            await BetterReplyAsync($"10 seconds over. Now executie-ing {user.ToString()} for reason: `{reason}`...", parameters: $"{user.ToString()} ({user.Id}); {reason}");

            await Task.Delay(1000);
            await BetterReplyAsync("3", parameters: $"{user.ToString()} ({user.Id}); {reason}");
            await Task.Delay(1000);
            await BetterReplyAsync("2", parameters: $"{user.ToString()} ({user.Id}); {reason}");
            await Task.Delay(1000);
            await BetterReplyAsync("1", parameters: $"{user.ToString()} ({user.Id}); {reason}");

            await Task.Delay(200);
            IUserMessage message = await BetterReplyAsync($"The target, `{user.ToString()}` has been hugged and their messages from the past 69 days have also been hugged. :heart:", parameters: $"{user.ToString()} ({user.Id}); {reason}");

            await message.AddReactionAsync(new Emoji("🍆"));
        }

    }

}
