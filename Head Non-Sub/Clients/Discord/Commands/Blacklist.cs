using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Settings;

namespace HeadNonSub.Clients.Discord.Commands {

    [Group("blacklist")]
    [DiscordStaffOnly]
    [RequireContext(ContextType.Guild)]
    public class Blacklist : BetterModuleBase {

        [Command("add")]
        public async Task BlacklistAdd(SocketUser user = null) {
            if (user == null) {
                await BetterReplyAsync("You must mention a user to add them to the blacklist.");
                return;
            }

            if (SettingsManager.Configuration.DiscordBlacklist.Any(x => x.Key == Context.Guild.Id && x.Value.Contains(user.Id))) {
                await BetterReplyAsync($"{BetterUserFormat(user)} is already blacklisted.", parameters: user.ToString());
            } else {
                if (SettingsManager.Configuration.DiscordBlacklist.ContainsKey(Context.Guild.Id)) {
                    SettingsManager.Configuration.DiscordBlacklist[Context.Guild.Id].Add(user.Id);
                } else {
                    SettingsManager.Configuration.DiscordBlacklist.Add(Context.Guild.Id, new HashSet<ulong>() { user.Id });
                }

                SettingsManager.Save();
                LoggingManager.Log.Info($"{user.ToString()} ({user.Id}) was added to the blacklist by {Context.User.ToString()} ({Context.User.Id})");

                await BetterReplyAsync($"{BetterUserFormat(user)} was added to the blacklist.", parameters: user.ToString());
                await LogMessageEmbedAsync($"User blacklisted from using {Constants.ApplicationName}", user: user);
            }
        }

        [Command("remove")]
        public async Task BlacklistRemove(SocketUser user = null) {
            if (user == null) {
                await BetterReplyAsync("You must mention a user to remove them from the blacklist.");
                return;
            }

            if (SettingsManager.Configuration.DiscordBlacklist.Any(x => x.Key == Context.Guild.Id && x.Value.Contains(user.Id))) {
                SettingsManager.Configuration.DiscordBlacklist[Context.Guild.Id].Remove(user.Id);

                SettingsManager.Save();
                LoggingManager.Log.Info($"{user.ToString()} ({user.Id}) was removed from the blacklist by {Context.User.ToString()} ({Context.User.Id})");

                await BetterReplyAsync($"{BetterUserFormat(user)} was removed from the blacklist.", parameters: user.ToString());
                await LogMessageEmbedAsync($"User removed from the {Constants.ApplicationName} blacklist", user: user);
            } else {
                await BetterReplyAsync($"{BetterUserFormat(user)} is not on the blacklist.", parameters: user.ToString());
            }
        }

        [Command("list")]
        public Task BlacklistList() {
            IEnumerable<ulong> userIds = SettingsManager.Configuration.DiscordBlacklist.Where(x => x.Key == Context.Guild.Id).SelectMany(x => x.Value);

            if (userIds.Count() > 0) {
                IEnumerable<SocketGuildUser> users = Context.Guild.Users.Where(x => userIds.Contains(x.Id));

                return BetterReplyAsync($"Blacklisted users{Environment.NewLine}```{string.Join(Environment.NewLine, users.Select(x => $"{x.ToString()} ({x.Id}) {x.Roles.OrderByDescending(r => r.Position).FirstOrDefault().Name}"))}```");
            } else {
                return BetterReplyAsync("There are no blacklisted users.");
            }
        }

    }

}
