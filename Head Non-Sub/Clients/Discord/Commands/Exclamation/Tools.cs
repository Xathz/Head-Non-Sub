using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Extensions;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [BlacklistEnforced]
    [RequireContext(ContextType.Guild)]
    public class Tools : BetterModuleBase {

        [Command("members")]
        public Task Members() {
            EmbedBuilder builder = new EmbedBuilder() {
                Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                Title = $"Members of {Context.Guild.Name}",
                ThumbnailUrl = Context.Guild.IconUrl
            };

            int total = Context.Guild.Users.Count;
            builder.AddField("Total", total.ToString("N0"), true);
            builder.AddField("Online / idle / do not disturb", Context.Guild.Users.Where(x => x.Status == UserStatus.Online).Count().ToString("N0") + " / " +
                Context.Guild.Users.Where(x => x.Status == UserStatus.Idle).Count().ToString("N0") + " / " +
                Context.Guild.Users.Where(x => x.Status == UserStatus.DoNotDisturb).Count().ToString("N0"), true);

            builder.AddField("Joined today / week / month", Context.Guild.Users.Where(x => x.JoinedAt.HasValue).Where(x => x.JoinedAt >= DateTime.UtcNow.AddDays(-1)).Count().ToString("N0") + " / " +
                Context.Guild.Users.Where(x => x.JoinedAt.HasValue).Where(x => x.JoinedAt >= DateTime.UtcNow.AddDays(-7)).Count().ToString("N0") + " / " +
                Context.Guild.Users.Where(x => x.JoinedAt.HasValue).Where(x => x.JoinedAt >= DateTime.UtcNow.AddMonths(-1)).Count().ToString("N0"), true);

            int nonSubs = Context.Guild.Users.Where(x => x.Roles.Any(r => r.Id == WubbysFunHouse.NonSubRoleId)).Count();
            string nonSubsPercent = ((double)nonSubs / total).ToString("0.00%");
            builder.AddField("Non-subs", $"{nonSubs.ToString("N0")} (_{nonSubsPercent}_)", true);

            int subsAndPatrons = Context.Guild.Users.Where(x => x.Roles.Any(r => r.Id == WubbysFunHouse.TwitchSubscriberRoleId || r.Id == WubbysFunHouse.PatronRoleId)).Distinct().Count();
            string subsAndPatronsPercent = ((double)subsAndPatrons / total).ToString("0%");
            builder.AddField("Subs / Patrons", $"{subsAndPatrons.ToString("N0")} (_{subsAndPatronsPercent}_)", true);

            return BetterReplyAsync(builder.Build());
        }

        [Command("random")]
        public async Task Random([Remainder]string type = "") {
            SocketGuildUser randomUser = null;

            if (type == "sub") {
                randomUser = Context.Guild.Users.Where(x => x.Roles.Any(r => r.Id == WubbysFunHouse.TwitchSubscriberRoleId || r.Id == WubbysFunHouse.PatronRoleId)).PickRandom();

            } else if (type == "non-sub" || type == "nonsub") {
                randomUser = Context.Guild.Users.Where(x => x.Roles.Any(r => r.Id == WubbysFunHouse.NonSubRoleId)).PickRandom();

            } else if (type == "tier3" || type == "t3") {
                randomUser = Context.Guild.Users.Where(x => x.Roles.Any(r => r.Id == WubbysFunHouse.Tier3RoleId)).PickRandom();

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
                    Text = $"Random user requested by {BetterUserFormat(useGrave: false)}"
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
                await BetterReplyAsync("Failed to select a random user.");
            }
        }

    }

}
