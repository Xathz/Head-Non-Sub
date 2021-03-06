﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Settings;

namespace HeadNonSub.Clients.Discord.Commands {

    [Group("whitelist")]
    [StaffOnly]
    [RequireContext(ContextType.Guild)]
    public class Whitelist : BetterModuleBase {

        [Command("add")]
        public Task WhitelistAdd(SocketUser user = null) {
            if (user == null) {
                return BetterReplyAsync("You must mention a user to add them to the whitelist.");
            }

            if (SettingsManager.Configuration.DiscordWhitelist.Any(x => x.Key == Context.Guild.Id && x.Value.Contains(user.Id))) {
                return BetterReplyAsync($"{BetterUserFormat(user)} is already whitelisted.", parameters: user.ToString());
            } else {
                if (SettingsManager.Configuration.DiscordWhitelist.ContainsKey(Context.Guild.Id)) {
                    SettingsManager.Configuration.DiscordWhitelist[Context.Guild.Id].Add(user.Id);
                } else {
                    SettingsManager.Configuration.DiscordWhitelist.Add(Context.Guild.Id, new HashSet<ulong>() { user.Id });
                }

                SettingsManager.Save();
                LoggingManager.Log.Info($"{user.ToString()} ({user.Id}) was added to the whitelist by {Context.User.ToString()} ({Context.User.Id})");

                return BetterReplyAsync($"{BetterUserFormat(user)} was added to the whitelist.", parameters: user.ToString());
            }
        }

        [Command("remove")]
        public Task WhitelistRemove(SocketUser user = null) {
            if (user == null) {
                return BetterReplyAsync("You must mention a user to remove them from the whitelist.");
            }

            if (SettingsManager.Configuration.DiscordWhitelist.Any(x => x.Key == Context.Guild.Id && x.Value.Contains(user.Id))) {
                SettingsManager.Configuration.DiscordWhitelist[Context.Guild.Id].Remove(user.Id);

                SettingsManager.Save();
                LoggingManager.Log.Info($"{user.ToString()} ({user.Id}) was removed from the whitelist by {Context.User.ToString()} ({Context.User.Id})");

                return BetterReplyAsync($"{BetterUserFormat(user)} was removed from the whitelist.", parameters: user.ToString());
            } else {
                return BetterReplyAsync($"{BetterUserFormat(user)} is not on the whitelist.", parameters: user.ToString());
            }
        }

        [Command("list")]
        public Task WhitelistList() {
            IEnumerable<ulong> userIds = SettingsManager.Configuration.DiscordWhitelist.Where(x => x.Key == Context.Guild.Id).SelectMany(x => x.Value);

            if (userIds.Count() > 0) {
                IEnumerable<SocketGuildUser> users = Context.Guild.Users.Where(x => userIds.Contains(x.Id));

                return BetterReplyAsync($"Whitelisted users{Environment.NewLine}```{string.Join(Environment.NewLine, users.Select(x => $"{x.ToString()} ({x.Id}) {x.Roles.OrderByDescending(r => r.Position).FirstOrDefault().Name}"))}```");
            } else {
                return BetterReplyAsync("There are no whitelisted users.");
            }
        }

    }

}
