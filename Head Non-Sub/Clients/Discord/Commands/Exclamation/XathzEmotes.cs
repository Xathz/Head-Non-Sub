using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Extensions;
using HeadNonSub.Settings;
using XathzEmotesEntities = HeadNonSub.Entities.Xathz.Emotes;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [RequireContext(ContextType.Guild)]
    public class XathzEmotes : BetterModuleBase {

        [Command("searchemotes")]
        [XathzOnly]
        public async Task SearchEmotes([Remainder] string search) {
            await Context.Channel.TriggerTypingAsync();
            search = search.Replace(" ", "").Trim().ToLowerInvariant();

            XathzEmotesEntities.XathzEmotes emotes = null;

            if (Cache.Get($"xathz_emotes_search_{search}") is XathzEmotesEntities.XathzEmotes cachedEmotes) {
                emotes = cachedEmotes;
            } else {
                Task<string> download = Http.SendRequestAsync($"https://emotes.xathz.net/api/?key={SettingsManager.Configuration.XathzEmotesKey}&search={search}");
                string data = await download;

                if (download.IsCompletedSuccessfully) {
                    XathzEmotesEntities.XathzEmotes downloadedEmotes = Http.DeserializeJson<XathzEmotesEntities.XathzEmotes>(data);
                    Cache.AddOrUpdate($"xathz_emotes_search_{search}", downloadedEmotes, 60);

                    emotes = downloadedEmotes;
                }
            }

            if (emotes is null) {
                await BetterReplyAsync("There was an unknown error searching emotes.", parameters: search);
                return;
            }

            XathzEmotesEntities.Emote randomEmote = emotes.Emotes.PickRandom();
            string[] randomEmoteNames = randomEmote.Names.Split(",");

            string names;
            if (randomEmoteNames.Length == 1) {
                names = $"**{randomEmoteNames[0]}**";
            } else {
                names = $"**{randomEmoteNames[0]}** aka: {string.Join(", ", randomEmoteNames.Skip(1).ToArray())}";
            }

            await BetterReplyAsync($"{names}\nhttps://emotes.xathz.net/files/{randomEmote.File}", parameters: search);

        }

    }

}
