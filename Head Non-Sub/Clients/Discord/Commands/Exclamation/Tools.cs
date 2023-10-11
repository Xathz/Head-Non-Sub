using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Clients.Twitch;
using HeadNonSub.Extensions;
using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [BlacklistEnforced]
    [RequireContext(ContextType.Guild)]
    public class Tools : BetterModuleBase {

        [Command("members")]
        public Task Members() {
            EmbedBuilder builder = new EmbedBuilder() {
                Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                Title = $"Members of {Context.Guild.Name}",
                ThumbnailUrl = WubbysFunHouse.IconUrl
            };

            int total = Context.Guild.Users.Count;
            builder.AddField("Total", total.ToString("N0"), true);
            builder.AddField("Online / idle / do not disturb", Context.Guild.Users.Where(x => x.Status == UserStatus.Online).Count().ToString("N0") + " / " +
                Context.Guild.Users.Where(x => x.Status == UserStatus.Idle).Count().ToString("N0") + " / " +
                Context.Guild.Users.Where(x => x.Status == UserStatus.DoNotDisturb).Count().ToString("N0"), true);

            builder.AddField("Joined today / week / month", Context.Guild.Users.Where(x => x.JoinedAt.HasValue).Where(x => x.JoinedAt >= DateTime.UtcNow.AddDays(-1)).Count().ToString("N0") + " / " +
                Context.Guild.Users.Where(x => x.JoinedAt.HasValue).Where(x => x.JoinedAt >= DateTime.UtcNow.AddDays(-7)).Count().ToString("N0") + " / " +
                Context.Guild.Users.Where(x => x.JoinedAt.HasValue).Where(x => x.JoinedAt >= DateTime.UtcNow.AddMonths(-1)).Count().ToString("N0"), true);

            int subsAndPatrons = Context.Guild.Users.Where(x => x.Roles.Any(r => r.Id == WubbysFunHouse.TwitchSubscriberRoleId)).Distinct().Count();
            string subsAndPatronsPercent = ((double)subsAndPatrons / total).ToString("0%");
            builder.AddField("Subs / Patrons", $"{subsAndPatrons:N0} (_{subsAndPatronsPercent}_)", true);

            int nonSubs = Context.Guild.Users.Where(x => !x.Roles.Any(r => r.Id == WubbysFunHouse.TwitchSubscriberRoleId)).Distinct().Count();
            string nonSubsPercent = ((double)nonSubs / total).ToString("0.00%");
            builder.AddField("Non-subs", $"{nonSubs:N0} (_{nonSubsPercent}_)", true);

            return BetterReplyAsync(builder.Build());
        }

        [Command("random")]
        public async Task Random([Remainder] string type = "") {
            SocketGuildUser randomUser = null;

            if (type == "sub") {
                randomUser = Context.Guild.Users.Where(x => x.Roles.Any(r => r.Id == WubbysFunHouse.TwitchSubscriberRoleId)).PickRandom();

            } else if (type == "non-sub" || type == "nonsub") {
                randomUser = Context.Guild.Users.Where(x => !x.Roles.Any(r => r.Id == WubbysFunHouse.TwitchSubscriberRoleId)).PickRandom();

            } else if (type == "tier3" || type == "t3") {
                randomUser = Context.Guild.Users.Where(x => x.Roles.Any(r => r.Id == WubbysFunHouse.TwitchSubscriberTier3RoleId)).PickRandom();

            } else if (type == "admin") {
                randomUser = Context.Guild.Users.Where(x => x.Roles.Any(r => r.Id == WubbysFunHouse.AdminsRoleId)).PickRandom();

            } else if (type == "mod") {
                randomUser = Context.Guild.Users.Where(x => x.Roles.Any(r => r.Id == WubbysFunHouse.ModsRoleId | r.Id == WubbysFunHouse.ModLiteRoleId | r.Id == WubbysFunHouse.TwitchModRoleId | r.Id == WubbysFunHouse.SubredditModRoleId)
                                        & !x.Roles.Any(r => r.Id == WubbysFunHouse.AdminsRoleId | r.Id == WubbysFunHouse.GingerBoyRoleId)).PickRandom();

            } else if (type == "tree") {
                randomUser = Context.Guild.Users.Where(x => x.Id == 339786294895050753).FirstOrDefault(); // Amanda

            } else if (type == "5'8\"" || type == "5'8") {
                randomUser = Context.Guild.Users.Where(x => x.Id == 177657233025400832).FirstOrDefault(); // Wubby

            } else {
                await BetterReplyAsync("**Valid roles are:** sub *(twitch and patreon)*, nonsub, tier3, admin, mod, tree, 5'8\"", parameters: type);
                return;
            }

            // If it is a valid user
            if (randomUser is SocketGuildUser) {
                EmbedBuilder builder = new EmbedBuilder() {
                    Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                    Title = $"Picking a random {type}...",
                    ThumbnailUrl = Constants.LoadingGifUrl
                };

                builder.Footer = new EmbedFooterBuilder() {
                    Text = $"Random user requested by {BetterUserFormat(formatChar: "")}"
                };

                IUserMessage message = await BetterReplyAsync(builder.Build(), parameters: type);

                await Task.Delay(8000);

                builder.Title = BetterUserFormat(randomUser);
                builder.ThumbnailUrl = null;
                builder.Fields.Clear();

                builder.AddField("Account created", $"{randomUser.CreatedAt.DateTime.ToShortDateString()} {randomUser.CreatedAt.DateTime.ToShortTimeString()}", true);

                if (randomUser.JoinedAt.HasValue) {
                    builder.AddField("Joined server", $"{randomUser.JoinedAt.Value.DateTime.ToShortDateString()} {randomUser.JoinedAt.Value.DateTime.ToShortTimeString()}", true);
                }

                await message.ModifyAsync(x => { x.Embed = builder.Build(); });

            } else {
                await BetterReplyAsync("Failed to select a random user.", parameters: type);
            }
        }

        [Command("timer")]
        public async Task Timer(TimeSpan input, [Remainder] string message) {
            if (string.IsNullOrWhiteSpace(message)) {
                await BetterReplyAsync("You must set a message for the timer. e.g. `!timer 20s boo! too spoopy`");
            }

            if (input.TotalMinutes > 30) {
                await BetterReplyAsync("The timer has a maximum duration of 30 minutes. Was too lazy to add data persistence for this.", parameters: $"{input.Humanize()}; {message}");
                return;
            }

            await BetterReplyAsync($"A timer has been set for {input.Humanize()}.", parameters: $"{input.Humanize()}; {message}");

            await Task.Delay(Convert.ToInt32(input.TotalMilliseconds));

            await BetterReplyAsync(message, parameters: $"{input.Humanize()}; {message}");
        }

        [Command("iswubbylive")]
        public Task IsWubbyLive() => BetterReplyAsync(TwitchClient.IsLive ? "**Yes**" : "**No**");

        [Command("altsniff")]
        [DiscordStaffOnly]
        [AllowedCategories(WubbysFunHouse.ModDaddiesCategoryId)]
        public async Task AltSniff(SocketGuildUser user = null) {
            if (user == null) {
                await BetterReplyAsync("You must mention a user to check.", parameters: "user null");
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            StringBuilder builder = new StringBuilder();

            // Account creation to join
            if (user.JoinedAt.HasValue) {
                TimeSpan timeFromCreateToJoin = user.JoinedAt.Value - user.CreatedAt;

                if (timeFromCreateToJoin.TotalHours <= 12) {
                    builder.AppendLine($":small_blue_diamond: **Time from account creation to join:** {timeFromCreateToJoin.Humanize(3, minUnit: Humanizer.Localisation.TimeUnit.Second)}");
                }
            }

            // Username matching
            {
                HashSet<string> matchingUsernames = new HashSet<string>();

                IEnumerable<SocketGuildUser> matchingUsernames_Inner = Context.Guild.Users.Where(x => x.Id != user.Id && x.Username.Contains(user.Username, StringComparison.OrdinalIgnoreCase));
                if (matchingUsernames_Inner.Count() > 0) {
                    foreach (SocketGuildUser matchedUser in matchingUsernames_Inner) {
                        matchingUsernames.Add($"{matchedUser.Username} (`{matchedUser.Id}`)");
                    }
                }

                //IEnumerable<SocketGuildUser> matchingUsernames_Outer = Context.Guild.Users.Where(x => x.Id != user.Id && user.Username.Contains(x.Username, StringComparison.OrdinalIgnoreCase));
                //if (matchingUsernames_Outer.Count() > 0) {
                //    foreach (SocketGuildUser matchedUser in matchingUsernames_Outer) {
                //        matchingUsernames.Add($"{matchedUser.Username} (`{matchedUser.Id}`)");
                //    }
                //}

                if (matchingUsernames.Count > 0) {
                    builder.AppendLine($":small_orange_diamond: **Matching usernames:** {string.Join(", ", matchingUsernames)}");
                }
            }

            // Banned username matching
            {
                IEnumerable<RestBan> bannedUsers = await Context.Guild.GetBansAsync().FlattenAsync();
                HashSet<string> matchingUsernames = new HashSet<string>();

                IEnumerable<RestBan> matchingUsernames_Inner = bannedUsers.Where(x => x.User.Id != user.Id && x.User.Username.Contains(user.Username, StringComparison.OrdinalIgnoreCase));
                if (matchingUsernames_Inner.Count() > 0) {
                    foreach (RestBan matchedUser in matchingUsernames_Inner) {
                        matchingUsernames.Add($"{matchedUser.User.Username} (`{matchedUser.User.Id}`)");
                    }
                }

                //IEnumerable<RestBan> matchingUsernames_Outer = bannedUsers.Where(x => x.User.Id != user.Id && user.Username.Contains(x.User.Username, StringComparison.OrdinalIgnoreCase));
                //if (matchingUsernames_Outer.Count() > 0) {
                //    foreach (RestBan matchedUser in matchingUsernames_Outer) {
                //        matchingUsernames.Add($"{matchedUser.User.Username} (`{matchedUser.User.Id}`)");
                //    }
                //}

                if (matchingUsernames.Count > 0) {
                    builder.AppendLine($":small_orange_diamond: **Matching banned usernames:** {string.Join(", ", matchingUsernames)}");
                }
            }


            // Nickname matching
            {
                if (!string.IsNullOrEmpty(user.Nickname)) {
                    HashSet<string> matchingNicknames = new HashSet<string>();

                    IEnumerable<SocketGuildUser> matchingNicknames_Inner = Context.Guild.Users.Where(x => x.Id != user.Id && !string.IsNullOrEmpty(x.Nickname) && x.Nickname.Contains(user.Nickname, StringComparison.OrdinalIgnoreCase));
                    if (matchingNicknames_Inner.Count() > 0) {
                        foreach (SocketGuildUser matchedUser in matchingNicknames_Inner) {
                            matchingNicknames.Add($"{matchedUser.Username} (`{matchedUser.Id}`)");
                        }
                    }

                    //IEnumerable<SocketGuildUser> matchingNicknames_Outer = Context.Guild.Users.Where(x => x.Id != user.Id && !string.IsNullOrEmpty(x.Nickname) && user.Nickname.Contains(x.Nickname, StringComparison.OrdinalIgnoreCase));
                    //if (matchingNicknames_Outer.Count() > 0) {
                    //    foreach (SocketGuildUser matchedUser in matchingNicknames_Outer) {
                    //        matchingNicknames.Add($"{matchedUser.Username} (`{matchedUser.Id}`)");
                    //    }
                    //}

                    if (matchingNicknames.Count > 0) {
                        builder.AppendLine($":small_orange_diamond: **Matching nicknames:** {string.Join(", ", matchingNicknames)}");
                    }
                }
            }

            if (!string.IsNullOrEmpty(builder.ToString())) {
                await BetterReplyAsync($"● Possible alts and suspicious events about {BetterUserFormat(user)}{Environment.NewLine}{builder}", parameters: $"{user} ({user.Id})");
            } else {
                await BetterReplyAsync($"No possible alts or suspicious events were found about {BetterUserFormat(user)}.", parameters: $"{user} ({user.Id})");
            }
        }

        [Command("banreason")]
        [TwitchStaffOnly]
        [AllowedCategories(WubbysFunHouse.ModDaddiesCategoryId)]
        public async Task BanReason(string username = null) {
            if (string.IsNullOrWhiteSpace(username)) {
                await BetterReplyAsync("You must enter a username to check.", parameters: "username null");
                return;
            }

            IEnumerable<RestBan> bans = await Context.Guild.GetBansAsync().FlattenAsync();
            IEnumerable<RestBan> matches = bans.Where(x => x.User.Username.IndexOf(username, StringComparison.OrdinalIgnoreCase) != -1);

            StringBuilder builder = new StringBuilder();

            foreach (RestBan match in matches) {
                builder.AppendLine($"{match.User}: {match.Reason}");
            }

            List<string> chunks = builder.ToString().SplitIntoChunksPreserveNewLines(1930);

            if (chunks.Count > 0) {
                foreach (string chunk in chunks) {
                    await BetterReplyAsync($"● Matching banned users ```{chunk}```", parameters: username);
                }
            } else {
                foreach (string chunk in chunks) {
                    await BetterReplyAsync("No banned users were found.", parameters: username);
                }
            }
        }

        [Command("rolld20"), Alias("d20")]
        public async Task Roll([Remainder] string reason = "") {
            int needed = new Random().Next(1, 20);
            int roll = new Random().Next(1, 20);
            bool rollMet = roll <= needed;

            if (string.IsNullOrWhiteSpace(reason)) {
                await BetterReplyAsync($":game_die: {Context.User.Mention} forgot the reason for their roll, will they instantly die? {(rollMet ? "Yes, in extreme pain" : "Unfortunately no")} (**{needed}** needed and rolled **{roll}**)");
            } else {
                await BetterReplyAsync($":game_die: {Context.User.Mention} rolls for `{reason.Replace("`", "")}`.. (**{needed}** needed and rolled **{roll}**)");
            }
        }

        //[Command("role")]
        //[DiscordStaffOnly]
        //[AllowedCategories(675632145749770297)]
        //public async Task AddRoleEz() {
        //    if (Context.Message.Embeds.Count > 0) {
        //        foreach (Embed embed in Context.Message.Embeds) {
        //            List<Entities.Discord.MessageTag> tags = embed.Description.ParseDiscordMessageTags();
        //            bool breakLoop = false;

        //            foreach (Entities.Discord.MessageTag tag in tags) {
        //                if (tag.TagType == Entities.Discord.TagType.User) {
        //                    SocketGuildUser mailUser = Context.Guild.GetUser(tag.Id);

        //                    await WubbysFunHouse.AddRoleAsync(mailUser, 913966047470714960, "Merch 'ugly' role.");
        //                    await Context.Channel.SendMessageAsync($"Added role to {mailUser.Mention}.");

        //                    breakLoop = true;
        //                    break;
        //                }
        //            }

        //            if (breakLoop) { break; }
        //        }
        //    }
        //}

    }
}
