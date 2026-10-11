using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord.Commands;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Entities.Discord;
using HeadNonSub.Extensions;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [BlacklistEnforced]
    [RequireContext(ContextType.Guild)]
    public class NotSoBotReplacements : BetterModuleBase {

        private static readonly char[] _TrimChars = new char[] { '0' };

        private const int _MaxEmoji = 5;

        [Command("emoji"), Alias("e")]
        [DisallowedChannels(WubbysFunHouse.MainChannelId)]
        [Cooldown(10, true)]
        public async Task EnlargeEmoji([Remainder] string emoji) {
            if (string.IsNullOrWhiteSpace(emoji)) {
                await BetterReplyAsync("You must provide an emote or emoji to enlarge.");
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            // Limit how many files one message can make the bot upload
            List<EmoteOrEmoji> items = Context.Message.Content.ParseDiscordMessageEmotes().Take(_MaxEmoji).ToList();

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
                        hex.Add(character.ToString("x4").TrimStart(_TrimChars));
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
    }

}
