using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [BlacklistEnforced]
    [RequireContext(ContextType.Guild)]
    public class Spam : BetterModuleBase {

        [Command("gimme")]
        [Cooldown(20)]
        public Task Gimme() => BetterReplyAsync("<:wubbydrugs:361993520040640516>");

        [Command("rnk")]
        [Cooldown(3600, true)]
        public Task Rnk() => BetterSendFileAsync(Cache.GetStream("rnk.png"), "rnk.png", $"● {BetterUserFormat()}");

        [Command("moneyshot")]
        [Cooldown(20)]
        public Task MoneyShot() => BetterSendFileAsync(Cache.GetStream("moneyshot.png"), "moneyshot.png", $"● {BetterUserFormat()}");

        [Command("yum")]
        [Cooldown(20)]
        public Task Yum() => BetterSendFileAsync(Cache.GetStream("yum.png"), "yum.png", $"● {BetterUserFormat()}");

        [Command("fuckedup"), Alias("ripdeposit")]
        [Cooldown(20)]
        public Task FuckedUp() => BetterSendFileAsync(Cache.GetStream("fucked_up.png"), "fucked_up.png", $"● {BetterUserFormat()}");

        [Command("what")]
        [Cooldown(20)]
        public Task What() => BetterSendFileAsync(Cache.GetStream("what.png"), "what.png", $"● {BetterUserFormat()}");

        [Command("bigoof")]
        [Cooldown(20)]
        public Task BigOof() => BetterSendFileAsync(Cache.GetStream("big_oof.gif"), "big_oof.gif", $"● {BetterUserFormat()}");

        [Command("1024nude")]
        [Cooldown(20)]
        public Task TenTwentyFourNude() => BetterSendFileAsync(Cache.GetStream("1024_nude.png"), "1024_nude.png", $"● {BetterUserFormat()}");

        [Command("star")]
        [Cooldown(20)]
        public Task Star() {
            Context.Channel.TriggerTypingAsync();
            return BetterSendFileAsync(Cache.GetStream("star.gif"), "star.gif", $"● {BetterUserFormat()}");
        }

        [Command("gongboy")]
        [Cooldown(20)]
        public Task GongBoy() => BetterSendFileAsync(Cache.GetStream("gongboy.png"), "gongboy.png", $"● {BetterUserFormat()}");

        [Command("true"), Alias("thatstrue")]
        [Cooldown(20)]
        [SubscriberOnly]
        public Task ThatsTrue() => BetterSendFileAsync(Cache.GetStream("true.png"), "true.png", $"● {BetterUserFormat()}");

        [Command("potato")]
        [Cooldown(20)]
        public Task Potato() => BetterSendFileAsync(Cache.GetStream("potato.png"), "potato.png", $"● {BetterUserFormat()}");

        [Command("nonsub")]
        [Cooldown(20)]
        [SubscriberOnly]
        public Task NonSub() => BetterSendFileAsync(Cache.GetStream("nonsub.png"), "nonsub.png", $"● {BetterUserFormat()}");

        [Command("lucky")]
        [Cooldown(20)]
        public Task Lucky() => BetterSendFileAsync(Cache.GetStream("lucky.png"), "lucky.png", $"● {BetterUserFormat()}");

        [Command("dickdowndennis")]
        [Cooldown(20)]
        public Task DickdownDennis() => BetterSendFileAsync(Cache.GetStream("dickdowndennis.png"), "dickdowndennis.png", $"● {BetterUserFormat()}");

        [Command("relax")]
        [Cooldown(20)]
        public Task Relax() => BetterSendFileAsync(Cache.GetStream("relax.png"), "relax.png", $"● {BetterUserFormat()}");

        [Command("dab")]
        [Cooldown(20)]
        public Task Dab() {
            Context.Channel.TriggerTypingAsync();
            return BetterSendFileAsync(Cache.GetStream("dab.gif"), "dab.gif", $"● {BetterUserFormat()}");
        }

        [Command("weebshit")]
        [Cooldown(20)]
        public Task WeebShit() => BetterSendFileAsync(Cache.GetStream("weebshit.png"), "weebshit.png", $"● {BetterUserFormat()}");

        [Command("gottem"), Alias("gotem")]
        [Cooldown(20)]
        public Task Gottem() {
            Context.Channel.TriggerTypingAsync();
            return BetterSendFileAsync(Cache.GetStream("gottem.gif"), "gottem.gif", $"● {BetterUserFormat()}");
        }

        [Command("youareallretarded")]
        [Cooldown(120)]
        public Task AllRetarded() {
            if (Context.Channel is SocketTextChannel channel) {
                IAsyncEnumerable<IMessage> messages = channel.GetMessagesAsync(Context.Message, Direction.Before, 10).Flatten().OrderByDescending(x => x.CreatedAt);
                IEmote emote = Context.Guild.Emotes.FirstOrDefault(x => x.Id == 329534564160765954);
                if (emote is IEmote) {
                    messages.ForEach(async x => {
                        if (x is IUserMessage message) {
                            await message.AddReactionAsync(emote);
                        }
                    });
                }
            }

            TrackStatistics();
            return Task.CompletedTask;
        }

        [Command("allthis")]
        [Cooldown(120)]
        public Task AllThis() {
            if (Context.Channel is SocketTextChannel channel) {
                IAsyncEnumerable<IMessage> messages = channel.GetMessagesAsync(Context.Message, Direction.Before, 10).Flatten().OrderByDescending(x => x.CreatedAt);
                IEmote emote = Context.Guild.Emotes.FirstOrDefault(x => x.Id == 451081265467359253);
                if (emote is IEmote) {
                    messages.ForEach(async x => {
                        if (x is IUserMessage message) {
                            await message.AddReactionAsync(emote);
                        }
                    });
                }
            }

            TrackStatistics();
            return Task.CompletedTask;
        }

        [Command("night")]
        [Cooldown(20)]
        public Task Night() => BetterReplyAsync(@".　　　　　　　　　　 ✦ 　　　　   　 　　　˚　　　　　　　　　　　　　　*　　　　　　   　　　　　　　　　　　　　　　.　　　　　　　　　　　　　　. 　　 　　　　　　　 ✦ 　　　　　　　　　　 　 ‍ ‍ ‍ ‍ 　　　　 　　　　　　　　　　　　,　　   　

.　　　　　　　　　　　　　.　　　ﾟ　  　　　.　　　　　　　　　　　　　.

　　　　　　,　　　　　　　.　　　　　　    　　　　 　　　　　　　　　　　　　　　　　　 ☀️ 　　　　　　　　　　　　　　　　　　    　      　　　　　        　　　　　　　　　　　　　. 　　　　　　　　　　.　　　　　　　　　　　　　. 　　　　　　　　　　　　　　　　       　   　　　　 　　　　　　　　　　　　　　　　       　   　　　　　　　　　　　　　　　　       　    ✦ 　   　　　,　　　　　　　　　　　    🚀 　　　　 　　,　　　 ‍ ‍ ‍ ‍ 　 　　　　　　　　　　　　.　　　　　 　　 　　　.　　　　　　　　　　　　　 　           　　　　　　　　　　　　　　　　　　　˚　　　 　   　　　　,　　　　　　　　　　　       　    　　　　　　　　　　　　　　　　.　　　  　　    　　　　　 　　　　　.　　　　　　　　　　　　　.　　　　　　　　　　　　　　　* 　　   　　　　　 ✦ 　　　　　　　         　        　　　　 　　 　　　　　　　 　　　　　.　　　　　　　　　　　　　　　　　　.　　　　　    　　. 　 　　　　　.　　　　 🌑 　　　　　   　　　　　.　　　　　　　　　　　.　　　　　　　　　　   　

　˚　　　　　　　　　　　　　　　　　　　　　ﾟ　　　　　.　　　　　　　　　　　　　　　. 　　 　 🌎 ‍ ‍ ‍ ‍ ‍ ‍ ‍ ‍ ‍ ‍ ,　 　　　　　　　　　　　　　　* .　　　　　 　　　　　　　　　　　　　　.　　　　　　　　　　 ✦ 　　　　   　 　　　˚　　　　　　　　　　　　　　*　　　　　　 ");

    }

}
