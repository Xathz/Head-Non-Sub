using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Statistics;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [BlacklistEnforced]
    [RequireContext(ContextType.Guild)]
    public class Spam : ModuleBase<SocketCommandContext> {

        [Command("gimme")]
        [Cooldown(20)]
        public Task GimmeAsync() {
            ulong reply = ReplyAsync("<:wubbydrugs:361993520040640516>").Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
            return Task.CompletedTask;
        }

        [Command("rnk")]
        [Cooldown(3600, true)]
        public Task RnkAsync() {
            ulong reply = Context.Message.Channel.SendFileAsync(Cache.GetStream("rnk.png"), "rnk.png").Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
            return Task.CompletedTask;
        }

        [Command("moneyshot")]
        [Cooldown(20)]
        public Task MoneyShotAsync() {
            ulong reply = Context.Message.Channel.SendFileAsync(Cache.GetStream("moneyshot.png"), "moneyshot.png").Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
            return Task.CompletedTask;
        }

        [Command("yum")]
        [Cooldown(20)]
        public Task YumAsync() {
            ulong reply = Context.Message.Channel.SendFileAsync(Cache.GetStream("yum.png"), "yum.png").Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
            return Task.CompletedTask;
        }

        [Command("fuckedup")]
        [Alias("ripdeposit")]
        [Cooldown(20)]
        public Task FuckedUpAsync() {
            ulong reply = Context.Message.Channel.SendFileAsync(Cache.GetStream("fucked_up.png"), "fucked_up.png").Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
            return Task.CompletedTask;
        }

        [Command("what")]
        [Cooldown(20)]
        public Task WhatAsync() {
            ulong reply = Context.Message.Channel.SendFileAsync(Cache.GetStream("what.png"), "what.png").Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
            return Task.CompletedTask;
        }

        [Command("bigoof")]
        [Cooldown(20)]
        public Task BigOofAsync() {
            ulong reply = Context.Message.Channel.SendFileAsync(Cache.GetStream("big_oof.gif"), "big_oof.gif").Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
            return Task.CompletedTask;
        }

        [Command("1024nude")]
        [Cooldown(20)]
        public Task TenTwentyFourNudeAsync() {
            ulong reply = Context.Message.Channel.SendFileAsync(Cache.GetStream("1024_nude.png"), "1024_nude.png").Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
            return Task.CompletedTask;
        }

        [Command("star")]
        [Cooldown(20)]
        public Task StarAsync() {
            ulong reply = Context.Message.Channel.SendFileAsync(Cache.GetStream("star.gif"), "star.gif").Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
            return Task.CompletedTask;
        }

        [Command("gongboy")]
        [Cooldown(20)]
        public Task GongBoyAsync() {
            ulong reply = Context.Message.Channel.SendFileAsync(Cache.GetStream("gongboy.png"), "gongboy.png").Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
            return Task.CompletedTask;
        }

        [Command("true")]
        [Cooldown(20)]
        [SubscriberOnly]
        public Task TrueAsync() {
            ulong reply = Context.Message.Channel.SendFileAsync(Cache.GetStream("true.png"), "true.png").Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
            return Task.CompletedTask;
        }

        [Command("potato")]
        [Cooldown(20)]
        public Task PotatoAsync() {
            ulong reply = Context.Message.Channel.SendFileAsync(Cache.GetStream("potato.png"), "potato.png").Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
            return Task.CompletedTask;
        }

        [Command("youareallretarded")]
        [Cooldown(120)]
        [AllowedGuilds(328300333010911242)]
        public Task AllRetardedAsync() {
            if (Context.Channel is SocketTextChannel channel) {
                IAsyncEnumerable<IMessage> messages = channel.GetMessagesAsync(Context.Message, Direction.Before, 10).Flatten().OrderByDescending(x => x.CreatedAt);
                IEmote emote = Context.Guild.Emotes.FirstOrDefault(x => x.Id == 329534564160765954);
                if (emote is IEmote) {
                    messages.ForEach(x => {
                        if (x is IUserMessage message) {
                            message.AddReactionAsync(emote).Wait();
                        }
                    });
                }
            }

            StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
            return Task.CompletedTask;
        }

        [Command("allthis")]
        [Cooldown(120)]
        [AllowedGuilds(328300333010911242)]
        public Task AllThisAsync() {
            if (Context.Channel is SocketTextChannel channel) {
                IAsyncEnumerable<IMessage> messages = channel.GetMessagesAsync(Context.Message, Direction.Before, 10).Flatten().OrderByDescending(x => x.CreatedAt);
                IEmote emote = Context.Guild.Emotes.FirstOrDefault(x => x.Id == 451081265467359253);
                if (emote is IEmote) {
                    messages.ForEach(x => {
                        if (x is IUserMessage message) {
                            message.AddReactionAsync(emote).Wait();
                        }
                    });
                }
            }

            StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
            return Task.CompletedTask;
        }

        [Command("night")]
        [Cooldown(20)]
        public Task NightAsync() {
            ulong reply = ReplyAsync(@".　　　　　　　　　　 ✦ 　　　　   　 　　　˚　　　　　　　　　　　　　　*　　　　　　   　　　　　　　　　　　　　　　.　　　　　　　　　　　　　　. 　　 　　　　　　　 ✦ 　　　　　　　　　　 　 ‍ ‍ ‍ ‍ 　　　　 　　　　　　　　　　　　,　　   　

.　　　　　　　　　　　　　.　　　ﾟ　  　　　.　　　　　　　　　　　　　.

　　　　　　,　　　　　　　.　　　　　　    　　　　 　　　　　　　　　　　　　　　　　　 ☀️ 　　　　　　　　　　　　　　　　　　    　      　　　　　        　　　　　　　　　　　　　. 　　　　　　　　　　.　　　　　　　　　　　　　. 　　　　　　　　　　　　　　　　       　   　　　　 　　　　　　　　　　　　　　　　       　   　　　　　　　　　　　　　　　　       　    ✦ 　   　　　,　　　　　　　　　　　    🚀 　　　　 　　,　　　 ‍ ‍ ‍ ‍ 　 　　　　　　　　　　　　.　　　　　 　　 　　　.　　　　　　　　　　　　　 　           　　　　　　　　　　　　　　　　　　　˚　　　 　   　　　　,　　　　　　　　　　　       　    　　　　　　　　　　　　　　　　.　　　  　　    　　　　　 　　　　　.　　　　　　　　　　　　　.　　　　　　　　　　　　　　　* 　　   　　　　　 ✦ 　　　　　　　         　        　　　　 　　 　　　　　　　 　　　　　.　　　　　　　　　　　　　　　　　　.　　　　　    　　. 　 　　　　　.　　　　 🌑 　　　　　   　　　　　.　　　　　　　　　　　.　　　　　　　　　　   　

　˚　　　　　　　　　　　　　　　　　　　　　ﾟ　　　　　.　　　　　　　　　　　　　　　. 　　 　 🌎 ‍ ‍ ‍ ‍ ‍ ‍ ‍ ‍ ‍ ‍ ,　 　　　　　　　　　　　　　　* .　　　　　 　　　　　　　　　　　　　　.　　　　　　　　　　 ✦ 　　　　   　 　　　˚　　　　　　　　　　　　　　*　　　　　　 ").Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
            return Task.CompletedTask;
        }

    }

}
