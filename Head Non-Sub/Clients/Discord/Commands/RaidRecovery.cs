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
    [OwnerAdminWhitelist]
    [RequireContext(ContextType.Guild)]
    [AllowedGuilds(328300333010911242)] // Wubby's Fun House
    [RequireBotPermission(GuildPermission.ManageMessages, ErrorMessage = "I do not have the `Manage Messages` permission, raid recovery can not be used.")]
    [RequireBotPermission(GuildPermission.ManageChannels, ErrorMessage = "I do not have the `Manage Channels` permission, raid recovery can not be used.")]
    public class RaidRecovery : BetterModuleBase {

        [Command("enable")]
        public Task Enable() {
            RaidRecoveryTracker.Track(Context.Channel.Id);

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
                "`ban <minutes>` Ban all users involved in the past # minutes" }));

            LoggingManager.Log.Warn($"Raid recovery mode enabled. {BetterLogFormat()}");
            return BetterReplyAsync(builder.Build());
        }

        [Command("disable")]
        public Task Disable() {
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
            if (minutes == 0 || minutes > 20160) {
                return BetterReplyAsync("Must be between 1 and 20160 (2 weeks). Rarely should this be greater than 10 unless a raid went unnoticed for longer.", minutes.ToString());
            }

            if (!RaidRecoveryTracker.Exists(Context.Channel.Id)) {
                return BetterReplyAsync($"This channel is not in raid recovery mode. Use `@{Context.Guild.CurrentUser.Username} rr enable` to use commands.");
            }

            IUserMessage message = BetterReplyAsync(embed: LoadingEmbed("Generating list"), parameters: minutes.ToString()).Result;

            SocketTextChannel channel = Context.Channel as SocketTextChannel;
            IAsyncEnumerable<IMessage> messages = channel.GetMessagesAsync(1200).Flatten();
            List<IAsyncGrouping<IUser, IMessage>> messagesPerUser = messages.OrderByDescending(x => x.CreatedAt)
                .Where(x => x.CreatedAt > DateTime.UtcNow.AddMinutes(-minutes))
                .Where(x => !People.Staff.ContainsKey(x.Author.Id)) // Exclude staff
                .Where(x => {
                    if (x.Author is SocketGuildUser guildUser) {
                        return guildUser.Roles.Any(r => r.Id == 508752510216044547); // Non-sub
                    } else {
                        return false;
                    }
                })
                .GroupBy(x => x.Author)
                .OrderByDescending(x => x.Count().Result)
                .ToList().Result;

            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"{messagesPerUser.Count} unique users sent at least one message within the last {minutes.Minutes().Humanize(3)}. Staff are excluded from this list.");

            builder.AppendLine("```");
            foreach (IAsyncGrouping<IUser, IMessage> user in messagesPerUser) {
                builder.Append($"{user.Key.ToString()}: {user.Count().Result}; ");
            }
            builder.AppendLine("```");

            message.ModifyAsync(x => { x.Embed = null; x.Content = builder.ToString(); });

            return Task.CompletedTask;
        }

        [Command("clean")]
        public Task Clean(int minutes = 5) {
            if (minutes == 0 || minutes > 20160) {
                return BetterReplyAsync("Must be between 1 and 20160 (2 weeks). Rarely should this be greater than 10 unless a raid went unnoticed for longer.", minutes.ToString());
            }

            if (!RaidRecoveryTracker.Exists(Context.Channel.Id)) {
                return BetterReplyAsync($"This channel is not in raid recovery mode. Use `@{Context.Guild.CurrentUser.Username} rr enable` to use commands.");
            }

            IUserMessage message = BetterReplyAsync(embed: LoadingEmbed("Deleting messages", "Staff messages will be preserved."), parameters: minutes.ToString()).Result;

            SocketTextChannel channel = Context.Channel as SocketTextChannel;
            IAsyncEnumerable<IMessage> messages = channel.GetMessagesAsync(1200).Flatten();
            IEnumerable<IMessage> messagesToDelete = messages.OrderByDescending(x => x.CreatedAt)
                .Where(x => x.CreatedAt > DateTime.UtcNow.AddMinutes(-minutes))
                .Where(x => !People.Staff.ContainsKey(x.Author.Id)) // Exclude staff
                .Where(x => {
                    if (x.Author is SocketGuildUser guildUser) {
                        return guildUser.Roles.Any(r => r.Id == 508752510216044547); // Non-sub
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
        public Task Ban(int minutes = 5, string banToken = "") {
            if (minutes == 0 || minutes > 20160) {
                return BetterReplyAsync("Must be between 1 and 20160 (2 weeks). Rarely should this be greater than 10 unless a raid went unnoticed for longer.", minutes.ToString());
            }

            if (!RaidRecoveryTracker.Exists(Context.Channel.Id)) {
                return BetterReplyAsync($"This channel is not in raid recovery mode. Use `@{Context.Guild.CurrentUser.Username} rr enable` to use commands.");
            }

            if (!string.IsNullOrWhiteSpace(banToken)) {
                if (RaidRecoveryTracker.ValidateBanToken(Context.Channel.Id, banToken)) {
                    IUserMessage banningMessage = BetterReplyAsync(embed: LoadingEmbed("Banning users", "All staff are safe."), parameters: minutes.ToString()).Result;
                    HashSet<ulong> usersIdsToBan = RaidRecoveryTracker.UsersToBan(Context.Channel.Id);

                    try {
                        foreach (ulong user in usersIdsToBan) {
                            Context.Guild.AddBanAsync(user, 1, $"Banned by '{Context.User.ToString()}' using the bot '{Context.Guild.CurrentUser.Username}' raid recovery system on {DateTime.UtcNow.ToString("o")}");
                        }
                    } catch { }

                    banningMessage.ModifyAsync(x => { x.Embed = null; x.Content = $"Banned {RaidRecoveryTracker.UsersToBan(Context.Channel.Id).Count} users."; });
                    return Task.CompletedTask;
                } else {
                    return BetterReplyAsync("Invalid ban token.");
                }
            }

            IUserMessage message = BetterReplyAsync(embed: LoadingEmbed("Generating list of users to ban", "All staff are safe."), parameters: minutes.ToString()).Result;

            SocketTextChannel channel = Context.Channel as SocketTextChannel;
            IAsyncEnumerable<IMessage> messages = channel.GetMessagesAsync(1200).Flatten();
            IEnumerable<SocketGuildUser> usersToBan = messages.OrderByDescending(x => x.CreatedAt)
                .Where(x => x.CreatedAt > DateTime.UtcNow.AddMinutes(-minutes))
                .Where(x => !People.Staff.ContainsKey(x.Author.Id)) // Exclude staff
                .Where(x => {
                    if (x.Author is SocketGuildUser guildUser) {
                        return guildUser.Roles.Any(r => r.Id == 508752510216044547); // Non-sub
                    } else {
                        return false;
                    }
                })
                .Select(x => x.Author as SocketGuildUser)
                .Distinct()
                .ToEnumerable();

            RaidRecoveryTracker.AddUsersToBan(Context.Channel.Id, usersToBan.Select(x => x.Id).ToHashSet());

            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"The following {usersToBan.Count()} users will be banned.");

            builder.AppendLine("```");
            foreach (SocketGuildUser banUser in usersToBan) {
                builder.Append($"{banUser.ToString()}; ");
            }
            builder.AppendLine("```");

            message.ModifyAsync(x => { x.Embed = null; x.Content = builder.ToString(); });

            BetterReplyAsync($"Ban preparation.{Environment.NewLine}{Environment.NewLine}" +
                $"Type `@{Context.Guild.CurrentUser.Username} rr skip @User` to remove them from the ban list.{Environment.NewLine}" +
                $"Type `@{Context.Guild.CurrentUser.Username} rr ban {RaidRecoveryTracker.BanToken(Context.Channel.Id)}` to ban the users above.").Wait();

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
