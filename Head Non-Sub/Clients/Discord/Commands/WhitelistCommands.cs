using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Settings;

namespace HeadNonSub.Clients.Discord.Commands {

    [Group("whitelist")]
    [RequireOwner]
    public class WhitelistCommands : ModuleBase<SocketCommandContext> {

        [Command("add")]
        [RequireOwner]
        [RequireContext(ContextType.Guild)]
        public Task WhitelistAddAsync(SocketUser user = null) {
            if (user == null) {
                return ReplyAsync($"You must mention a user to add them to the whitelist.");
            }

            if (SettingsManager.Configuration.DiscordWhitelist.Contains(user.Id)) {
                return ReplyAsync($"{user.Username} (`{user.Id}`) is already whitelisted.");
            } else {
                SettingsManager.Configuration.DiscordWhitelist.Add(user.Id);
                SettingsManager.Save();
                LoggingManager.Log.Info($"{user.Username} ({user.Id}) was added to the whitelist by {Context.User.Username} ({Context.User.Id})");

                return ReplyAsync($"{user.Username} (`{user.Id}`) was added to the whitelist.");
            }
        }

        [Command("remove")]
        [RequireOwner]
        [RequireContext(ContextType.Guild)]
        public Task WhitelistRemoveAsync(SocketUser user = null) {
            if (user == null) {
                return ReplyAsync($"You must mention a user to remove them from the whitelist.");
            }

            if (SettingsManager.Configuration.DiscordWhitelist.Contains(user.Id)) {
                SettingsManager.Configuration.DiscordWhitelist.Remove(user.Id);
                SettingsManager.Save();
                LoggingManager.Log.Info($"{user.Username} ({user.Id}) was removed from the whitelist by {Context.User.Username} ({Context.User.Id})");

                return ReplyAsync($"{user.Username} (`{user.Id}`) was removed from the whitelist.");
            } else {
                return ReplyAsync($"{user.Username} (`{user.Id}`) is not on the whitelist.");
            }
        }

    }

}
