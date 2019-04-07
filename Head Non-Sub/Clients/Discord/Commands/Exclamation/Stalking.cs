using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Database;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [BlacklistEnforced, SubscriberOnly]
    [RequireContext(ContextType.Guild)]
    public class Stalking : BetterModuleBase {

        [Command("stalk")]
        public async Task Stalk(SocketUser user = null) {
            if (user == null) {
                await BetterReplyAsync("You must provide a user to stalk.");
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            if (DatabaseManager.Stalking.Insert(Context.Guild.Id, Context.User.Id, user.Id)) {
                Cache.SetStalkers(DatabaseManager.Stalking.GetAll());

                await BetterReplyAsync($"{Context.User.Mention} You are now stalking {BetterUserFormat(user)}. You will be messaged directly when the user does various... things :eyes:");
            } else {
                await BetterReplyAsync($"{Context.User.Mention} You are already stalking {BetterUserFormat(user)}.");
            }
        }

        [Command("stopstalking"), Alias("stopstalk")]
        public async Task StopStalking(SocketUser user = null) {
            if (user == null) {
                await BetterReplyAsync("You must provide a user to stop stalking.");
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            if (DatabaseManager.Stalking.Delete(Context.Guild.Id, Context.User.Id, user.Id)) {
                Cache.SetStalkers(DatabaseManager.Stalking.GetAll());

                await BetterReplyAsync($"{Context.User.Mention} You stopped stalking {BetterUserFormat(user)}.");
            } else {
                await BetterReplyAsync($"{Context.User.Mention} You are not stalking {BetterUserFormat(user)}.");
            }
        }

    }

}
