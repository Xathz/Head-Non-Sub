using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Statistics;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [AllowedGuilds(528475747334225925)]
    [RequireContext(ContextType.Guild)]
    public class Yam : ModuleBase<SocketCommandContext> {

        [Command("yamvspotato")]
        public Task YamVsPotatoAsync() {
            ulong reply = Context.Message.Channel.SendFileAsync(Path.Combine(Constants.ContentDirectory, "yamvspotato.png")).Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
            return Task.CompletedTask;
        }

    }

}
