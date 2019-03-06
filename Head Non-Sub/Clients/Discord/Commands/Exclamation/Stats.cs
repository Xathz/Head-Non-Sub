using System.Threading.Tasks;
using Discord.Commands;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Statistics;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [BlacklistEnforced]
    [RequireContext(ContextType.Guild)]
    public class Stats : ModuleBase<SocketCommandContext> {

        [Command("truecount")]
        public Task TrueCountAsync() {
            ulong count = StatisticsManager.Statistics.Commands(Context.Guild.Id).TimesExecuted("TrueAsync");

            ulong reply = ReplyAsync($"There are {count.ToString("N0")} truths here.").Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
            return Task.CompletedTask;
        }

    }

}
