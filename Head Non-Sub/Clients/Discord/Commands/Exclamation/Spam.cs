using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using HeadNonSub.Clients.Discord.Attributes;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [RequireContext(ContextType.Guild)]
    public class Spam : ModuleBase<SocketCommandContext> {

        [Command("gimme")]
        [Cooldown(20)]
        public Task GimmeAsync() {
            ulong reply = ReplyAsync("<:wubbydrugs:361993520040640516>").Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            return Task.CompletedTask;
        }

        [Command("rnk")]
        [Cooldown(3600, true)]
        public Task RnkAsync() {
            ulong reply = Context.Message.Channel.SendFileAsync(Path.Combine(Constants.ContentDirectory, "rnk.png")).Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            return Task.CompletedTask;
        }

        [Command("moneyshot")]
        [Cooldown(20)]
        public Task MoneyShotAsync() {
            ulong reply = Context.Message.Channel.SendFileAsync(Path.Combine(Constants.ContentDirectory, "moneyshot.png")).Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            return Task.CompletedTask;
        }

        [Command("yum")]
        [Cooldown(20)]
        public Task YumAsync() {
            ulong reply = Context.Message.Channel.SendFileAsync(Path.Combine(Constants.ContentDirectory, "yum.png")).Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            return Task.CompletedTask;
        }

        [Command("fuckedup")]
        [Alias("ripdeposit")]
        [Cooldown(20)]
        public Task FuckedUpAsync() {
            ulong reply = Context.Message.Channel.SendFileAsync(Path.Combine(Constants.ContentDirectory, "fucked_up.png")).Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            return Task.CompletedTask;
        }

        [Command("what")]
        [Cooldown(20)]
        public Task WhatAsync() {
            ulong reply = Context.Message.Channel.SendFileAsync(Path.Combine(Constants.ContentDirectory, "what.png")).Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            return Task.CompletedTask;
        }

        [Command("bigoof")]
        [Cooldown(20)]
        public Task BigOofAsync() {
            ulong reply = Context.Message.Channel.SendFileAsync(Path.Combine(Constants.ContentDirectory, "big_oof.gif")).Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            return Task.CompletedTask;
        }

        [Command("1024nude")]
        [Cooldown(20)]
        public Task TenTwentyFourNudeAsync() {
            ulong reply = Context.Message.Channel.SendFileAsync(Path.Combine(Constants.ContentDirectory, "1024_nude.png")).Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            return Task.CompletedTask;
        }

    }

}
