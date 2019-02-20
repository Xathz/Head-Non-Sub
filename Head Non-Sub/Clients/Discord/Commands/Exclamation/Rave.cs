using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    // https://discordapp.com/developers/docs/resources/channel#embed-limits

    public class Rave : ModuleBase<SocketCommandContext> {

        [Command("rave")]
        [Cooldown(1800)]
        [RequireContext(ContextType.Guild)]
        public Task RaveAsync([Remainder]string input) {

            // Wubby's Fun House
            if (Context.Guild.Id == 328300333010911242) {
                // 'actual-fucking-spam'
                if (!(Context.Channel.Id == 537727672747294738)) {
                    ReplyAsync($"`!rave` is only usable in <#537727672747294738>.");
                    return Task.FromException(new UnauthorizedAccessException("Not a valid channel for command."));
                }
            }

            // Cam’s pocket
            if (Context.Guild.Id == 528475747334225925) {
                // 'shitposting-cause-xathz'
                if (Context.Channel.Id != 546863784157904896) {
                    ReplyAsync($"`!rave` is only usable in <#546863784157904896>.");
                    return Task.FromException(new UnauthorizedAccessException("Not a valid channel for command."));
                }
            }

            string[] messages = input.Split(' ');

            RaveTracker.Track(Context.Guild.Id, Context.Channel.Id);

            foreach (string message in messages) {
                if (RaveTracker.IsStopped(Context.Guild.Id, Context.Channel.Id)) { return Task.CompletedTask; }

                ReplyAsync($":crab: {message} :crab:").Wait();
                Task.Delay(1250).Wait();
            }

            return Task.CompletedTask;
        }

        [Command("ravestop")]
        [Alias("stoprave", "stopraves")]
        [OwnerAdminWhitelist]
        [RequireContext(ContextType.Guild)]
        public Task RaveStopAsync() {
            RaveTracker.Stop(Context.Guild.Id, Context.Channel.Id);

            ulong reply = ReplyAsync("Stopping all raves in this channel... you party pooper.").Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            return Task.CompletedTask;
        }

        [Command("raveundo")]
        [Alias("undorave", "undoraves")]
        [OwnerAdminWhitelist]
        [RequireContext(ContextType.Guild)]
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

                IAsyncEnumerable<IMessage> toDelete = messages.Where(x => x.Author.Id == Context.Guild.CurrentUser.Id & x.Content.StartsWith(":crab:")).OrderByDescending(x => x.CreatedAt).Take(messageCount);
                channel.DeleteMessagesAsync(toDelete.ToEnumerable()).Wait();
            }

            noticeMessage.DeleteAsync();

            return Task.CompletedTask;
        }

    }

}