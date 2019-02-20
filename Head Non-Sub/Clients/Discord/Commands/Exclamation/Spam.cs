using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    // https://discordapp.com/developers/docs/resources/channel#embed-limits

    public class Spam : ModuleBase<SocketCommandContext> {

        [Command("rave")]
        [RequireContext(ContextType.Guild)]
        [Cooldown(1800)]
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
        [RequireContext(ContextType.Guild)]
        [OwnerAdminWhitelist]
        public Task RaveStopAsync() {
            RaveTracker.Stop(Context.Guild.Id, Context.Channel.Id);

            ulong reply = ReplyAsync("Stopping all raves in this channel... you party pooper.").Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            return Task.CompletedTask;
        }

        [Command("raveundo")]
        [Alias("undorave", "undoraves")]
        [RequireContext(ContextType.Guild)]
        [OwnerAdminWhitelist]
        public Task RaveUndoAsync(int messageCount = 300) {
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

        [Command("gimme")]
        [RequireContext(ContextType.Guild)]
        [Cooldown(20)]
        public Task GimmeAsync() {
            ulong reply = ReplyAsync("<:wubbydrugs:361993520040640516>").Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            return Task.CompletedTask;
        }

        [Command("rnk")]
        [RequireContext(ContextType.Guild)]
        [Cooldown(3600, true)]
        public Task RnkAsync() {
            ulong reply = Context.Message.Channel.SendFileAsync(Path.Combine(Constants.ContentDirectory, "rnk.png")).Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            return Task.CompletedTask;
        }

        [Command("moneyshot")]
        [RequireContext(ContextType.Guild)]
        [Cooldown(20)]
        public Task MoneyShotAsync() {
            ulong reply = Context.Message.Channel.SendFileAsync(Path.Combine(Constants.ContentDirectory, "moneyshot.png")).Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            return Task.CompletedTask;
        }

        [Command("yum")]
        [RequireContext(ContextType.Guild)]
        [Cooldown(20)]
        public Task YumAsync() {
            ulong reply = Context.Message.Channel.SendFileAsync(Path.Combine(Constants.ContentDirectory, "yum.png")).Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            return Task.CompletedTask;
        }

        [Command("fuckedup")]
        [Alias("ripdeposit")]
        [RequireContext(ContextType.Guild)]
        [Cooldown(20)]
        public Task FuckedUpAsync() {
            ulong reply = Context.Message.Channel.SendFileAsync(Path.Combine(Constants.ContentDirectory, "fucked_up.png")).Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            return Task.CompletedTask;
        }

        [Command("what")]
        [RequireContext(ContextType.Guild)]
        [Cooldown(20)]
        public Task WhatAsync() {
            ulong reply = Context.Message.Channel.SendFileAsync(Path.Combine(Constants.ContentDirectory, "what.png")).Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            return Task.CompletedTask;
        }

    }

}
