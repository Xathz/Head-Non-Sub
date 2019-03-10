using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Extensions;
using HeadNonSub.Statistics;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [BlacklistEnforced]
    [RequireContext(ContextType.Guild)]
    public class Clips : ModuleBase<SocketCommandContext> {

        [Command("randomclip")]
        public Task RandomClipAsync() {
            List<(DateTime createdAt, string title, int viewCount, string url)> clips;

            if (Cache.Get("clips:paymoneywubby") is List<(DateTime createdAt, string title, int viewCount, string url)> validClips) {
                clips = validClips;
            } else {
                clips = Twitch.TwitchClient.GetClips("paymoneywubby");
                Cache.Add("clips:paymoneywubby", clips, 30);
            }

            (DateTime createdAt, string title, int viewCount, string url) = clips.PickRandom();

            ulong reply;
            if (!string.IsNullOrEmpty(url)) {
                reply = ReplyAsync(clips.PickRandom().url).Result.Id;
            } else {
                reply = ReplyAsync("Failed to pick a random clip.").Result.Id;
            }

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
            return Task.CompletedTask;
        }

    }

}
