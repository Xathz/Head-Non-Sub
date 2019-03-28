using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [BlacklistEnforced, AllowedChannels(WubbysFunHouse.ActualFuckingSpamChannelId)]
    [RequireContext(ContextType.Guild)]
    public class Raves : BetterModuleBase {

        [Command("rave")]
        [Cooldown(1800)]
        [SubscriberOnly]
        public Task Rave([Remainder]string input) {
            string[] messages = input.Split(' ');

            RaveTracker.Track(Context.Guild.Id, Context.Channel.Id);

            foreach (string message in messages) {
                if (RaveTracker.IsStopped(Context.Guild.Id, Context.Channel.Id)) { return Task.CompletedTask; }

                ReplyAsync($":crab: {message} :crab:").Wait();
                Task.Delay(1250).Wait();
            }

            TrackStatistics(parameters: input);
            return Task.CompletedTask;
        }

        [Command("ravve")]
        [Cooldown(1800)]
        [SubscriberOnly]
        public Task Ravve([Remainder] int length = 30) {
            RaveTracker.Track(Context.Guild.Id, Context.Channel.Id);
            Random random = new Random();

            for (int i = 0; i < length; i++) {
                if (RaveTracker.IsStopped(Context.Guild.Id, Context.Channel.Id)) { return Task.CompletedTask; }

                string message = ":crab:".PadLeft(random.Next(0, 35), '.');

                ReplyAsync(message).Wait();
                Task.Delay(1250).Wait();
            }

            TrackStatistics(parameters: length.ToString());
            return Task.CompletedTask;
        }

        [Command("ravestop"), Alias("stoprave", "stopraves")]
        [DiscordStaffOnly]
        public Task RaveStop() {
            RaveTracker.Stop(Context.Guild.Id, Context.Channel.Id);

            return BetterReplyAsync("Stopping all raves in this channel... you party pooper.");
        }

        [Command("raveundo"), Alias("undorave", "undoraves")]
        [DiscordStaffOnly]
        public Task RaveUndo(int messageCount = 300) {
            if (messageCount == 0 || messageCount > 500) {
                return BetterReplyAsync("Must be between 1 and 500.", messageCount.ToString());
            }

            Context.Message.DeleteAsync();

            EmbedBuilder builder = new EmbedBuilder() {
                Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                Title = "Undoing raves...",
                Description = $"Deleting up to {messageCount} rave messages",
                ThumbnailUrl = Constants.LoadingGifUrl
            };

            IUserMessage noticeMessage = BetterReplyAsync(builder.Build(), messageCount.ToString()).Result;

            try {
                if (Context.Channel is SocketTextChannel channel) {
                    IAsyncEnumerable<IMessage> messages = channel.GetMessagesAsync(500).Flatten();

                    IEnumerable<IMessage> toDelete = messages.Where(x => (x.Author.Id == Context.Guild.CurrentUser.Id) &
                    (x.Content.StartsWith(":crab:")) || (x.Content.StartsWith(".") & x.Content.Contains(":crab:")))
                    .OrderByDescending(x => x.CreatedAt).Take(messageCount).ToEnumerable();

                    channel.DeleteMessagesAsync(toDelete).Wait();
                }
            } catch {
                return BetterReplyAsync("Failed to delete messages.");
            } finally {
                noticeMessage.DeleteAsync();
            }

            return Task.CompletedTask;
        }

    }

}