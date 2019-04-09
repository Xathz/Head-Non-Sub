using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Entities.Twitch;
using HeadNonSub.Exceptions;
using HeadNonSub.Extensions;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [BlacklistEnforced]
    [RequireContext(ContextType.Guild)]
    public class Clips : BetterModuleBase {

        [Command("randomclip")]
        public async Task RandomClip([Remainder]string channel = "paymoneywubby") {
            await Context.Channel.TriggerTypingAsync();

            string twitchChannel = channel.ToLower();

            try {
                List<Clip> clips;

                if (Cache.Get($"clips:{twitchChannel}") is List<Clip> validClips) {
                    clips = validClips;
                } else {
                    clips = await Twitch.TwitchClient.GetClipsAsync(twitchChannel);
                    Cache.Add($"clips:{twitchChannel}", clips, 30);
                }

                Clip clip = clips.PickRandom();

                if (!string.IsNullOrEmpty(clip.Url)) {
                    await BetterReplyAsync(clip.Url);
                } else {
                    await BetterReplyAsync("Failed to pick a random clip.");
                }
            } catch (UnsupportedTwitchChannelException ex) {
                await BetterReplyAsync(ex.Message);
            } catch {
                await BetterReplyAsync("Failed to pick a random clip.");
            }
        }

    }

}
