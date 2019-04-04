using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public Task Enable() {
            if (Context.Channel is SocketTextChannel channel) {
                if (!RaidRecoveryTracker.Track(Context.Channel.Id, Context.User.Id, channel.SlowModeInterval)) {
                    ulong? startedBy = RaidRecoveryTracker.StartedBy(Context.Channel.Id);

                    if (startedBy.HasValue) {
                        return BetterReplyAsync($"Raid recovery system is already enabled. Enabled by {BetterUserFormat(UserFromUserId(startedBy.Value))}.");
                    } else {
                        return BetterReplyAsync($"Raid recovery system is already enabled.");
                    }
                }

                channel.ModifyAsync(x => { x.SlowModeInterval = 120; });
            } else {
                return BetterReplyAsync("Invalid channel, not a text only channel.");
            }

            EmbedBuilder builder = new EmbedBuilder() {
                Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                Title = "Raid recovery system enabled",
                Description = "This channel is now in slow mode, normal uses can only send one message every two minutes."
            };

            builder.AddField("Commands", string.Join(Environment.NewLine, new string[] {
                $"Commands are used `@{Context.Guild.CurrentUser.Username} rr <command>`",
                "`disable` Disable raid recovery system",
                "`list <minutes>` List suspected users from the past # minutes",
                "`clean <minutes>` Remove messages from suspected users from the past # minutes",
                "`ban <minutes>` Ban suspected users in the past # minutes, will prompt for confirmation" }));

            LoggingManager.Log.Warn($"Raid recovery system enabled. {BetterLogFormat()}");
            return BetterReplyAsync(builder.Build());
        }

        [Command("disable")]
        public Task Disable() {
            if (!RaidRecoveryTracker.Exists(Context.Channel.Id)) {
                return BetterReplyAsync($"The raid recovery system is not enabled. Use `@{Context.Guild.CurrentUser.Username} rr enable` to enable.");
            }

            RaidRecoveryTracker.Untrack(Context.Channel.Id);

            if (Context.Channel is SocketTextChannel channel) {
                channel.ModifyAsync(x => { x.SlowModeInterval = 0; });
            }

            EmbedBuilder builder = new EmbedBuilder() {
                Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                Title = "Raid recovery system disabled",
                Description = "This channel has returned to normal."
            };

            LoggingManager.Log.Warn($"Raid recovery system disabled. {BetterLogFormat()}");
            return BetterReplyAsync(builder.Build());
        }

        [Command("list")]
        public Task List(int minutes = 10) {
            if (!RaidRecoveryTracker.Exists(Context.Channel.Id)) {
                return BetterReplyAsync($"The raid recovery system is not enabled. Use `@{Context.Guild.CurrentUser.Username} rr enable` to enable.");
            }

            if (minutes == 0 || minutes > 20160) {
                return BetterReplyAsync("Must be between 1 and 20160 (2 weeks). Rarely should this be greater than 10 unless a raid went unnoticed for longer.", minutes.ToString());
            }

            IUserMessage message = BetterReplyAsync(embed: LoadingEmbed("Generating list"), parameters: minutes.ToString()).Result;

            List<IGrouping<IUser, IMessage>> messagesPerUser = GetMessages().OrderByDescending(x => x.CreatedAt)
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

                message.ModifyAsync(x => { x.Embed = null; x.Content = builder.ToString(); });
            } else {
                message.ModifyAsync(x => { x.Embed = null; x.Content = $"There are no suspected users from the past {minutes.Minutes().Humanize(3)}."; });
            }

            return Task.CompletedTask;
        }

        [Command("clean")]
        public Task Clean(int minutes = 10) {
            if (!RaidRecoveryTracker.Exists(Context.Channel.Id)) {
                return BetterReplyAsync($"The raid recovery system is not enabled. Use `@{Context.Guild.CurrentUser.Username} rr enable` to enable.");
            }

            if (minutes == 0 || minutes > 20160) {
                return BetterReplyAsync("Must be between 1 and 20160 (2 weeks). Rarely should this be greater than 10 unless a raid went unnoticed for longer.", minutes.ToString());
            }

            IUserMessage message = BetterReplyAsync(embed: LoadingEmbed("Deleting messages", "Staff messages will be preserved."), parameters: minutes.ToString()).Result;

            SocketTextChannel channel = Context.Channel as SocketTextChannel;
            IEnumerable<IMessage> messagesToDelete = GetMessages().OrderByDescending(x => x.CreatedAt)
                .Where(x => x.CreatedAt > DateTime.UtcNow.AddMinutes(-minutes))
                .Where(x => IsSuspected(x.Author));

            channel.DeleteMessagesAsync(messagesToDelete).Wait();

            message.ModifyAsync(x => { x.Embed = null; x.Content = $"Deleted {messagesToDelete.Count()} messages from the past {minutes.Minutes().Humanize(3)}."; });

            return Task.CompletedTask;
        }

        [Command("ban")]
        [RequireBotPermission(GuildPermission.BanMembers, ErrorMessage = "I do not have the `Ban Members` permission.")]
        public Task Ban(int minutes = 10, [Remainder]string banToken = "") {
            if (!RaidRecoveryTracker.Exists(Context.Channel.Id)) {
                return BetterReplyAsync($"The raid recovery system is not enabled. Use `@{Context.Guild.CurrentUser.Username} rr enable` to enable.");
            }

            if (minutes == 0 || minutes > 20160) {
                return BetterReplyAsync("Must be between 1 and 20160 (2 weeks). Rarely should this be greater than 10 unless a raid went unnoticed for longer.", minutes.ToString());
            }

            if (!string.IsNullOrWhiteSpace(banToken)) {
                if (RaidRecoveryTracker.ValidateBanToken(Context.Channel.Id, banToken)) {
                    IUserMessage banningMessage = BetterReplyAsync(embed: LoadingEmbed("Banning users"), parameters: minutes.ToString()).Result;
                    HashSet<ulong> usersIdsToBan = RaidRecoveryTracker.UsersToBan(Context.Channel.Id);

                    try {
                        foreach (ulong user in usersIdsToBan) {
                            Context.Guild.AddBanAsync(user, 1, $"Banned by '{Context.User.ToString()}' using the bot '{Context.Guild.CurrentUser.Username}' raid recovery system on {DateTime.UtcNow.ToString("o")}");
                        }
                    } catch { }

                    banningMessage.ModifyAsync(x => { x.Embed = null; x.Content = $"Banned {usersIdsToBan.Count} users."; });
                    return Task.CompletedTask;
                } else {
                    return BetterReplyAsync("Invalid ban token.");
                }
            }

            IUserMessage message = BetterReplyAsync(embed: LoadingEmbed("Generating list of users to ban"), parameters: minutes.ToString()).Result;

            IEnumerable<IUser> usersToBan = GetMessages().OrderByDescending(x => x.CreatedAt)
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

                message.ModifyAsync(x => { x.Embed = null; x.Content = builder.ToString(); });

                BetterReplyAsync($"Ban confirmation. Selected time frame: {minutes.Minutes().Humanize(3)}; Token: `{RaidRecoveryTracker.BanToken(Context.Channel.Id)}`{Environment.NewLine}{Environment.NewLine}" +
                    $"Type `@{Context.Guild.CurrentUser.Username} rr skip @User` to remove them from the ban list.{Environment.NewLine}" +
                    $"Type `@{Context.Guild.CurrentUser.Username} rr ban {minutes} {RaidRecoveryTracker.BanToken(Context.Channel.Id)}` to ban the users above.").Wait();
            } else {
                message.ModifyAsync(x => { x.Embed = null; x.Content = $"There are no suspected users to ban from the past {minutes.Minutes().Humanize(3)}."; });
            }

            return Task.CompletedTask;
        }

        [Command("skip")]
        [RequireBotPermission(GuildPermission.BanMembers, ErrorMessage = "I do not have the `Ban Members` permission.")]
        public Task Skip(SocketUser user = null) {
            if (!RaidRecoveryTracker.Exists(Context.Channel.Id)) {
                return BetterReplyAsync($"The raid recovery system is not enabled. Use `@{Context.Guild.CurrentUser.Username} rr enable` to enable.");
            }

            if (user == null) {
                return BetterReplyAsync("You must provide a user to skip them from being banned.");
            }

            if (RaidRecoveryTracker.SkipUserToBan(Context.Channel.Id, user.Id)) {
                return BetterReplyAsync($"{BetterUserFormat(user)} will be skipped and **not** banned.");
            } else {
                return BetterReplyAsync($"{BetterUserFormat(user)} was not suspected and will **not** be banned.");
            }
        }

        private ReadOnlyCollection<IMessage> GetMessages() {
            if (Context.Channel is SocketTextChannel channel) {
                List<IMessage> channelMessages = new List<IMessage>();
                channel.CachedMessages.ToList().ForEach(x => {
                    if (!channelMessages.Any(m => m.Id == x.Id)) {
                        channelMessages.Add(x);
                    }
                });

                if (channelMessages.Count > 0) {
                    channel.GetMessagesAsync(channelMessages.OrderBy(x => x.CreatedAt).FirstOrDefault(), Direction.Before, 1000).Flatten().ToList().Result.ForEach(x => {
                        if (!channelMessages.Any(m => m.Id == x.Id)) {
                            channelMessages.Add(x);
                        }
                    });
                } else {
                    channel.GetMessagesAsync(1000).Flatten().ToList().Result.ForEach(x => {
                        if (!channelMessages.Any(m => m.Id == x.Id)) {
                            channelMessages.Add(x);
                        }
                    });
                }

                return channelMessages.ToList().AsReadOnly();
            } else {
                return new List<IMessage>().AsReadOnly();
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
