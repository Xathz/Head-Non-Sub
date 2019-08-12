using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Entities.Discord;
using HeadNonSub.Extensions;
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
                await BetterReplyAsync("Failed to select a random user.", parameters: type);
            }
        }

        [Command("timer")]
        public async Task Timer(TimeSpan input, [Remainder]string message) {
            if (string.IsNullOrWhiteSpace(message)) {
                await BetterReplyAsync("You must set a message for the timer. e.g. `!timer 20s boo! too spoopy`");
            }

            if (input.TotalMinutes > 30) {
                await BetterReplyAsync("The timer has a maximum duration of 30 minutes. Was too lazy to add data persistence for this.", parameters: $"{input.Humanize()}; {message}");
            }

            await BetterReplyAsync($"A timer has been set for {input.Humanize()}.", parameters: $"{input.Humanize()}; {message}");

            await Task.Delay(Convert.ToInt32(input.TotalMilliseconds));

            await BetterReplyAsync(message, parameters: $"{input.Humanize()}; {message}");
        }

        // TODO Move commands below to somewhere else...

        [Command("names")]
        public async Task NameChanges(SocketUser user = null) {
            if (user == null) {
                await BetterReplyAsync("You must mention a user to see their name changes.");
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            string changes = StatisticsManager.GetUserChanges(Context.Guild.Id, user.Id);

            if (string.IsNullOrWhiteSpace(changes)) {
                await BetterReplyAsync($"There is no name change data for {BetterUserFormat(user)}. Maybe they just never changed their name. :shrug:", parameters: $"{user.ToString()} ({user.Id})");
                return;
            }

            List<string> chunks = changes.SplitIntoChunksPreserveNewLines(1930);

            if (chunks.Count == 1) {
                foreach (string chunk in chunks) {
                    await BetterReplyAsync($"● Name changes for {BetterUserFormat(user)} ```{chunk}```", parameters: $"{user.ToString()} ({user.Id})");
                }
            } else {
                Backblaze.File uploadedFile = await Backblaze.UploadTemporaryFileAsync(changes, $"{user.Id}/{Backblaze.ISOFileNameDate("txt")}");

                string message;
                if (uploadedFile is Backblaze.File) {
                    message = uploadedFile.ShortUrl;
                } else {
                    message = "There was an error uploading the temporary file.";
                }

                await BetterReplyAsync($"There are too many name changes to display here. {message}", parameters: $"{user.ToString()} ({user.Id})");
            }
        }

        [Command("topnamechangers")]
        public async Task TopNameChangers(int count = 20) {
            if (count < 5 || count > 50) {
                await BetterReplyAsync("Count must be between 5 and 50.", parameters: count.ToString());
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            List<KeyValuePair<ulong, long>> changers = StatisticsManager.GetTopChangers(count);

            StringBuilder builder = new StringBuilder();

            foreach (KeyValuePair<ulong, long> changer in changers) {
                if (Context.Guild.GetUser(changer.Key) is SocketGuildUser user) {
                    builder.AppendLine($"{Constants.ZeroWidthSpace}{changer.Value.ToString("N0").PadLeft(4)}: {BetterUserFormat(user, true)}");
                }
            }

            List<string> chunks = builder.ToString().SplitIntoChunksPreserveNewLines(1930);

            if (changers.Count == 0 || chunks.Count == 0) {
                await BetterReplyAsync("There is no top name/profile changers data. Maybe no one changed their name or profile. :shrug:", parameters: count.ToString());
                return;
            }

            foreach (string chunk in chunks) {
                await BetterReplyAsync($"● Top name/profile changers ```{chunk}```", parameters: count.ToString());
            }
        }

        [Command("avatar")]
        public async Task Avatar(SocketUser user = null) {
            if (user == null) {
                await BetterReplyAsync("You must mention a user to see their avatar.");
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            using (HttpResponseMessage response = await Http.Client.GetAsync(user.GetAvatarUrl(size: 1024))) {
                if (response.IsSuccessStatusCode) {
                    using (HttpContent content = response.Content) {
                        Stream stream = await content.ReadAsStreamAsync();

                        if (stream.Length > Constants.DiscordMaximumFileSize) {
                            await BetterReplyAsync("There was an error processing the avatar. Re-upload was too large.");
                        } else {
                            stream.Seek(0, SeekOrigin.Begin);
                            await BetterSendFileAsync(stream, $"{user.Id}_avatar.{(user.AvatarId.StartsWith("a_") ? "gif" : "png")}", $"● Avatar of {BetterUserFormat(user)}", parameters: $"{user.ToString()} ({user.Id})");
                        }
                    }
                } else {
                    await BetterReplyAsync("There was an error processing the avatar. Avatar download failed.", parameters: $"{user.ToString()} ({user.Id})");
                }
            }
        }

        [Command("emoji"), Alias("e")]
        public async Task EnlargeEmoji([Remainder]string emoji) {
            if (string.IsNullOrWhiteSpace(emoji)) {
                await BetterReplyAsync("You must provide an emote or emoji to enlarge.");
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            List<EmoteOrEmoji> items = Context.Message.Content.ParseDiscordMessageEmotes();

            foreach (EmoteOrEmoji item in items) {
                if (item.IsEmote) {
                    using (HttpResponseMessage response = await Http.Client.GetAsync($"https://cdn.discordapp.com/emojis/{item.Id}.{(item.Animated ? "gif" : "png")}")) {
                        if (response.IsSuccessStatusCode) {
                            using (HttpContent content = response.Content) {
                                Stream stream = await content.ReadAsStreamAsync();

                                if (stream.Length > Constants.DiscordMaximumFileSize) {
                                    await BetterReplyAsync("There was an error processing an emoji. Re-upload was too large.", parameters: emoji);
                                } else {
                                    stream.Seek(0, SeekOrigin.Begin);
                                    await BetterSendFileAsync(stream, $"{item.Id}_emote.{(item.Animated ? "gif" : "png")}", "", parameters: emoji);
                                }
                            }
                        } else {
                            await BetterReplyAsync("There was an error processing an emoji. Emote download failed.", parameters: emoji);
                        }
                    }
                } else {
                    List<string> hex = new List<string>();
                    foreach (int character in item.Emoji.GetUnicodeCodePoints()) {
                        hex.Add(character.ToString("x4").TrimStart(new char[] { '0' }));
                    }

                    string fullHex = string.Join("-", hex);

                    if (!string.IsNullOrEmpty(fullHex)) {
                        using (MemoryStream stream = Cache.GetStream($"twemoji_{fullHex}.png")) {
                            if (stream is MemoryStream) {
                                await BetterSendFileAsync(stream, $"{fullHex}_emoji.png", "", parameters: emoji);
                            }
                        }
                    } else {
                        await BetterReplyAsync("There was an error processing an emoji. Parse failed.", parameters: emoji);
                    }
                }
            }
        }

    }

}