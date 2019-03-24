using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Clients.Discord.Services;
using HeadNonSub.Extensions;
using HeadNonSub.Statistics;

namespace HeadNonSub.Clients.Discord.Commands {

    [BlacklistEnforced]
    [RequireContext(ContextType.Guild)]
    public class Tools : BetterModuleBase {

        [Command("ping")]
        public Task Ping() {
            DateTime now = DateTime.Now.ToUniversalTime();

            return BetterReplyAsync($"{now.Subtract(Context.Message.CreatedAt.DateTime).TotalMilliseconds.ToString("N0")}ms" +
                $"```Ping: {Context.Message.CreatedAt.DateTime.ToString(Constants.DateTimeFormat)}{Environment.NewLine}Pong: {now.ToString(Constants.DateTimeFormat)}```");
        }

        [Command("failfast")]
        [OwnerAdminXathz]
        public Task FailFast() {
            LoggingManager.Log.Fatal($"Forcibly disconnected from Discord. Server: {Context.Guild.Name} ({Context.Guild.Id}); Channel: {Context.Channel.Name} ({Context.Channel.Id}); User: {Context.User.Username} ({Context.User.Id})");
            BetterReplyAsync("Forcibly disconnecting from Discord, please tell <@!227088829079617536> as soon as possible. Good bye.").Wait();

            DiscordClient.FailFast();
            return Task.CompletedTask;
        }

        [Command("random")]
        public Task Random([Remainder]string type = "") {
            SocketGuildUser randomUser = null;

            if (type == "sub") {
                randomUser = Context.Guild.Users.Where(x => x.Roles.Any(r => r.Id == 428052879371272192 || r.Id == 328732005024137217)).PickRandom();

            } else if (type == "non-sub" || type == "nonsub") {
                randomUser = Context.Guild.Users.Where(x => x.Roles.Any(r => r.Id == 508752510216044547)).PickRandom();

            } else if (type == "tier3" || type == "t3") {
                randomUser = Context.Guild.Users.Where(x => x.Roles.Any(r => r.Id == 493641643765792768)).PickRandom();

            } else if (type == "admin") {
                randomUser = Context.Guild.Users.Where(x => x.Roles.Any(r => r.Id == 372244721625464845)).PickRandom();

            } else if (type == "mod") {
                randomUser = Context.Guild.Users.Where(x => x.Roles.Any(r => r.Id == 336022934621519874 | r.Id == 497550793923362827 | r.Id == 468960777978642443 | r.Id == 542135412106330122)
                                        & !x.Roles.Any(r => r.Id == 372244721625464845 | r.Id == 465872398772862976)).PickRandom();

            } else if (type == "tree") {
                randomUser = Context.Guild.Users.Where(x => x.Id == 339786294895050753).FirstOrDefault();

            } else if (type == "5'8\"" || type == "5'8") {
                randomUser = Context.Guild.Users.Where(x => x.Id == 177657233025400832).FirstOrDefault();

            } else {
                return BetterReplyAsync("**Valid roles are:** sub *(includes twitch and patreon roles)*, nonsub, tier3, admin, mod, tree, 5'8\"", parameters: type);
            }

            // If it is a valid user
            if (randomUser is SocketGuildUser) {
                IGuildUser contextUser = Context.User as IGuildUser;

                EmbedBuilder builder = new EmbedBuilder() {
                    Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                    Title = $"Picking a random {type}...",
                    ThumbnailUrl = "https://cdn.discordapp.com/attachments/338137121166721026/559460210155192330/Loading.gif"
                };

                builder.Footer = new EmbedFooterBuilder() {
                    Text = $"Random user requested by {BetterUserFormat(useGrave: false)}"
                };

                IUserMessage message = BetterReplyAsync(embed: builder.Build(), parameters: type).Result;

                Task.Delay(8000).Wait();

                builder.Title = BetterUserFormat(randomUser);
                builder.ThumbnailUrl = null;
                builder.Fields.Clear();

                builder.AddField("Account created", $"{randomUser.CreatedAt.DateTime.ToShortDateString()} {randomUser.CreatedAt.DateTime.ToShortTimeString()}", true);

                if (randomUser.JoinedAt.HasValue) {
                    builder.AddField("Joined server", $"{randomUser.JoinedAt.Value.DateTime.ToShortDateString()} {randomUser.JoinedAt.Value.DateTime.ToShortTimeString()}", true);
                }

                message.ModifyAsync(x => { x.Embed = builder.Build(); }).Wait();

            } else {
                _ = BetterReplyAsync("Failed to select a random user.");
            }

            return Task.CompletedTask;
        }

        [Command("undo")]
        public Task Undo([Remainder]int count = 1) {
            List<ulong> toDelete = new List<ulong> { Context.Message.Id };

            if (Context.Channel is SocketTextChannel channel) {
                foreach (ulong? message in StatisticsManager.UndoMessages(Context.Channel.Id, Context.User.Id, count)) {
                    if (message.HasValue) {
                        toDelete.Add(message.Value);
                    }
                }

                channel.DeleteMessagesAsync(toDelete).Wait();
            }

            TrackStatistics(parameters: count.ToString());
            return Task.CompletedTask;
        }

        [Command("undobot")]
        [OwnerAdminWhitelist]
        public Task UndoBot(int messageCount = 100) {
            if (messageCount == 0 || messageCount > 500) {
                return BetterReplyAsync("Must be between 1 and 500.", messageCount.ToString());
            }

            Context.Message.DeleteAsync();

            EmbedBuilder builder = new EmbedBuilder() {
                Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                Title = $"Undoing {Context.Client.CurrentUser.Username} messages...",
                Description = $"Deleting up to {messageCount} bot messages",
                ThumbnailUrl = "https://cdn.discordapp.com/attachments/338137121166721026/559460210155192330/Loading.gif"
            };

            IUserMessage noticeMessage = BetterReplyAsync(builder.Build(), messageCount.ToString()).Result;

            try {
                if (Context.Channel is SocketTextChannel channel) {
                    IAsyncEnumerable<IMessage> messages = channel.GetMessagesAsync(500).Flatten();
                    IAsyncEnumerable<IMessage> toDelete = messages.Where(x => (x.Author.Id == Context.Client.CurrentUser.Id)).OrderByDescending(x => x.CreatedAt).Take(messageCount);

                    channel.DeleteMessagesAsync(toDelete.ToEnumerable()).Wait();
                }

                noticeMessage.DeleteAsync();
            } catch {
                return BetterReplyAsync("Failed to delete messages.");
            } finally {
                noticeMessage.DeleteAsync();
            }

            return Task.CompletedTask;
        }

        [Command("servermap")]
        [OwnerAdminXathz]
        public Task ServerMap() {
            try {
                ServerMap map = new ServerMap(Context);
                string jsonFile = map.Generate();

                Context.User.SendFileAsync(jsonFile, $"{Context.Guild.Name} (`{Context.Guild.Id}`): Server Map").Wait();

                return BetterReplyAsync($"{Context.User.Mention} the server map was sent to you privately. The message may be blocked if you reject direct messages.");
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
                return BetterReplyAsync("Failed to generate the server map.");
            }
        }

    }

}
