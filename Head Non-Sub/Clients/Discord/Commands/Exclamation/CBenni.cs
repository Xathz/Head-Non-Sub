using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Discord.Commands;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Extensions;
using HeadNonSub.Settings;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [BlacklistEnforced, TwitchStaffOnly]
    [RequireContext(ContextType.Guild)]
    public class CBenni : BetterModuleBase {

        [Command("twitchuser")]
        public async Task TwitchUser(string user = "") {
            if (string.IsNullOrWhiteSpace(user)) {
                await BetterReplyAsync("You must provide a Twitch username.");
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            string log = GetCBenniUser(user.ToLower());

            if (string.IsNullOrWhiteSpace(log)) {
                await BetterReplyAsync($"{user} was not found.");
                return;
            } else {
                await BetterReplyAsync(log);
            }
        }

        private string GetCBenniUser(string user) {
            WebClient webClient = new WebClient();
            string reply = webClient.DownloadString($"https://cbenni.com/api/slack/?default_channel=paymoneywubby&text={user}&lvtoken={SettingsManager.Configuration.CBenniToken}");

            List<string> clean = reply.SplitByNewLines();

            return string.Join(Environment.NewLine, clean);
        }

    }

}
