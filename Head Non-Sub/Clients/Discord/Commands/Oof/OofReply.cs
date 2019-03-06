using System.IO;
using System.Threading.Tasks;
using Discord.Commands;

namespace HeadNonSub.Clients.Discord.Commands.Oof {

    [RequireContext(ContextType.Guild)]
    public class OofReply : ModuleBase<SocketCommandContext> {

        [Command("oof")]
        public Task OofAsync() {
            ulong reply = ReplyAsync("oof").Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            return Task.CompletedTask;
        }

        [Command("floof")]
        public Task FloofAsync() {
            ulong reply = Context.Message.Channel.SendFileAsync(Path.Combine(Constants.ContentDirectory, "floof.gif")).Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            return Task.CompletedTask;
        }

    }

}
