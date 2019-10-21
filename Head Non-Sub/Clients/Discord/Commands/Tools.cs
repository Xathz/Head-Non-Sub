using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Clients.Discord.Services;
using HeadNonSub.Entities.Discord;
using HeadNonSub.Extensions;
using HeadNonSub.Statistics;
using Humanizer;

namespace HeadNonSub.Clients.Discord.Commands {

    [BlacklistEnforced]
    [RequireContext(ContextType.Guild)]
    public class Tools : BetterModuleBase {

        [Command("ping")]
        public Task Ping() {
            DateTime now = DateTime.Now.ToUniversalTime();

            return BetterReplyAsync($"{now.Subtract(Context.Message.CreatedAt.DateTime).TotalMilliseconds.ToString("N0")}ms" +
                $"```Ping: {Context.Message.CreatedAt.DateTime.ToString(Constants.DateTimeFormatFull)}{Environment.NewLine}Pong: {now.ToString(Constants.DateTimeFormatFull)}```");
        }

        [Command("failfast")]
        [DiscordStaffOnly]
        public Task FailFast() => BetterReplyAsync($"The failfast command has been disabled. Contact <@{Constants.XathzUserId}> for help.");

        [Command("undo")]
        public async Task Undo(int count = 1) {
            List<ulong> toDelete = new List<ulong> { Context.Message.Id };

            if (Context.Channel is SocketTextChannel channel) {
                foreach (ulong messageId in StatisticsManager.UndoMessages(Context.Channel.Id, Context.User.Id, count)) {
                    toDelete.Add(messageId);
                }

                await channel.DeleteMessagesAsync(toDelete);
            }

            TrackStatistics(parameters: count.ToString());
        }

        [Command("undobot"), Alias("botundo")]
        [DiscordStaffOnly]
        public async Task UndoBot(int count = 100) {
            if (count == 0 || count > 500) {
                await BetterReplyAsync("Must be between 1 and 500.", parameters: count.ToString());
            }

            EmbedBuilder builder = new EmbedBuilder() {
                Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                Title = $"Undoing {Context.Client.CurrentUser.Username} Messages",
                Description = $"Deleting up to {count} bot messages",
                ThumbnailUrl = Constants.LoadingGifUrl
            };

            IUserMessage noticeMessage = await BetterReplyAsync(builder.Build(), parameters: count.ToString());

            try {
                List<IMessage> toDelete = new List<IMessage> { Context.Message };

                if (Context.Channel is SocketTextChannel channel) {
                    IAsyncEnumerable<IMessage> messages = channel.GetMessagesAsync(500).Flatten();
                    IEnumerable<IMessage> foundToDelete = messages.Where(x => (x.Author.Id == Context.Client.CurrentUser.Id)).OrderByDescending(x => x.CreatedAt).Take(count).ToEnumerable();
                    toDelete.AddRange(foundToDelete);

                    await channel.DeleteMessagesAsync(toDelete);
                }

                await LogMessageEmbedAsync($"Undo bot executed `@{Context.Guild.CurrentUser.ToString()} undobot`", $"{count} messages were requested to be deleted, {toDelete.Count} were deleted.");
            } catch { }

            await noticeMessage.DeleteAsync();
        }

        [Command("undoemotes"), Alias("undoemoji")]
        [DiscordStaffOnly]
        public async Task UndoEmotes(int count = 100) {
            if (count == 0 || count > 500) {
                await BetterReplyAsync("Must be between 1 and 500.", parameters: count.ToString());
            }

            EmbedBuilder builder = new EmbedBuilder() {
                Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                Title = $"Undoing Emote/Emoji Only Messages",
                Description = $"Deleting up to {count} messages",
                ThumbnailUrl = Constants.LoadingGifUrl
            };

            IUserMessage noticeMessage = await BetterReplyAsync(builder.Build(), count.ToString());

            try {
                List<IMessage> toDelete = new List<IMessage> { Context.Message };

                if (Context.Channel is SocketTextChannel channel) {
                    IAsyncEnumerable<IMessage> messages = channel.GetMessagesAsync(500).Flatten();
                    IEnumerable<IMessage> foundToDelete = messages.Where(x => {
                        List<EmoteOrEmoji> emotes = x.Content.ParseDiscordMessageEmotes();

                        if (emotes.Count > 0) {
                            string content = x.Content;

                            foreach (EmoteOrEmoji emote in emotes) {
                                content = content.Replace(emote.ToString(), "").Trim();
                            }

                            return content.Length <= 2;
                        } else {
                            return false;
                        }
                    }).OrderByDescending(x => x.CreatedAt).Take(count).ToEnumerable();
                    toDelete.AddRange(foundToDelete);

                    await channel.DeleteMessagesAsync(toDelete);

                    await LogMessageEmbedAsync($"Undo emotes executed `@{Context.Guild.CurrentUser.ToString()} undoemotes`", $"{count} emotes were requested to be deleted, {toDelete.Count} were deleted.");
                }
            } catch { }

            await noticeMessage.DeleteAsync();
        }

