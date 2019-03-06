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

        private string User(SocketUser user) {
            if (user is SocketGuildUser guildUser) {
                return $"{(!string.IsNullOrWhiteSpace(guildUser.Nickname) ? guildUser.Nickname : guildUser.Username)} `{guildUser.ToString()}`";
            } else {
                return $"{Context.User.Username} `{Context.User.ToString()}`";
            }
        }

        [Command("add")]
        public Task BlacklistAddAsync(SocketUser user = null) {
            if (user == null) {
                return ReplyAsync($"You must mention a user to add them to the blacklist.");
            }

            if (SettingsManager.Configuration.DiscordBlacklist.Any(x => x.Key == Context.Guild.Id && x.Value.Contains(user.Id))) {
                return ReplyAsync($"{User(user)} is already blacklisted.");
            } else {
                if (SettingsManager.Configuration.DiscordBlacklist.ContainsKey(Context.Guild.Id)) {
                    SettingsManager.Configuration.DiscordBlacklist[Context.Guild.Id].Add(user.Id);
                } else {
                    SettingsManager.Configuration.DiscordBlacklist.Add(Context.Guild.Id, new HashSet<ulong>() { user.Id });
                }

                SettingsManager.Save();
                LoggingManager.Log.Info($"{user.ToString()} ({user.Id}) was added to the blacklist by {Context.User.ToString()} ({Context.User.Id})");

                return ReplyAsync($"{User(user)} was added to the blacklist.");
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
                LoggingManager.Log.Info($"{user.ToString()} ({user.Id}) was removed from the blacklist by {Context.User.ToString()} ({Context.User.Id})");

                return ReplyAsync($"{User(user)} was removed from the blacklist.");
            } else {
                return ReplyAsync($"{User(user)} is not on the blacklist.");
            }
        }

    }

}
