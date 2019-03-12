using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Exceptions;
using HeadNonSub.Extensions;
using HeadNonSub.Statistics;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [BlacklistEnforced]
    [RequireContext(ContextType.Guild)]
    public class Clips : ModuleBase<SocketCommandContext> {

        [Command("randomclip")]
        public Task RandomClipAsync([Remainder]string channel = "") {
            string twitchChannel = channel.ToLower();
            string replyMessage;
            ulong reply;

            // TODO Come up with a better way to do this
            if (string.IsNullOrEmpty(twitchChannel)) {
                if (Context.Guild.Id == 328300333010911242) { // Wubby's Fun House
                    twitchChannel = "paymoneywubby";
                } else if (Context.Guild.Id == 471045301407449088) { // Claire's Trash Pandas
                    twitchChannel = "clairebere";
                }
            }

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
                    replyMessage = clips.PickRandom().url;
                } else {
                    replyMessage = "Failed to pick a random clip.";
                }
            } catch (UnsupportedTwitchChannelException ex) {
                replyMessage = ex.Message;
            } catch {
                replyMessage = "Failed to pick a random clip.";
            }

            reply = ReplyAsync(replyMessage).Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
            return Task.CompletedTask;
        }

    }

}
