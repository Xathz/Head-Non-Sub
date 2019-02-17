using System.IO;
using System.Threading.Tasks;
using Discord.Commands;

namespace HeadNonSub.Clients.Discord.Commands {

    // https://discordapp.com/developers/docs/resources/channel#embed-limits

    public class Oof : ModuleBase<SocketCommandContext> {

        [Command("oof")]
        [RequireContext(ContextType.Guild)]
        public Task OofAsync() {
            ulong reply = ReplyAsync("oof").Result.Id;

            DiscordMessageTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            return Task.CompletedTask;
        }

        [Command("floof")]
        [Alias("foof", "woof")]
        [RequireContext(ContextType.Guild)]
        public Task YumAsync() {
            ulong reply = Context.Message.Channel.SendFileAsync(Path.Combine(Constants.ContentDirectory, "floof.gif")).Result.Id;

            DiscordMessageTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            return Task.CompletedTask;
        }

    }

}
