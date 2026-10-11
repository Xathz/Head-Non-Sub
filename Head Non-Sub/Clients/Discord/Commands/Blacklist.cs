using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Settings;

namespace HeadNonSub.Clients.Discord.Commands {

    [Group("blacklist", "Block users from using the bot")]
    [DefaultMemberPermissions(GuildPermission.ManageMessages)]
    [DiscordStaffOnly]
    [CommandContextType(InteractionContextType.Guild)]
    public class Blacklist : BetterModuleBase {

        /// <summary>
        /// Get the current configuration (for commands that modify/read config).
        /// </summary>
        protected static Configuration GetConfiguration() => SettingsManager.Configuration;

        [SlashCommand("add", "Add a user to the blacklist")]
        public async Task BlacklistAdd([Summary("user", "User to blacklist")] SocketGuildUser user) {
            if (WubbysFunHouse.IsDiscordStaff(user)) {
                await EphemeralReplyAsync("You can not blacklist a discord staff member.");
                return;
            }

            if (GetConfiguration().DiscordBlacklist.Any(x => x.Key == Context.Guild.Id && x.Value.Contains(user.Id))) {
                await BetterReplyAsync($"{BetterUserFormat(user)} is already blacklisted.", parameters: $"{user} ({user.Id})");
            } else {
                if (GetConfiguration().DiscordBlacklist.TryGetValue(Context.Guild.Id, out HashSet<ulong> value)) {
                    value.Add(user.Id);
                } else {
                    GetConfiguration().DiscordBlacklist.Add(Context.Guild.Id, new HashSet<ulong>() { user.Id });
                }

                SettingsManager.Save();

                LoggingManager.Log.Info($"{user} ({user.Id}) was added to the blacklist by {Context.User} ({Context.User.Id})");
                await BetterReplyAsync($"{BetterUserFormat(user)} was added to the blacklist.", parameters: $"{user} ({user.Id})");
                await LogMessageEmbedAsync($"User blacklisted from using {Constants.ApplicationName}", user: user);
            }
        }

        [SlashCommand("remove", "Remove a user from the blacklist")]
        public async Task BlacklistRemove([Summary("user", "User to remove from the blacklist")] SocketGuildUser user) {
            if (GetConfiguration().DiscordBlacklist.Any(x => x.Key == Context.Guild.Id && x.Value.Contains(user.Id))) {
                GetConfiguration().DiscordBlacklist[Context.Guild.Id].Remove(user.Id);
                SettingsManager.Save();

                LoggingManager.Log.Info($"{user} ({user.Id}) was removed from the blacklist by {Context.User} ({Context.User.Id})");
                await BetterReplyAsync($"{BetterUserFormat(user)} was removed from the blacklist.", parameters: $"{user} ({user.Id})");
                await LogMessageEmbedAsync($"User removed from the {Constants.ApplicationName} blacklist", user: user);
            } else {
                await BetterReplyAsync($"{BetterUserFormat(user)} is not on the blacklist.", parameters: $"{user} ({user.Id})");
            }
        }

        [SlashCommand("list", "List blacklisted users")]
        public async Task BlacklistList() {
            IEnumerable<ulong> userIds = GetConfiguration().DiscordBlacklist.Where(x => x.Key == Context.Guild.Id).SelectMany(x => x.Value);

            if (userIds.Any()) {
                IEnumerable<SocketGuildUser> users = Context.Guild.Users.Where(x => userIds.Contains(x.Id));
                await BetterReplyAsync($"Blacklisted users{Environment.NewLine}```{string.Join(Environment.NewLine, users.Select(x => $"{x} ({x.Id}) {x.Roles.OrderByDescending(r => r.Position).FirstOrDefault().Name}"))}```");
            } else {
                await BetterReplyAsync("There are no blacklisted users.");
            }
        }

    }

}
