using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Statistics;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    // Wubby's Fun House: 'actual-fucking-spam'; Cam’s Pocket: 'shitposting-cause-xathz'; Claire's Trash Pandas: 'spamalot'
    [BlacklistEnforced, AllowedChannels(537727672747294738, 546863784157904896, 553314066731499541)]
    [RequireContext(ContextType.Guild)]
    public class Rave : ModuleBase<SocketCommandContext> {

        [Command("rave")]
        [Cooldown(1800)]
        public Task RaveAsync([Remainder]string input) {
            string[] messages = input.Split(' ');

            RaveTracker.Track(Context.Guild.Id, Context.Channel.Id);

            foreach (string message in messages) {
                if (RaveTracker.IsStopped(Context.Guild.Id, Context.Channel.Id)) { return Task.CompletedTask; }

                ReplyAsync($":crab: {message} :crab:").Wait();
                Task.Delay(1250).Wait();
            }

            StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
            return Task.CompletedTask;
        }

        [Command("ravve")]
        [Cooldown(1800)]
        public Task RavveAsync([Remainder] int length = 30) {
            RaveTracker.Track(Context.Guild.Id, Context.Channel.Id);
            Random random = new Random();

            for (int i = 0; i < length; i++) {
                if (RaveTracker.IsStopped(Context.Guild.Id, Context.Channel.Id)) { return Task.CompletedTask; }

                string message = ":crab:".PadLeft(random.Next(0, 35), '.');

                ReplyAsync(message).Wait();
                Task.Delay(1250).Wait();
            }

            StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
            return Task.CompletedTask;
        }

        [Command("ravestop")]
        [Alias("stoprave", "stopraves")]
        [OwnerAdminWhitelist]
        public Task RaveStopAsync() {
            RaveTracker.Stop(Context.Guild.Id, Context.Channel.Id);

            ulong reply = ReplyAsync("Stopping all raves in this channel... you party pooper.").Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
            return Task.CompletedTask;
        }

        [Command("raveundo")]
        [Alias("undorave", "undoraves")]
        [OwnerAdminWhitelist]
        public Task RaveUndoAsync(int messageCount = 300) {
            if (messageCount == 0 || messageCount > 500) {
                return ReplyAsync("Must be between 1 and 500.");
            }

            Context.Message.DeleteAsync();

            EmbedBuilder builder = new EmbedBuilder() {
                Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                Title = "Undoing raves...",
                Description = $"Deleting up to {messageCount} rave messages",
                ThumbnailUrl = "https://cdn.discordapp.com/emojis/425366701794656276.gif"
            };

            IUserMessage noticeMessage = ReplyAsync(embed: builder.Build()).Result;

            if (Context.Channel is SocketTextChannel channel) {
                IAsyncEnumerable<IMessage> messages = channel.GetMessagesAsync(500).Flatten();

                IAsyncEnumerable<IMessage> toDelete = messages.Where(x => (x.Author.Id == Context.Guild.CurrentUser.Id) &
                (x.Content.StartsWith(":crab:")) || (x.Content.StartsWith(".") & x.Content.Contains(":crab:")))
                .OrderByDescending(x => x.CreatedAt).Take(messageCount);

                channel.DeleteMessagesAsync(toDelete.ToEnumerable()).Wait();
            }

            noticeMessage.DeleteAsync();

            StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
            return Task.CompletedTask;
        }

    }

}