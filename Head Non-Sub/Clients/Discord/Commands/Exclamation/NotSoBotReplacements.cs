using System;
using System.Collections.Generic;
using System.IO;
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

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [BlacklistEnforced]
    [RequireContext(ContextType.Guild)]
    public class NotSoBotReplacements : BetterModuleBase {

        [Command("names")]
        public async Task NameChanges(SocketUser user = null) {
            if (user == null) {
                await BetterReplyAsync("You must mention a user to see their name changes.", parameters: "user null");
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            string changes = StatisticsManager.GetUserChanges(Context.Guild.Id, user.Id);

            if (string.IsNullOrWhiteSpace(changes)) {
                await BetterReplyAsync($"There is no name change data for {BetterUserFormat(user)}. Maybe they just never changed their name. :shrug:", parameters: $"{user} ({user.Id})");
                return;
            }

            List<string> chunks = changes.SplitIntoChunksPreserveNewLines(1930);

            if (chunks.Count == 1) {
                foreach (string chunk in chunks) {
                    await BetterReplyAsync($"● Name changes for {BetterUserFormat(user)} ```{chunk}```", parameters: $"{user} ({user.Id})");
                }
            } else {
                Backblaze.File uploadedFile = await Backblaze.UploadTemporaryFileAsync(changes, $"{user.Id}/{Backblaze.ISOFileNameDate("txt")}");

                string message;
                if (uploadedFile is Backblaze.File) {
                    message = uploadedFile.ShortUrl;
                } else {
                    message = "There was an error uploading the temporary file.";
                }

                await BetterReplyAsync($"There are too many name changes to display here. {message}", parameters: $"{user} ({user.Id})");
            }
        }

        [Command("namesid")]
        public async Task NameChangesById(string input = null) {
            if (!ulong.TryParse(input, out ulong id)) {
                await BetterReplyAsync("You must supply a valid user id.", parameters: "user null");
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            string changes = StatisticsManager.GetUserChanges(Context.Guild.Id, id);

            if (string.IsNullOrWhiteSpace(changes)) {
                await BetterReplyAsync($"There is no name change data for {id}. Maybe they just never changed their name. :shrug:", parameters: $"({id})");
                return;
            }

            List<string> chunks = changes.SplitIntoChunksPreserveNewLines(1930);

            if (chunks.Count == 1) {
                foreach (string chunk in chunks) {
                    await BetterReplyAsync($"● Name changes for {id} ```{chunk}```", parameters: $"({id})");
                }
            } else {
                Backblaze.File uploadedFile = await Backblaze.UploadTemporaryFileAsync(changes, $"{id}/{Backblaze.ISOFileNameDate("txt")}");

                string message;
                if (uploadedFile is Backblaze.File) {
                    message = uploadedFile.ShortUrl;
                } else {
                    message = "There was an error uploading the temporary file.";
                }

                await BetterReplyAsync($"There are too many name changes to display here. {message}", parameters: $"({id})");
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
                if (UserFromUserId(changer.Key) is SocketGuildUser user) {
                    builder.AppendLine($"{Constants.ZeroWidthSpace}{changer.Value,-5:N0}: {user}{(!string.IsNullOrEmpty(user.Nickname) ? $" ({user.Nickname})" : "")}");
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
        [DiscordStaffOnly]
        public async Task Avatar(SocketUser user = null) {
            if (user == null) {
                await BetterReplyAsync("You must mention a user to see their avatar.", parameters: "user null");
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            using HttpResponseMessage response = await Http.Client.GetAsync(user.GetAvatarUrl(size: 1024));

            if (response.IsSuccessStatusCode) {
                using HttpContent content = response.Content;
                Stream stream = await content.ReadAsStreamAsync();

                if (stream.Length > Constants.DiscordMaximumFileSize) {
                    await BetterReplyAsync("There was an error processing the avatar. Re-upload was too large.");
                } else {
                    string changes = StatisticsManager.GetUserAvatarChanges(user.Id);
                    List<string> chunks = changes.SplitIntoChunksPreserveNewLines(1930);
                    string message = null;

                    if (chunks.Count == 0) {
                        message = $"● Avatar of {BetterUserFormat(user)}";
                    } else if (chunks.Count == 1) {
                        foreach (string chunk in chunks) {
                            message = $"● Previous avatars of {BetterUserFormat(user)} ```{chunk}```{Environment.NewLine}Current avatar:";
                        }
                    } else {
                        Backblaze.File uploadedFile = await Backblaze.UploadTemporaryFileAsync(changes, $"{user.Id}/{Backblaze.ISOFileNameDate("txt")}");

                        if (uploadedFile is Backblaze.File) {
                            message = $"● There are too many previous avatars to list here for {BetterUserFormat(user)} <{uploadedFile.ShortUrl}>{Environment.NewLine}{Environment.NewLine}Current avatar:";
                        }
                    }

                    stream.Seek(0, SeekOrigin.Begin);
                    await BetterSendFileAsync(stream, $"{user.Id}_avatar.{(user.AvatarId.StartsWith("a_") ? "gif" : "png")}", message, parameters: $"{user} ({user.Id})");
                }
            } else {
                await BetterReplyAsync("There was an error processing the avatar. Avatar download failed.", parameters: $"{user} ({user.Id})");
            }
        }

        [Command("emoji"), Alias("e")]
        [DisallowedChannels(WubbysFunHouse.MainChannelId)]
        public async Task EnlargeEmoji([Remainder]string emoji) {
            if (string.IsNullOrWhiteSpace(emoji)) {
                await BetterReplyAsync("You must provide an emote or emoji to enlarge.");
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            List<EmoteOrEmoji> items = Context.Message.Content.ParseDiscordMessageEmotes();

            foreach (EmoteOrEmoji item in items) {
                if (item.IsEmote) {
                    using HttpResponseMessage response = await Http.Client.GetAsync($"https://cdn.discordapp.com/emojis/{item.Id}.{(item.Animated ? "gif" : "png")}");

                    if (response.IsSuccessStatusCode) {
                        using HttpContent content = response.Content;

                        Stream stream = await content.ReadAsStreamAsync();

                        if (stream.Length > Constants.DiscordMaximumFileSize) {
                            await BetterReplyAsync("There was an error processing an emoji. Re-upload was too large.", parameters: emoji);
                        } else {
                            stream.Seek(0, SeekOrigin.Begin);
                            await BetterSendFileAsync(stream, $"{item.Id}_emote.{(item.Animated ? "gif" : "png")}", "", parameters: emoji);
                        }
                    } else {
                        await BetterReplyAsync("There was an error processing an emoji. Emote download failed.", parameters: emoji);
                    }
                } else {
                    List<string> hex = new List<string>();
                    foreach (int character in item.Emoji.GetUnicodeCodePoints()) {
                        hex.Add(character.ToString("x4").TrimStart(new char[] { '0' }));
                    }

                    string fullHex = string.Join("-", hex);

                    if (!string.IsNullOrEmpty(fullHex)) {
                        using MemoryStream stream = Cache.GetStream($"twemoji_{fullHex}.png");

                        if (stream is MemoryStream) {
                            await BetterSendFileAsync(stream, $"{fullHex}_emoji.png", "", parameters: emoji);
                        }
                    } else {
                        await BetterReplyAsync("There was an error processing an emoji. Parse failed.", parameters: emoji);
                    }
                }
            }
        }

        [Command("nick")]
        [DiscordStaffOnly]
        [RequireBotPermission(GuildPermission.ManageNicknames, ErrorMessage = "I do not have the `Manage Nicknames` permission, `!nick` can not be used.")]
        public async Task SetNickname(SocketGuildUser user = null, [Remainder]string nickname = "") {
            if (user == null) {
                await BetterReplyAsync("You must mention a user to set their nickname.", parameters: $"user null; {nickname}");
                return;
            }

            if (string.IsNullOrWhiteSpace(nickname)) {
                await BetterReplyAsync($"You did not specify a nickname to set for `{user}`.", parameters: $"{user} ({user.Id}); {nickname}");
                return;
            }

            if (WubbysFunHouse.IsDiscordStaff(user)) {
                await BetterReplyAsync("You can not set the nickname of another staff member.", parameters: $"{user} ({user.Id}); {nickname}");
                return;
            }

            string orginalUserName = BetterUserFormat(user);

            await user.ModifyAsync(x => { x.Nickname = nickname; }, new RequestOptions { AuditLogReason = $"Changed by {Context.User} ({Context.User.Id})" });

            await BetterReplyAsync($"Changed nickname for {orginalUserName} to `{nickname}`.", parameters: $"{user} ({user.Id}); {nickname}");
        }

    }

}