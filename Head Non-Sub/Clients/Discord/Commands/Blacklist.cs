using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Settings;

namespace HeadNonSub.Clients.Discord.Commands {

    [Group("blacklist")]
    [OwnerAdminXathz]
    [RequireContext(ContextType.Guild)]
    public class Blacklist : ModuleBase<SocketCommandContext> {

        [Command("add")]
        public Task BlacklistAddAsync(SocketUser user = null) {
            if (user == null) {
                return ReplyAsync($"You must mention a user to add them to the blacklist.");
            }

            if (SettingsManager.Configuration.DiscordBlacklist.Any(x => x.Key == Context.Guild.Id && x.Value.Contains(user.Id))) {
                return ReplyAsync($"{user.Username} (`{user.Id}`) is already blacklisted.");
            } else {
                if (SettingsManager.Configuration.DiscordBlacklist.ContainsKey(Context.Guild.Id)) {
                    SettingsManager.Configuration.DiscordBlacklist[Context.Guild.Id].Add(user.Id);
                } else {
                    SettingsManager.Configuration.DiscordBlacklist.Add(Context.Guild.Id, new HashSet<ulong>() { user.Id });
                }

                SettingsManager.Save();
                LoggingManager.Log.Info($"{user.Username} ({user.Id}) was added to the blacklist by {Context.User.Username} ({Context.User.Id})");

                return ReplyAsync($"{user.Username} (`{user.Id}`) was added to the blacklist.");
            }
        }

        [Command("remove")]
        public Task BlacklistRemoveAsync(SocketUser user = null) {
            if (user == null) {
                return ReplyAsync($"You must mention a user to remove them from the blacklist.");
            }

            if (SettingsManager.Configuration.DiscordBlacklist.Any(x => x.Key == Context.Guild.Id && x.Value.Contains(user.Id))) {
                SettingsManager.Configuration.DiscordBlacklist[Context.Guild.Id].Remove(user.Id);

                SettingsManager.Save();
                LoggingManager.Log.Info($"{user.Username} ({user.Id}) was removed from the blacklist by {Context.User.Username} ({Context.User.Id})");

                return ReplyAsync($"{user.Username} (`{user.Id}`) was removed from the blacklist.");
            } else {
                return ReplyAsync($"{user.Username} (`{user.Id}`) is not on the blacklist.");
            }
        }

    }

}
