using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Settings;

namespace HeadNonSub.Clients.Discord.Commands {

    [Group("whitelist")]
    [OwnerAdminXathz]
    public class Whitelist : ModuleBase<SocketCommandContext> {

        [Command("add")]
        [OwnerAdminXathz]
        [RequireContext(ContextType.Guild)]
        public Task WhitelistAddAsync(SocketUser user = null) {
            if (user == null) {
                return ReplyAsync($"You must mention a user to add them to the whitelist.");
            }

            if (SettingsManager.Configuration.DiscordWhitelist.Any(x => x.Key == Context.Guild.Id && x.Value.Contains(user.Id))) {
                return ReplyAsync($"{user.Username} (`{user.Id}`) is already whitelisted.");
            } else {
                if (SettingsManager.Configuration.DiscordWhitelist.ContainsKey(Context.Guild.Id)) {
                    SettingsManager.Configuration.DiscordWhitelist[Context.Guild.Id].Add(user.Id);
                } else {
                    SettingsManager.Configuration.DiscordWhitelist.Add(Context.Guild.Id, new HashSet<ulong>() { user.Id });
                }

                SettingsManager.Save();
                LoggingManager.Log.Info($"{user.Username} ({user.Id}) was added to the whitelist by {Context.User.Username} ({Context.User.Id})");

                return ReplyAsync($"{user.Username} (`{user.Id}`) was added to the whitelist.");
            }
        }

        [Command("remove")]
        [OwnerAdminXathz]
        [RequireContext(ContextType.Guild)]
        public Task WhitelistRemoveAsync(SocketUser user = null) {
            if (user == null) {
                return ReplyAsync($"You must mention a user to remove them from the whitelist.");
            }

            if (SettingsManager.Configuration.DiscordWhitelist.Any(x => x.Key == Context.Guild.Id && x.Value.Contains(user.Id))) {
                SettingsManager.Configuration.DiscordWhitelist[Context.Guild.Id].Remove(user.Id);

                SettingsManager.Save();
                LoggingManager.Log.Info($"{user.Username} ({user.Id}) was removed from the whitelist by {Context.User.Username} ({Context.User.Id})");

                return ReplyAsync($"{user.Username} (`{user.Id}`) was removed from the whitelist.");
            } else {
                return ReplyAsync($"{user.Username} (`{user.Id}`) is not on the whitelist.");
            }
        }

    }

}
