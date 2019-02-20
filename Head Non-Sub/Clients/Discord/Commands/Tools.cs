using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Clients.Discord.Services;
using HeadNonSub.Extensions;

namespace HeadNonSub.Clients.Discord.Commands {

    public class Tools : ModuleBase<SocketCommandContext> {

        // https://discordapp.com/developers/docs/resources/channel#embed-limits

        [Command("ping")]
        [RequireContext(ContextType.Guild)]
        public Task PingAsync() {
            DateTime now = DateTime.Now.ToUniversalTime();

            ulong reply = ReplyAsync($"{now.Subtract(Context.Message.CreatedAt.DateTime).TotalMilliseconds.ToString("N0")}ms" +
                $"```Ping: {Context.Message.CreatedAt.DateTime.ToString(Constants.DateTimeFormat)}{Environment.NewLine}Pong: {now.ToString(Constants.DateTimeFormat)}```").Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            return Task.CompletedTask;
        }

        [Command("failfast")]
        [OwnerAdminXathz]
        [RequireContext(ContextType.Guild)]
        public Task FailFastAsync() {
            LoggingManager.Log.Fatal($"Forcibly disconnected from Discord. Server: {Context.Guild.Name} ({Context.Guild.Id}); Channel: {Context.Channel.Name} ({Context.Channel.Id}); User: {Context.User.Username} ({Context.User.Id})");

            ReplyAsync("Forcibly disconnecting from Discord, please tell <@!227088829079617536> as soon as possible. Good bye.").Wait();

            DiscordClient.FailFast();

            return Task.CompletedTask;
        }

        [Command("random")]
        [RequireContext(ContextType.Guild)]
        public Task RandomAsync([Remainder]string type = "") {
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
                randomUser = Context.Guild.Users.Where(x => x.Roles.Any(r => r.Id == 336022934621519874)).PickRandom();

            } else if (type == "tree") {
                randomUser = Context.Guild.Users.Where(x => x.Id == 339786294895050753).FirstOrDefault();

            } else if (type == "5'8\"" || type == "5'8") {
                randomUser = Context.Guild.Users.Where(x => x.Id == 177657233025400832).FirstOrDefault();

            } else {
                return ReplyAsync("**Valid roles are:** sub *(includes twitch and patreon roles)*, nonsub, tier3, admin, mod, tree, 5'8\"");
            }

            // If it is a valid user
            if (randomUser is SocketGuildUser) {
                IGuildUser contextUser = Context.User as IGuildUser;

                EmbedBuilder builder = new EmbedBuilder() {
                    Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                    Title = $"Picking a random {type}...",
                    ThumbnailUrl = "https://cdn.discordapp.com/emojis/425366701794656276.gif"
                };

                builder.Footer = new EmbedFooterBuilder() {
                    Text = $"Random user requested by {(!string.IsNullOrWhiteSpace(contextUser.Nickname) ? contextUser.Nickname : contextUser.Username)}"
                };

                IUserMessage message = ReplyAsync(embed: builder.Build()).Result;

                Task.Delay(10000).Wait();

                builder.Title = $"{(!string.IsNullOrWhiteSpace(randomUser.Nickname) ? randomUser.Nickname : randomUser.Username)} ({randomUser.ToString()})";
                builder.ThumbnailUrl = null;
                builder.Fields.Clear();

                builder.AddField("Account created", $"{randomUser.CreatedAt.DateTime.ToShortDateString()} {randomUser.CreatedAt.DateTime.ToShortTimeString()}", true);

                if (randomUser.JoinedAt.HasValue) {
                    builder.AddField("Joined server", $"{randomUser.JoinedAt.Value.DateTime.ToShortDateString()} {randomUser.JoinedAt.Value.DateTime.ToShortTimeString()}", true);
                }

                builder.AddField("Congratulations", $"{randomUser.Mention}");

                message.ModifyAsync(x => { x.Embed = builder.Build(); }).Wait();

                UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, message.Id);
            } else {
                ReplyAsync("Failed to select a random user.");
            }

            return Task.CompletedTask;
        }

        [Command("undo")]
        [RequireContext(ContextType.Guild)]
        public Task UndoAsync() {
            ulong? reply = UndoTracker.MostRecentReply(Context.Guild.Id, Context.Channel.Id, Context.User.Id);
            ulong? message = UndoTracker.MostRecentMessage(Context.Guild.Id, Context.Channel.Id, Context.User.Id);

            Context.Message.DeleteAsync().Wait();

            if (reply.HasValue) {
                Context.Channel.DeleteMessageAsync(reply.Value);
                UndoTracker.Untrack(reply.Value);
            }

            if (message.HasValue) {
                Context.Channel.DeleteMessageAsync(message.Value);
                UndoTracker.Untrack(message.Value);
            }

            return Task.CompletedTask;
        }

        [Command("servermap")]
        [OwnerAdminXathz]
        [RequireContext(ContextType.Guild)]
        public Task ServerMapAsync() {
            ulong reply;

            try {
                ServerMap map = new ServerMap(Context);
                string jsonFile = map.Generate();

                Context.User.SendFileAsync(jsonFile, $"{Context.Guild.Name} (`{Context.Guild.Id}`): Server Map");
                reply = ReplyAsync($"{Context.User.Mention} the server map was sent to you privately. The message may be blocked if you reject direct messages.").Result.Id;
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
                reply = ReplyAsync(ex.Message).Result.Id;
            }

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            return Task.CompletedTask;
        }

    }

}
