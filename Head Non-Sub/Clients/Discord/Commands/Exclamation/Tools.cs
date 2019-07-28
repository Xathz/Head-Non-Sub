using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Extensions;
using HeadNonSub.Settings;
using HeadNonSub.Statistics;
using Humanizer;

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

            int ruleReaders = Context.Guild.Users.Where(x => x.Roles.Any(r => r.Id == WubbysFunHouse.RuleReaderRoleId)).Distinct().Count();
            string ruleReadersPercent = ((double)ruleReaders / total).ToString("0%");
            builder.AddField("People who read the rules", $"{ruleReaders.ToString("N0")} (_{ruleReadersPercent}_)", true);

            int subsAndPatrons = Context.Guild.Users.Where(x => x.Roles.Any(r => r.Id == WubbysFunHouse.TwitchSubscriberRoleId || r.Id == WubbysFunHouse.PatronRoleId)).Distinct().Count();
            string subsAndPatronsPercent = ((double)subsAndPatrons / total).ToString("0%");
            builder.AddField("Subs / Patrons", $"{subsAndPatrons.ToString("N0")} (_{subsAndPatronsPercent}_)", true);

            int nonSubs = Context.Guild.Users.Where(x => x.Roles.Any(r => r.Id == WubbysFunHouse.NonSubRoleId)).Count();
            string nonSubsPercent = ((double)nonSubs / total).ToString("0.00%");
            builder.AddField("Non-subs", $"{nonSubs.ToString("N0")} (_{nonSubsPercent}_)", true);

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

        [Command("timer")]
        public async Task Timer(TimeSpan input, [Remainder]string message) {
            if (string.IsNullOrWhiteSpace(message)) {
                await BetterReplyAsync("You must set a message for the timer. e.g. `!timer 20s boo! too spoopy`");
            }

            if (input.TotalMinutes > 30) {
                await BetterReplyAsync("The timer has a maximum duration of 30 minutes. Was too lazy to add data persistence for this.");
            }

            await BetterReplyAsync($"A timer has been set for {input.Humanize()}.");

            await Task.Delay(Convert.ToInt32(input.TotalMilliseconds));

            await BetterReplyAsync(message);
        }

        [Command("names")]
        public async Task NameChanges(SocketUser user = null) {
            if (user == null) {
                await BetterReplyAsync("You must mention a user to see their name changes.");
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            string changes = StatisticsManager.GetUserChanges(user.Id);

            if (string.IsNullOrWhiteSpace(changes)) {
                await BetterReplyAsync($"There is no name change data for {BetterUserFormat(user)}. Maybe they just never changed their name. :shrug:");
                return;
            }

            List<string> chunks = changes.SplitIntoChunksPreserveNewLines(1930);

            if (chunks.Count > 2) {
                try {
                    string message = "";

                    using (HttpClient client = new HttpClient())
                    using (MultipartFormDataContent form = new MultipartFormDataContent()) {
                        byte[] byteArray = Encoding.UTF8.GetBytes(changes);

                        using (StreamContent streamContent = new StreamContent(new MemoryStream(byteArray)))
                        using (ByteArrayContent fileContent = new ByteArrayContent(streamContent.ReadAsByteArrayAsync().Result)) {
                            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");

                            form.Add(new StringContent(SettingsManager.Configuration.UploadKey), "key");
                            form.Add(fileContent, "file", $"{Guid.NewGuid()}.txt");

                            HttpResponseMessage response = client.PostAsync("https://xathz.net/headnonsub/upload.php", form).Result;

                            if (response.IsSuccessStatusCode) {
                                using (HttpContent content = response.Content) {
                                    string fileId = await content.ReadAsStringAsync();
                                    message = $"https://xathz.net/headnonsub/uploads/{fileId}";
                                }
                            } else {
                                message = $"There was an error uploading the file. ({response.StatusCode}) {response.ReasonPhrase}";
                            }
                        }
                    }

                    await BetterReplyAsync($"There are too many name changes to display here. {message}");
                    return;
                } catch (Exception ex) {
                    LoggingManager.Log.Error(ex);
                    return;
                }
            }

            foreach (string chunk in chunks) {
                await BetterReplyAsync($"● Name changes for {BetterUserFormat(user)} ```{chunk}```", user.Id.ToString());
            }
        }

    }

}
