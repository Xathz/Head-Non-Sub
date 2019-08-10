using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;
using Humanizer;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [BlacklistEnforced, AllowedChannels(WubbysFunHouse.ActualFuckingSpamChannelId)]
    [RequireContext(ContextType.Guild)]
    public class Raves : BetterModuleBase {

        [Command("rave")]
        [Cooldown(1800)]
        [SubscriberOnly]
        public async Task Rave([Remainder]string input) {
            await BetterReplyAsync("The `rave` command has been retired... for now.");
            return;

            RaveTracker.Track(Context.Channel.Id);
            DateTimeOffset start = DateTimeOffset.UtcNow;
            string[] messages = input.Split(' ');

            foreach (string message in messages) {
                if (RaveTracker.GetStatus(Context.Channel.Id) != RaveTracker.Status.Running) { break; }

                await BetterReplyAsync($":crab: {message} :crab:", parameters: message);
                await Task.Delay(1400);
            }

            string runtime = (start - DateTimeOffset.UtcNow).TotalMilliseconds.Milliseconds().Humanize();
            await BetterReplyAsync($"{BetterUserFormat()} rave lasted for {runtime} and had {messages.Count()} messages.");
        }

        [Command("ravve")]
        [Cooldown(1800)]
        [SubscriberOnly]
        public async Task Ravve([Remainder] int length = 30) {
            await BetterReplyAsync("The `ravve` command has been retired... for now.");
            return;

            RaveTracker.Track(Context.Channel.Id);
            DateTimeOffset start = DateTimeOffset.UtcNow;
            Random random = new Random();

            for (int i = 0; i < length; i++) {
                if (RaveTracker.GetStatus(Context.Channel.Id) != RaveTracker.Status.Running) { break; }

                string message = ":crab:".PadLeft(random.Next(0, 35), '.');

                await BetterReplyAsync(message, parameters: message);
                await Task.Delay(1400);
            }

            string runtime = (start - DateTimeOffset.UtcNow).TotalMilliseconds.Milliseconds().Humanize();
            await BetterReplyAsync($"{BetterUserFormat()} ravve lasted for {runtime} and had {length} messages.");
        }

        [Command("ravestop"), Alias("stoprave", "stopraves")]
        [DiscordStaffOnly]
        public Task RaveStop() {
            return BetterReplyAsync("The `ravestop`, `stoprave`, and `stopraves` commands have been retired... for now.");

            RaveTracker.Stop(Context.Channel.Id);

            return BetterReplyAsync("Stopping all raves in this channel... you party pooper.");
        }

        [Command("raveundo"), Alias("undorave", "undoraves")]
        [DiscordStaffOnly]
        public async Task RaveUndo(int messageCount = 300) {
            await BetterReplyAsync("The `raveundo`, `undorave`, and `undoraves` commands have been retired... for now.");
            return;

            if (messageCount == 0 || messageCount > 500) {
                await BetterReplyAsync("Must be between 1 and 500.", messageCount.ToString());
                return;
            }

            await Context.Message.DeleteAsync();

            EmbedBuilder builder = new EmbedBuilder() {
                Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                Title = "Undoing Raves",
                Description = $"Deleting up to {messageCount} rave messages",
                ThumbnailUrl = Constants.LoadingGifUrl
            };

            IUserMessage noticeMessage = await BetterReplyAsync(builder.Build(), messageCount.ToString());

            try {
                if (Context.Channel is SocketTextChannel channel) {
                    IAsyncEnumerable<IMessage> messages = channel.GetMessagesAsync(500).Flatten();

                    IEnumerable<IMessage> toDelete = messages.Where(x => (x.Author.Id == Context.Guild.CurrentUser.Id) &
                    (x.Content.StartsWith(":crab:")) || (x.Content.StartsWith(".") & x.Content.Contains(":crab:")))
                    .OrderByDescending(x => x.CreatedAt).Take(messageCount).ToEnumerable();

                    await channel.DeleteMessagesAsync(toDelete);
                }
            } catch {
                await BetterReplyAsync("Failed to delete messages.");
            } finally {
                await noticeMessage.DeleteAsync();
            }
        }

    }

}