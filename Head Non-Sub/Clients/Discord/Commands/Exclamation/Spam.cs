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

        [Command("gongboy")]
        [Cooldown(20)]
        public Task GongBoy() => BetterSendFileAsync(Cache.GetStream("gongboy.png"), "gongboy.png", $"● {BetterUserFormat()}");

        [Command("true"), Alias("thatstrue")]
        [Cooldown(20)]
        public Task ThatsTrue() => BetterSendFileAsync(Cache.GetStream("true.png"), "true.png", $"● {BetterUserFormat()}");

        [Command("potato")]
        [Cooldown(20)]
        public Task Potato() => BetterSendFileAsync(Cache.GetStream("potato.png"), "potato.png", $"● {BetterUserFormat()}");

        [Command("nonsub")]
        [Cooldown(20)]
        public Task NonSub() => BetterReplyAsync($"Sub <{WubbysFunHouse.TwitchSubscribeUrl}> or link your Twitch <{WubbysFunHouse.LinkTwitchToDiscordUrl}>.");

        [Command("radar")]
        [Cooldown(20)]
        public Task Radar() => BetterSendFileAsync(Cache.GetStream("radar.png"), "radar.png", $"● {BetterUserFormat()}, created by SimplyMoose");

        [Command("lucky")]
        [Cooldown(20)]
        public Task Lucky() => BetterSendFileAsync(Cache.GetStream("lucky.png"), "lucky.png", $"● {BetterUserFormat()}");

        [Command("keyboard")]
        [Cooldown(20)]
        public Task WubbyKeyboard() => BetterSendFileAsync(Cache.GetStream("wubbykeyboard.png"), "wubbykeyboard.png", $"● {BetterUserFormat()}");

        [Command("wubbycheeto")]
        [Cooldown(20)]
        public Task WubbyCheeto() => BetterSendFileAsync(Cache.GetStream("wubbycheeto.png"), "wubbycheeto.png", $"● {BetterUserFormat()}, created by icy_ann");

        [Command("dickdowndennis")]
        [Cooldown(20)]
        public Task DickdownDennis() => BetterSendFileAsync(Cache.GetStream("dickdowndennis.png"), "dickdowndennis.png", $"● {BetterUserFormat()}");

        [Command("relax")]
        [Cooldown(20)]
        public Task Relax() => BetterSendFileAsync(Cache.GetStream("relax.png"), "relax.png", $"● {BetterUserFormat()}");

        [Command("weebshit")]
        [Cooldown(20)]
        public Task WeebShit() => BetterSendFileAsync(Cache.GetStream("weebshit.png"), "weebshit.png", $"● {BetterUserFormat()}");

        [Command("juan")]
        [Cooldown(20)]
        public Task Juan() => BetterSendFileAsync(Cache.GetStream("juan.png"), "juan.png", $"● {BetterUserFormat()}");

        [Command("weebmas")]
        [Cooldown(20)]
        public Task Weebmas() {
            Context.Channel.TriggerTypingAsync();
            return BetterSendFileAsync(Cache.GetStream("wubbypadoru_xmas.gif"), "wubbypadoru_xmas.gif", $"● {BetterUserFormat()}, created by Xaffiri");
        }

        [Command("dab")]
        [Cooldown(20)]
        public Task Dab() {
            Context.Channel.TriggerTypingAsync();
            return BetterSendFileAsync(Cache.GetStream("dab.gif"), "dab.gif", $"● {BetterUserFormat()}");
        }

        [Command("star")]
        [Cooldown(20)]
        public Task Star() {
            Context.Channel.TriggerTypingAsync();
            return BetterSendFileAsync(Cache.GetStream("star.gif"), "star.gif", $"● {BetterUserFormat()}");
        }

        [Command("gottem"), Alias("gotem")]
        [Cooldown(20)]
        public Task Gottem() {
            Context.Channel.TriggerTypingAsync();
            return BetterSendFileAsync(Cache.GetStream("gottem.gif"), "gottem.gif", $"● {BetterUserFormat()}");
        }

        [Command("youareallretarded")]
        [XathzOnly]
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
        [XathzOnly]
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

        //[Command("clap")]
        //[Cooldown(180, true)]
        //[SubscriberOnly]
        //public async Task Clap([Remainder] string input = "") {
        //    if (string.IsNullOrWhiteSpace(input)) {
        //        input = $"{BetterUserFormat(Context.User, true)} did not say anything and is dumb.";
        //    }

        //    IUserMessage message = await BetterReplyAsync(input, input);
        //    await Task.Delay(500);

        //    List<int> spaceIndexes = new List<int>();
        //    for (int i = input.IndexOf(' '); i > -1; i = input.IndexOf(' ', i + 1)) {
        //        spaceIndexes.Add(i);
        //    }

        //    foreach (int spaceIndex in spaceIndexes) {
        //        string tempInput = input.Remove(spaceIndex, 1).Insert(spaceIndex, $":clap::skin-tone-{new Random().Next(1, 5)}:");

        //        await message.ModifyAsync(x => x.Content = tempInput).ConfigureAwait(false);
        //        await Task.Delay(500);
        //    }

        //    await message.ModifyAsync(x => x.Content = $":clap::skin-tone-{new Random().Next(1, 5)}:");
        //    await Task.Delay(500);

        //    await message.DeleteAsync();
        //}

        [Command("night")]
        [Cooldown(20)]
        public Task Night() => BetterReplyAsync(@".　　　　　　　　　　 ✦ 　　　　   　 　　　˚　　　　　　　　　　　　　　*　　　　　　   　　　　　　　　　　　　　　　.　　　　　　　　　　　　　　. 　　 　　　　　　　 ✦ 　　　　　　　　　　 　 ‍ ‍ ‍ ‍ 　　　　 　　　　　　　　　　　　,　　   　

.　　　　　　　　　　　　　.　　　ﾟ　  　　　.　　　　　　　　　　　　　.

　　　　　　,　　　　　　　.　　　　　　    　　　　 　　　　　　　　　　　　　　　　　　 ☀️ 　　　　　　　　　　　　　　　　　　    　      　　　　　        　　　　　　　　　　　　　. 　　　　　　　　　　.　　　　　　　　　　　　　. 　　　　　　　　　　　　　　　　       　   　　　　 　　　　　　　　　　　　　　　　       　   　　　　　　　　　　　　　　　　       　    ✦ 　   　　　,　　　　　　　　　　　    🚀 　　　　 　　,　　　 ‍ ‍ ‍ ‍ 　 　　　　　　　　　　　　.　　　　　 　　 　　　.　　　　　　　　　　　　　 　           　　　　　　　　　　　　　　　　　　　˚　　　 　   　　　　,　　　　　　　　　　　       　    　　　　　　　　　　　　　　　　.　　　  　　    　　　　　 　　　　　.　　　　　　　　　　　　　.　　　　　　　　　　　　　　　* 　　   　　　　　 ✦ 　　　　　　　         　        　　　　 　　 　　　　　　　 　　　　　.　　　　　　　　　　　　　　　　　　.　　　　　    　　. 　 　　　　　.　　　　 🌑 　　　　　   　　　　　.　　　　　　　　　　　.　　　　　　　　　　   　

　˚　　　　　　　　　　　　　　　　　　　　　ﾟ　　　　　.　　　　　　　　　　　　　　　. 　　 　 🌎 ‍ ‍ ‍ ‍ ‍ ‍ ‍ ‍ ‍ ‍ ,　 　　　　　　　　　　　　　　* .　　　　　 　　　　　　　　　　　　　　.　　　　　　　　　　 ✦ 　　　　   　 　　　˚　　　　　　　　　　　　　　*　　　　　　 ");

    }

}
