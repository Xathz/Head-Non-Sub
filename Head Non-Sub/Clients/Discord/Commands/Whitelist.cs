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
    [RequireContext(ContextType.Guild)]
    public class Whitelist : ModuleBase<SocketCommandContext> {

        private string User(SocketUser user) {
            if (user is SocketGuildUser guildUser) {
                return $"{(!string.IsNullOrWhiteSpace(guildUser.Nickname) ? guildUser.Nickname : guildUser.Username)} `{guildUser.ToString()}`";
            } else {
                return $"{Context.User.Username} `{Context.User.ToString()}`";
            }
        }

        [Command("add")]
        public Task WhitelistAddAsync(SocketUser user = null) {
            if (user == null) {
                return ReplyAsync($"You must mention a user to add them to the whitelist.");
            }

            if (SettingsManager.Configuration.DiscordWhitelist.Any(x => x.Key == Context.Guild.Id && x.Value.Contains(user.Id))) {
                return ReplyAsync($"{User(user)} is already whitelisted.");
            } else {
                if (SettingsManager.Configuration.DiscordWhitelist.ContainsKey(Context.Guild.Id)) {
                    SettingsManager.Configuration.DiscordWhitelist[Context.Guild.Id].Add(user.Id);
                } else {
                    SettingsManager.Configuration.DiscordWhitelist.Add(Context.Guild.Id, new HashSet<ulong>() { user.Id });
                }

                SettingsManager.Save();
                LoggingManager.Log.Info($"{user.ToString()} ({user.Id}) was added to the whitelist by {Context.User.ToString()} ({Context.User.Id})");

                return ReplyAsync($"{User(user)} was added to the whitelist.");
            }
        }

        [Command("remove")]
        public Task WhitelistRemoveAsync(SocketUser user = null) {
            if (user == null) {
                return ReplyAsync($"You must mention a user to remove them from the whitelist.");
            }

            if (SettingsManager.Configuration.DiscordWhitelist.Any(x => x.Key == Context.Guild.Id && x.Value.Contains(user.Id))) {
                SettingsManager.Configuration.DiscordWhitelist[Context.Guild.Id].Remove(user.Id);

                SettingsManager.Save();
                LoggingManager.Log.Info($"{user.ToString()} ({user.Id}) was removed from the whitelist by {Context.User.ToString()} ({Context.User.Id})");

                return ReplyAsync($"{User(user)} was removed from the whitelist.");
            } else {
                return ReplyAsync($"{User(user)} is not on the whitelist.");
            }
        }

    }

}
