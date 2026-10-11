using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Entities.Discord;
using HeadNonSub.Extensions;

namespace HeadNonSub.Clients.Discord.Commands {

    [BlacklistEnforced]
    [CommandContextType(InteractionContextType.Guild)]
    public class Emojis : BetterModuleBase {

        private static readonly char[] _TrimChars = new char[] { '0' };

        private const int _MaxEmoji = 5;

        [SlashCommand("emoji", "Enlarge emotes or emoji, up to 5")]
        [DisallowedChannels(WubbysFunHouse.MainChannelId)]
        [Cooldown(10, true)]
        public async Task EnlargeEmoji([Summary("emoji", "Emotes or emoji to enlarge")] string emoji) {
            // Limit how many files one command can make the bot upload
            List<EmoteOrEmoji> items = emoji.ParseDiscordMessageEmotes().Take(_MaxEmoji).ToList();

            if (items.Count == 0) {
                await EphemeralReplyAsync("You must provide an emote or emoji to enlarge.");
                return;
            }

            await DeferAsync();

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
                        await BetterSendFileAsync(stream, $"{fullHex}_emoji.png", "", parameters: emoji);
                    } else {
                        await BetterReplyAsync("There was an error processing an emoji. Parse failed.", parameters: emoji);
                    }
                }
            }
        }

    }

}
