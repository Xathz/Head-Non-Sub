using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;
using Humanizer;

namespace HeadNonSub.Clients.Discord.Commands {

    [Group("rr")]
    [DiscordStaffOnly]
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(GuildPermission.ManageMessages, ErrorMessage = "I do not have the `Manage Messages` permission, raid recovery can not be used.")]
    [RequireBotPermission(GuildPermission.ManageChannels, ErrorMessage = "I do not have the `Manage Channels` permission, raid recovery can not be used.")]
    public class RaidRecovery : BetterModuleBase {

        private readonly int _MessageThreshold = 3;

        [Command("enable")]
        public async Task EnableRaidRecovery() {
            if (Context.Channel is SocketTextChannel channel) {
                if (!RaidRecoveryTracker.Track(Context.Channel.Id, Context.User.Id, channel.SlowModeInterval)) {
                    ulong? startedBy = RaidRecoveryTracker.StartedBy(Context.Channel.Id);

                    if (startedBy.HasValue) {
                        await BetterReplyAsync($"Raid recovery system is already enabled. Enabled by {BetterUserFormat(UserFromUserId(startedBy.Value))}.");
                        return;
                    } else {
                        await BetterReplyAsync($"Raid recovery system is already enabled.");
                        return;
                    }
                }

                await channel.ModifyAsync(x => { x.SlowModeInterval = 120; });
            } else {
                await BetterReplyAsync("Invalid channel, not a text only channel.");
                return;
            }

            EmbedBuilder builder = new EmbedBuilder() {
                Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                Title = "Raid Recovery System Enabled",
                Description = "This channel is now in slow mode, normal users can only send one message every two minutes."
            };

            builder.AddField("Commands", string.Join(Environment.NewLine, new string[] {
                $"Commands are used `@{Context.Guild.CurrentUser.Username} rr <command>`",
                "`disable` Disable raid recovery system",
                "`list <minutes>` List suspected users from the past # minutes",
                "`clean <minutes>` Remove messages from suspected users from the past # minutes",
                "`ban <minutes>` Ban suspected users in the past # minutes, will prompt for confirmation" }));

            LoggingManager.Log.Warn($"Raid recovery system enabled. {BetterLogFormat()}");
            await BetterReplyAsync(builder.Build());
            await LogMessageAsync("Raid recovery system enabled");
        }

        [Command("disable")]
        public async Task DisableRaidRecovery() {
            if (!RaidRecoveryTracker.Exists(Context.Channel.Id)) {
                await BetterReplyAsync($"The raid recovery system is not enabled. Use `@{Context.Guild.CurrentUser.Username} rr enable` to enable.");
                return;
            }

            int previousSlowModeInterval = RaidRecoveryTracker.PreviousSlowModeInterval(Context.Channel.Id);
            if (Context.Channel is SocketTextChannel channel) {
                await channel.ModifyAsync(x => { x.SlowModeInterval = previousSlowModeInterval; });
            }

            RaidRecoveryTracker.Untrack(Context.Channel.Id);

            EmbedBuilder builder = new EmbedBuilder() {
                Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                Title = "Raid Recovery System Disabled",
                Description = "This channel has returned to normal."
            };

            LoggingManager.Log.Warn($"Raid recovery system disabled. {BetterLogFormat()}");
            await BetterReplyAsync(builder.Build());
            await LogMessageAsync("Raid recovery system disabled");
        }

        [Command("list")]
        public async Task ListRaidRecovery(int minutes = 10) {
            if (!RaidRecoveryTracker.Exists(Context.Channel.Id)) {
                await BetterReplyAsync($"The raid recovery system is not enabled. Use `@{Context.Guild.CurrentUser.Username} rr enable` to enable.");
                return;
            }

            if (minutes == 0 || minutes > 20160) {
                await BetterReplyAsync("Must be between 1 and 20160 (2 weeks). Rarely should this be greater than 10 unless a raid went unnoticed for longer.", minutes.ToString());
                return;
            }

            IUserMessage message = await BetterReplyAsync(embed: LoadingEmbed("Generating list"), parameters: minutes.ToString());

            ConcurrentBag<IMessage> messages = await GetMessagesAsync();
            List<IGrouping<IUser, IMessage>> messagesPerUser = messages.OrderByDescending(x => x.CreatedAt)
                .Where(x => x.CreatedAt > DateTime.UtcNow.AddMinutes(-minutes))
                .Where(x => IsSuspected(x.Author))
                .GroupBy(x => x.Author)
                .Where(x => x.Count() > _MessageThreshold)
                .OrderByDescending(x => x.Count())
                .ToList();

            if (messagesPerUser.Count > 0) {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine($"{messagesPerUser.Count} different users sent at least {_MessageThreshold} messages within the last {minutes.Minutes().Humanize(3)}.");

                builder.AppendLine("```");
                foreach (IGrouping<IUser, IMessage> user in messagesPerUser) {
                    builder.Append($"{user.Key.ToString()}: {user.Count()}; ");
                }
                builder.AppendLine("```");

                await message.ModifyAsync(x => { x.Embed = null; x.Content = builder.ToString(); });
            } else {
                await message.ModifyAsync(x => { x.Embed = null; x.Content = $"There are no suspected users from the past {minutes.Minutes().Humanize(3)}."; });
            }
        }

        [Command("clean")]
        public async Task CleanRaidRecovery(int minutes = 10) {
            if (!RaidRecoveryTracker.Exists(Context.Channel.Id)) {
                await BetterReplyAsync($"The raid recovery system is not enabled. Use `@{Context.Guild.CurrentUser.Username} rr enable` to enable.");
                return;
            }

            if (minutes == 0 || minutes > 20160) {
                await BetterReplyAsync("Must be between 1 and 20160 (2 weeks). Rarely should this be greater than 10 unless a raid went unnoticed for longer.", minutes.ToString());
                return;
            }

            IUserMessage message = await BetterReplyAsync(embed: LoadingEmbed("Deleting messages", "Staff messages will be preserved."), parameters: minutes.ToString());

            SocketTextChannel channel = Context.Channel as SocketTextChannel;

            ConcurrentBag<IMessage> messages = await GetMessagesAsync();
            IEnumerable<IMessage> messagesToDelete = messages.OrderByDescending(x => x.CreatedAt)
                .Where(x => x.CreatedAt > DateTime.UtcNow.AddMinutes(-minutes))
                .Where(x => IsSuspected(x.Author));

            await channel.DeleteMessagesAsync(messagesToDelete);

            await message.ModifyAsync(x => { x.Embed = null; x.Content = $"Deleted {messagesToDelete.Count()} messages from the past {minutes.Minutes().Humanize(3)}."; });   
            await LogMessageAsync("Raid recovery system", $"Deleted {messagesToDelete.Count()} messages from the past {minutes.Minutes().Humanize(3)}.");
        }

        [Command("ban")]
        [RequireBotPermission(GuildPermission.BanMembers, ErrorMessage = "I do not have the `Ban Members` permission.")]
        public async Task BanRaidRecovery(int minutes = 10, [Remainder]string banToken = "") {
            if (!RaidRecoveryTracker.Exists(Context.Channel.Id)) {
                await BetterReplyAsync($"The raid recovery system is not enabled. Use `@{Context.Guild.CurrentUser.Username} rr enable` to enable.");
                return;
            }

            if (minutes == 0 || minutes > 20160) {
                await BetterReplyAsync("Must be between 1 and 20160 (2 weeks). Rarely should this be greater than 10 unless a raid went unnoticed for longer.", minutes.ToString());
                return;
            }

            if (!string.IsNullOrWhiteSpace(banToken)) {
                if (RaidRecoveryTracker.ValidateBanToken(Context.Channel.Id, banToken)) {
                    IUserMessage banningMessage = await BetterReplyAsync(embed: LoadingEmbed("Banning users"), parameters: minutes.ToString());
                    HashSet<ulong> usersIdsToBan = RaidRecoveryTracker.UsersToBan(Context.Channel.Id);

                    try {
                        foreach (ulong user in usersIdsToBan) {
                            await Context.Guild.AddBanAsync(user, 1, $"Banned by '{Context.User.ToString()}' using the bot '{Context.Guild.CurrentUser.Username}' raid recovery system on {DateTime.UtcNow.ToString("o")}");
                        }
                    } catch { }

                    await banningMessage.ModifyAsync(x => { x.Embed = null; x.Content = $"Banned {usersIdsToBan.Count} users."; });
                    await LogMessageAsync("Raid recovery system", $"Banned {usersIdsToBan.Count} users.{Environment.NewLine}{Environment.NewLine}{string.Join(", ", usersIdsToBan)}");
                    return;
                } else {
                    await BetterReplyAsync("Invalid ban token.");
                    return;
                }
            }

            IUserMessage message = await BetterReplyAsync(embed: LoadingEmbed("Generating list of users to ban"), parameters: minutes.ToString());

            ConcurrentBag<IMessage> messages = await GetMessagesAsync();
            IEnumerable<IUser> usersToBan = messages.OrderByDescending(x => x.CreatedAt)
                .Where(x => x.CreatedAt > DateTime.UtcNow.AddMinutes(-minutes))
                .Where(x => IsSuspected(x.Author))
                .GroupBy(x => x.Author)
                .Where(x => x.Count() > _MessageThreshold)
                .Select(x => x.Key)
                .Distinct();

            if (usersToBan.Count() > 0) {
                RaidRecoveryTracker.AddUsersToBan(Context.Channel.Id, usersToBan.Select(x => x.Id).ToHashSet());

                StringBuilder builder = new StringBuilder();
                builder.AppendLine($"The following {usersToBan.Count()} users will be banned.");

                builder.AppendLine("```");
                foreach (IUser banUser in usersToBan) {
                    builder.Append($"{banUser.ToString()}; ");
                }
                builder.AppendLine("```");

                await message.ModifyAsync(x => { x.Embed = null; x.Content = builder.ToString(); });

                await BetterReplyAsync($"**Ban confirmation.** Selected time frame: {minutes.Minutes().Humanize(3)}; Token: `{RaidRecoveryTracker.BanToken(Context.Channel.Id)}`{Environment.NewLine}{Environment.NewLine}" +
                    $"Type `@{Context.Guild.CurrentUser.Username} rr skip @User` to remove them from the ban list.{Environment.NewLine}" +
                    $"Type `@{Context.Guild.CurrentUser.Username} rr ban {minutes} {RaidRecoveryTracker.BanToken(Context.Channel.Id)}` to ban the users above.");
            } else {
                await message.ModifyAsync(x => { x.Embed = null; x.Content = $"There are no suspected users to ban from the past {minutes.Minutes().Humanize(3)}."; });
            }
        }

        [Command("skip")]
        [RequireBotPermission(GuildPermission.BanMembers, ErrorMessage = "I do not have the `Ban Members` permission.")]
        public async Task SkipRaidRecovery(SocketUser user = null) {
            if (!RaidRecoveryTracker.Exists(Context.Channel.Id)) {
                await BetterReplyAsync($"The raid recovery system is not enabled. Use `@{Context.Guild.CurrentUser.Username} rr enable` to enable.");
                return;
            }

            if (user == null) {
                await BetterReplyAsync("You must provide a user to skip them from being banned.");
                return;
            }

            if (RaidRecoveryTracker.SkipUserToBan(Context.Channel.Id, user.Id)) {
                await BetterReplyAsync($"{BetterUserFormat(user)} will be skipped and **not** banned.");
                await LogMessageAsync("Raid recovery system", $"{BetterUserFormat(user)} will be skipped and **not** banned.");

            } else {
                await BetterReplyAsync($"{BetterUserFormat(user)} was not suspected and will **not** be banned.");
            }
        }

        private async Task<ConcurrentBag<IMessage>> GetMessagesAsync() {
            if (Context.Channel is SocketTextChannel channel) {
                ConcurrentBag<IMessage> channelMessages = new ConcurrentBag<IMessage>();
                channel.CachedMessages.ToList().ForEach(x => {
                    if (!channelMessages.Any(m => m.Id == x.Id)) {
                        channelMessages.Add(x);
                    }
                });

                if (channelMessages.Count > 0) {
                    IAsyncEnumerable<IMessage> messages = channel.GetMessagesAsync(channelMessages.OrderBy(x => x.CreatedAt).FirstOrDefault(), Direction.Before, 1000).Flatten();
                    await messages.ForEachAsync(x => {
                        if (!channelMessages.Any(m => m.Id == x.Id)) {
                            channelMessages.Add(x);
                        }
                    });
                } else {
                    IAsyncEnumerable<IMessage> messages = channel.GetMessagesAsync(1000).Flatten();
                    await messages.ForEachAsync(x => {
                        if (!channelMessages.Any(m => m.Id == x.Id)) {
                            channelMessages.Add(x);
                        }
                    });
                }

                return channelMessages;
            } else {
                return new ConcurrentBag<IMessage>();
            }
        }

        private bool IsSuspected(IUser user) {
            if (user is SocketGuildUser guildUser) {
                if (guildUser.Roles.Count == 0) {
                    return true;
                } else if (guildUser.IsBot) {
                    return false;
                } else if (guildUser.Roles.Any(r => r.Permissions.Administrator)) {
                    return false;
                } else if (guildUser.Roles.Any(r => WubbysFunHouse.DiscordStaffRoles.Contains(r.Id))) {
                    return false;
                } else if (guildUser.Roles.Any(r => r.Id == WubbysFunHouse.NonSubRoleId)) {
                    return true;
                } else if (guildUser.Roles.Any(r => r.Id == WubbysFunHouse.MutedRoleId)) {
                    return true;
                } else {
                    return false;
                }
            } else {
                return true;
            }
        }

        private Embed LoadingEmbed(string message, string description = "") {
            EmbedBuilder builder = new EmbedBuilder() {
                Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                Title = $"{message}...",
                ThumbnailUrl = Constants.LoadingGifUrl
            };
            builder.Description = (string.IsNullOrWhiteSpace(description) ? null : description);

            return builder.Build();
        }

    }

}
