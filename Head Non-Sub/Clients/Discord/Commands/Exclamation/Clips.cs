using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Entities.Twitch;
using HeadNonSub.Exceptions;
using HeadNonSub.Extensions;
using HeadNonSub.Settings;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [BlacklistEnforced]
    [RequireContext(ContextType.Guild)]
    public class Clips : BetterModuleBase {

        [Command("randomclip")]
        public async Task RandomClip() {
            await Context.Channel.TriggerTypingAsync();

            try {
                List<Clip> clips;

                if (Cache.Get($"clips:{SettingsManager.Configuration.TwitchStream.UserId}") is List<Clip> validClips) {
                    clips = validClips;
                } else {
                    clips = await Twitch.TwitchClient.GetClipsAsync(SettingsManager.Configuration.TwitchStream.UserId, SettingsManager.Configuration.TwitchStream.DisplayName);
                    Cache.Add($"clips:{SettingsManager.Configuration.TwitchStream.UserId}", clips, 30);
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
