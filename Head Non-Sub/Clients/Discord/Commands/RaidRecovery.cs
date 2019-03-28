using System;
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

        private readonly int _MessageThreshold = 5;

        [Command("enable")]
        public Task Enable() {
            if (!RaidRecoveryTracker.Track(Context.Channel.Id, Context.User.ToString())) {
                return BetterReplyAsync($"Raid recovery mode is already enabled by {RaidRecoveryTracker.StartedBy(Context.Channel.Id)}.");
            }

            if (Context.Channel is SocketTextChannel channel) {
                channel.ModifyAsync(x => { x.SlowModeInterval = 120; });
            }

            EmbedBuilder builder = new EmbedBuilder() {
                Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                Title = "Raid recovery mode enabled",
                Description = "This channel is now in slow mode, normal uses can only send one message every two minutes."
            };

            builder.AddField("Commands", string.Join(Environment.NewLine, new string[] {
                $"Commands are used `@{Context.Guild.CurrentUser.Username} rr <command>`",
                "`disable` Disable raid recovery mode",
                "`list <minutes>` List all users involved in the past # minutes",
                "`clean <minutes>` Remove all messages from involved users from the past # minutes",
                "`ban <minutes>` Ban all users involved in the past # minutes, will prompt for confirmation" }));

            LoggingManager.Log.Warn($"Raid recovery mode enabled. {BetterLogFormat()}");
            return BetterReplyAsync(builder.Build());
        }

        [Command("disable")]
        public Task Disable() {
            if (!RaidRecoveryTracker.Exists(Context.Channel.Id)) {
                return BetterReplyAsync($"This channel is not in raid recovery mode. Use `@{Context.Guild.CurrentUser.Username} rr enable` to use commands.");
            }

            RaidRecoveryTracker.Untrack(Context.Channel.Id);

            if (Context.Channel is SocketTextChannel channel) {
                channel.ModifyAsync(x => { x.SlowModeInterval = 0; });
            }

            EmbedBuilder builder = new EmbedBuilder() {
                Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                Title = "Raid recovery mode disabled",
                Description = "This channel has returned to normal."
            };

            LoggingManager.Log.Warn($"Raid recovery mode disabled. {BetterLogFormat()}");
            return BetterReplyAsync(builder.Build());
        }

        [Command("list")]
        public Task List(int minutes = 5) {
            if (!RaidRecoveryTracker.Exists(Context.Channel.Id)) {
                return BetterReplyAsync($"This channel is not in raid recovery mode. Use `@{Context.Guild.CurrentUser.Username} rr enable` to use commands.");
            }

            if (minutes == 0 || minutes > 20160) {
                return BetterReplyAsync("Must be between 1 and 20160 (2 weeks). Rarely should this be greater than 10 unless a raid went unnoticed for longer.", minutes.ToString());
            }

            IUserMessage message = BetterReplyAsync(embed: LoadingEmbed("Generating list"), parameters: minutes.ToString()).Result;

            SocketTextChannel channel = Context.Channel as SocketTextChannel;
            IAsyncEnumerable<IMessage> messages = channel.GetMessagesAsync(1200).Flatten();
            List<IAsyncGrouping<IUser, IMessage>> messagesPerUser = messages.OrderByDescending(x => x.CreatedAt)
                .Where(x => x.CreatedAt > DateTime.UtcNow.AddMinutes(-minutes))
                .Where(x => {
                    if (x.Author is SocketGuildUser guildUser) {
                        if (guildUser.Roles.Any(r => WubbysFunHouse.DiscordStaffRoles.Contains(r.Id))) {
                            return false;
                        } else if (guildUser.Roles.Any(r => r.Id == WubbysFunHouse.NonSubRoleId)) {
                            return true;
                        } else if (guildUser.Roles.Count == 0) {
                            return true;
                        } else {
                            return false;
                        }
                    } else {
                        return false;
                    }
                })
                .GroupBy(x => x.Author)
                .Where(x => x.Count().Result > _MessageThreshold)
                .OrderByDescending(x => x.Count().Result)
                .ToList().Result;

            if (messagesPerUser.Count > 0) {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine($"{messagesPerUser.Count} unique users sent at least {_MessageThreshold} messages within the last {minutes.Minutes().Humanize(3)}. Staff are excluded from this list.");

                builder.AppendLine("```");
                foreach (IAsyncGrouping<IUser, IMessage> user in messagesPerUser) {
                    builder.Append($"{user.Key.ToString()}: {user.Count().Result}; ");
                }
                builder.AppendLine("```");

                message.ModifyAsync(x => { x.Embed = null; x.Content = builder.ToString(); });
            } else {
                message.ModifyAsync(x => { x.Embed = null; x.Content = $"There are no flagged users or messages in the past {minutes.Minutes().Humanize(3)}."; });
            }

            return Task.CompletedTask;
        }

        [Command("clean")]
        public Task Clean(int minutes = 5) {
            if (!RaidRecoveryTracker.Exists(Context.Channel.Id)) {
                return BetterReplyAsync($"This channel is not in raid recovery mode. Use `@{Context.Guild.CurrentUser.Username} rr enable` to use commands.");
            }

            if (minutes == 0 || minutes > 20160) {
                return BetterReplyAsync("Must be between 1 and 20160 (2 weeks). Rarely should this be greater than 10 unless a raid went unnoticed for longer.", minutes.ToString());
            }

            IUserMessage message = BetterReplyAsync(embed: LoadingEmbed("Deleting messages", "Staff and important bots messages will be preserved."), parameters: minutes.ToString()).Result;

            SocketTextChannel channel = Context.Channel as SocketTextChannel;
            IAsyncEnumerable<IMessage> messages = channel.GetMessagesAsync(1200).Flatten();
            IEnumerable<IMessage> messagesToDelete = messages.OrderByDescending(x => x.CreatedAt)
                .Where(x => x.CreatedAt > DateTime.UtcNow.AddMinutes(-minutes))
                .Where(x => {
                    if (x.Author is SocketGuildUser guildUser) {
                        if (guildUser.Roles.Any(r => WubbysFunHouse.DiscordStaffRoles.Contains(r.Id))) {
                            return false;
                        } else if (guildUser.Roles.Any(r => r.Id == WubbysFunHouse.NonSubRoleId)) {
                            return true;
                        } else if (guildUser.Roles.Count == 0) {
                            return true;
                        } else {
                            return false;
                        }
                    } else {
                        return false;
                    }
                })
                .ToEnumerable();

            channel.DeleteMessagesAsync(messagesToDelete).Wait();

            message.ModifyAsync(x => { x.Embed = null; x.Content = $"Deleted {messagesToDelete.Count()} messages from the past {minutes.Minutes().Humanize(3)}."; });

            return Task.CompletedTask;
        }

        [Command("ban")]
        [RequireBotPermission(GuildPermission.BanMembers, ErrorMessage = "I do not have the `Ban Members` permission.")]
        public Task Ban(int minutes = 5, [Remainder]string banToken = "") {
            if (!RaidRecoveryTracker.Exists(Context.Channel.Id)) {
                return BetterReplyAsync($"This channel is not in raid recovery mode. Use `@{Context.Guild.CurrentUser.Username} rr enable` to use commands.");
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

            IUserMessage message = BetterReplyAsync(embed: LoadingEmbed("Generating list of users to ban", "All staff and important bots are safe."), parameters: minutes.ToString()).Result;

            SocketTextChannel channel = Context.Channel as SocketTextChannel;
            IAsyncEnumerable<IMessage> messages = channel.GetMessagesAsync(1200).Flatten();
            IEnumerable<SocketGuildUser> usersToBan = messages.OrderByDescending(x => x.CreatedAt)
                .Where(x => x.CreatedAt > DateTime.UtcNow.AddMinutes(-minutes))
                .Where(x => {
                    if (x.Author is SocketGuildUser guildUser) {
                        if (guildUser.Roles.Any(r => WubbysFunHouse.DiscordStaffRoles.Contains(r.Id))) {
                            return false;
                        } else if (guildUser.Roles.Any(r => r.Id == WubbysFunHouse.NonSubRoleId)) {
                            return true;
                        } else if (guildUser.Roles.Count == 0) {
                            return true;
                        } else {
                            return false;
                        }
                    } else {
                        return false;
                    }
                })
                .GroupBy(x => x.Author)
                .Where(x => (x as IAsyncGrouping<IUser, IMessage>).Count().Result > _MessageThreshold)
                .Select(x => (x as IAsyncGrouping<IUser, IMessage>).Key as SocketGuildUser)
                .Distinct()
                .ToEnumerable();

            if (usersToBan.Count() > 0) {
                RaidRecoveryTracker.AddUsersToBan(Context.Channel.Id, usersToBan.Select(x => x.Id).ToHashSet());

                StringBuilder builder = new StringBuilder();
                builder.AppendLine($"The following {usersToBan.Count()} users will be banned.");

                builder.AppendLine("```");
                foreach (SocketGuildUser banUser in usersToBan) {
                    builder.Append($"{banUser.ToString()}; ");
                }
                builder.AppendLine("```");

                message.ModifyAsync(x => { x.Embed = null; x.Content = builder.ToString(); });

                BetterReplyAsync($"Ban confirmation. Selected time frame: {minutes.Minutes().Humanize(3)}; Token: `{RaidRecoveryTracker.BanToken(Context.Channel.Id)}`{Environment.NewLine}{Environment.NewLine}" +
                    $"Type `@{Context.Guild.CurrentUser.Username} rr skip @User` to remove them from the ban list.{Environment.NewLine}" +
                    $"Type `@{Context.Guild.CurrentUser.Username} rr ban {minutes} {RaidRecoveryTracker.BanToken(Context.Channel.Id)}` to ban the users above.").Wait();
            } else {
                message.ModifyAsync(x => { x.Embed = null; x.Content = $"There are no flagged users to ban within the past {minutes.Minutes().Humanize(3)}."; });
            }

            return Task.CompletedTask;
        }

        [Command("skip")]
        [RequireBotPermission(GuildPermission.BanMembers, ErrorMessage = "I do not have the `Ban Members` permission.")]
        public Task Skip(SocketUser user = null) {
            if (user == null) {
                return BetterReplyAsync("You must mention a user to skip them from being banned.");
            }

            if (RaidRecoveryTracker.SkipUserToBan(Context.Channel.Id, user.Id)) {
                return BetterReplyAsync($"{BetterUserFormat(user)} will be skipped and **not** banned.");
            } else {
                return BetterReplyAsync($"{BetterUserFormat(user)} was not found on the 'to ban list'.");
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
