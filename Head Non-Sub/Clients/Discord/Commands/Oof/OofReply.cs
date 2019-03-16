using System.Threading.Tasks;
using Discord.Commands;
using HeadNonSub.Clients.Discord.Attributes;

namespace HeadNonSub.Clients.Discord.Commands.Oof {

    [BlacklistEnforced]
    [RequireContext(ContextType.Guild)]
    public class OofReply : BetterModuleBase {

        [Command("oof")]
        public Task OofOof() => BetterReplyAsync("oof");

        [Command("floof")]
        public Task OofFloof() => BetterSendFileAsync(Cache.GetStream("floof.gif"), "floof.gif", $"● {BetterUserFormat()}");

    }

}
