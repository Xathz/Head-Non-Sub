using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using HeadNonSub.Clients.Discord.Attributes;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    // https://discordapp.com/developers/docs/resources/channel#embed-limits

    public class Spam : ModuleBase<SocketCommandContext> {

        [Command("rave", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [Cooldown(300)]
        public Task GimmeAsync([Remainder]string input) {
            string[] messages = input.Split(' ');

            // Wubby's Fun House
            if (Context.Guild.Id == 328300333010911242) {
                // 'bot-commands' or 'actual-fucking-spam'
                if (Context.Channel.Id != 462517221789532170 || Context.Channel.Id != 537727672747294738) {
                    return ReplyAsync($"`!rave` is only usable in <#462517221789532170> or <#537727672747294738>.");
                }
            }

            // Cam’s pocket
            if (Context.Guild.Id == 528475747334225925) {
                // 'shitposting-cause-xathz'
                if (Context.Channel.Id != 546863784157904896) { return ReplyAsync($"`!rave` is only usable in <#546863784157904896>."); }
            }

            foreach (string message in messages) {
                ReplyAsync($":crab: {message} :crab:").Wait();

                Task.Delay(1250).Wait();
            }

            return Task.CompletedTask;
        }

        [Command("gimme")]
        [RequireContext(ContextType.Guild)]
        [Cooldown(10)]
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
        [Cooldown(10)]
        public Task MoneyShotAsync() {
            ulong reply = Context.Message.Channel.SendFileAsync(Path.Combine(Constants.ContentDirectory, "moneyshot.png")).Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            return Task.CompletedTask;
        }

        [Command("yum")]
        [RequireContext(ContextType.Guild)]
        [Cooldown(10)]
        public Task YumAsync() {
            ulong reply = Context.Message.Channel.SendFileAsync(Path.Combine(Constants.ContentDirectory, "yum.png")).Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            return Task.CompletedTask;
        }

        [Command("fuckedup")]
        [Alias("ripdeposit")]
        [RequireContext(ContextType.Guild)]
        [Cooldown(10)]
        public Task FuckedUpAsync() {
            ulong reply = Context.Message.Channel.SendFileAsync(Path.Combine(Constants.ContentDirectory, "fucked_up.png")).Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            return Task.CompletedTask;
        }

        [Command("what")]
        [RequireContext(ContextType.Guild)]
        [Cooldown(10)]
        public Task WhatAsync() {
            ulong reply = Context.Message.Channel.SendFileAsync(Path.Combine(Constants.ContentDirectory, "what.png")).Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            return Task.CompletedTask;
        }

    }

}
