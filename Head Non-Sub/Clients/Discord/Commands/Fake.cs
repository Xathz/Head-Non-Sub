using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace HeadNonSub.Clients.Discord.Commands {

    // https://discordapp.com/developers/docs/resources/channel#embed-limits

    public class Fake : ModuleBase<SocketCommandContext> {

        [Command("!execute", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        public Task ExecuteAsync(SocketUser user = null, [Remainder]string message = "") {

            ReplyAsync($"{user.Mention} you have 10 seconds to say last words before you are executed.").Wait();
            Task.Delay(10000).Wait();

            ReplyAsync($"10 seconds over. Now executing {user.ToString()} for reason: `{message}`...").Wait();

            Task.Delay(1000).Wait();
            ReplyAsync("3");
            Task.Delay(1000).Wait();
            ReplyAsync("2");
            Task.Delay(1000).Wait();
            ReplyAsync("1").Wait();

            Task.Delay(200).Wait();
            ReplyAsync($"The target, `{user.ToString()}` has been executed and their messages from the past 0 days of messages have been purged.").Wait();

            return Task.CompletedTask;
        }

    }

}
