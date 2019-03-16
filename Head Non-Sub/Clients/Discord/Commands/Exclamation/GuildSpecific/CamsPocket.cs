using System.Threading.Tasks;
using Discord.Commands;
using HeadNonSub.Clients.Discord.Attributes;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation.GuildSpecific {

    // Cam's Pocket
    [BlacklistEnforced, AllowedGuilds(528475747334225925)]
    [RequireContext(ContextType.Guild)]
    public class CamsPocket : BetterModuleBase {

        [Command("yamvspotato")]
        public Task YamVsPotato() => BetterSendFileAsync(Cache.GetStream("yamvspotato.png"), "yamvspotato.png", $"● {BetterUserFormat()}");

    }

}
