using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Exceptions;
using HeadNonSub.Extensions;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [BlacklistEnforced]
    [RequireContext(ContextType.Guild)]
    public class Clips : BetterModuleBase {

        [Command("randomclip")]
        public Task RandomClip([Remainder]string channel = "paymoneywubby") {
            Context.Channel.TriggerTypingAsync();

            string twitchChannel = channel.ToLower();

            try {
                List<(DateTime createdAt, string title, int viewCount, string url)> clips;

                if (Cache.Get($"clips:{twitchChannel}") is List<(DateTime createdAt, string title, int viewCount, string url)> validClips) {
                    clips = validClips;
                } else {
                    clips = Twitch.TwitchClient.GetClips(twitchChannel);
                    Cache.Add($"clips:{twitchChannel}", clips, 30);
                }

                (DateTime createdAt, string title, int viewCount, string url) = clips.PickRandom();

                if (!string.IsNullOrEmpty(url)) {
                    return BetterReplyAsync(clips.PickRandom().url);
                } else {
                    return BetterReplyAsync("Failed to pick a random clip.");
                }
            } catch (UnsupportedTwitchChannelException ex) {
                return BetterReplyAsync(ex.Message);
            } catch {
               return BetterReplyAsync("Failed to pick a random clip.");
            }
        }

    }

}