        [Command("emotemode")]
        [DiscordStaffOnly]
        public async Task EmoteMode([Remainder]string mode = "") {
            if (mode == "off") {
                EmoteModeTracker.RemoveMode(Context.Channel.Id);

                EmbedBuilder builder = new EmbedBuilder() {
                    Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                    Title = "Emote Mode ● None",
                    Description = "This channel has returned to normal."
                };

                await LogMessageEmbedAsync($"Emote mode executed `@{Context.Guild.CurrentUser.ToString()} emotemode off`", "**Mode:** Off");
                await BetterReplyAsync(builder.Build(), parameters: mode);

            } else if (mode == "textonly") {
                EmoteModeTracker.SetMode(Context.Channel.Id, EmoteModeTracker.Mode.TextOnly);

                EmbedBuilder builder = new EmbedBuilder() {
                    Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                    Title = "Emote Mode ● Text Only",
                    Description = "All messages with emotes/emoji will be deleted."
                };

                await LogMessageEmbedAsync($"Emote mode executed `@{Context.Guild.CurrentUser.ToString()} emotemode textonly`", "**Mode:** Text Only");
                await BetterReplyAsync(builder.Build(), parameters: mode);

            } else if (mode == "emoteonly") {
                EmoteModeTracker.SetMode(Context.Channel.Id, EmoteModeTracker.Mode.EmoteOnly);

                EmbedBuilder builder = new EmbedBuilder() {
                    Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                    Title = "Emote Mode ● Emote Only",
                    Description = "All messages with text will be deleted."
                };

                await LogMessageEmbedAsync($"Emote mode executed `@{Context.Guild.CurrentUser.ToString()} emotemode emoteonly`", "**Mode:** Emote Only");
                await BetterReplyAsync(builder.Build(), parameters: mode);

            } else {
                await BetterReplyAsync("**Valid modes are:** off, textonly, emoteonly", parameters: mode);
            }
        }

        [Command("servermap")]
        [DiscordStaffOnly]
        public async Task ServerMap() {
            try {
                ServerMap map = new ServerMap(Context);
                string jsonFile = map.Generate();

                await Context.User.SendFileAsync(jsonFile, $"{Context.Guild.Name} (`{Context.Guild.Id}`): Server Map");

                await BetterReplyAsync($"{Context.User.Mention} the server map was sent to you privately. The message may be blocked if you reject direct messages.");
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
                await BetterReplyAsync("Failed to generate the server map.");
            }

            await LogMessageEmbedAsync($"Server map executed `@{Context.Guild.CurrentUser.ToString()} servermap`");
        }

        [Command("membermap")]
        [DiscordStaffOnly]
        public async Task MemberMap() {
            try {
                MemberMap map = new MemberMap(Context);
                string jsonFile = map.Generate();

                await Context.User.SendFileAsync(jsonFile, $"{Context.Guild.Name} (`{Context.Guild.Id}`): Member Map");

                await BetterReplyAsync($"{Context.User.Mention} the member map was sent to you privately. The message may be blocked if you reject direct messages.");
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
                await BetterReplyAsync("Failed to generate the member map.");
            }

            await LogMessageEmbedAsync($"Member map executed `@{Context.Guild.CurrentUser.ToString()} membermap`");
        }

        [Command("emotes")]
        [DiscordStaffOnly]
        public async Task Emotes() {
            IReadOnlyCollection<GuildEmote> emotes = Context.Guild.Emotes;
            StringBuilder builder = new StringBuilder();

            foreach (GuildEmote emote in emotes) {
                List<string> allowedRoleNames = new List<string>();
                foreach (ulong id in emote.RoleIds) {
                    allowedRoleNames.Add(Context.Guild.Roles.FirstOrDefault(x => x.Id == id).Name);
                }
                string allowedRoles = allowedRoleNames.Count > 0 ? string.Join(", ", allowedRoleNames) : "Everyone";

                List<string> flagsList = new List<string>();
                if (emote.Animated) { flagsList.Add("Animated"); }
                if (emote.IsManaged) { flagsList.Add("Managed"); }

                builder.AppendLine($"{emote.Name} ({emote.Id}) {emote.CreatedAt.DateTime.ToString(Constants.DateTimeFormatShort).ToLower()} utc ● {(flagsList.Count > 0 ? $"{string.Join(", ", flagsList)} ● " : "")}{allowedRoles}");
            }

            List<string> chunks = builder.ToString().SplitIntoChunksPreserveNewLines(1950);

            foreach (string chunk in chunks) {
                await BetterReplyAsync($"```{chunk}```");
            }
        }

        [Command("roles")]
        [DiscordStaffOnly]
        public async Task Roles() {
            List<SocketRole> roles = Context.Guild.Roles.OrderByDescending(x => x.Position).ToList();
            StringBuilder builder = new StringBuilder();

            foreach (SocketRole role in roles) {
                builder.AppendLine($"{role.Name.Truncate(16, "...").PadRight(16)} {role.Id} {role.CreatedAt.DateTime.ToString(Constants.DateTimeFormatShort).ToLower()} utc {role.Color.ToString()}");
            }

            List<string> chunks = builder.ToString().SplitIntoChunksPreserveNewLines(1950);

            foreach (string chunk in chunks) {
                await BetterReplyAsync($"```{chunk}```");
            }
        }

    }

}
