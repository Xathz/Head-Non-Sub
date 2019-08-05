using System;
using System.Collections.Generic;
using System.Linq;
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
        public async Task TwitchUser(string user = "", int count = 10) {
            if (string.IsNullOrWhiteSpace(user)) {
                await BetterReplyAsync("You must provide a Twitch username.", $"{user} ({count})");
                return;
            }

            if (count < 10 || count > 100) {
                await BetterReplyAsync("Count must be between 10 and 100.", $"{user} ({count})");
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            string log = await GetCBenniUserAsync(user.ToLower(), count);
            log = log.RemoveEmptyLines();

            string messages = log.Extract("```", "```");
            List<string> messageChunks = messages.SplitIntoChunksPreserveNewLines(1980);

            IEnumerable<string> possibleUsers = Context.Guild.Users.Where(x => x.Username.Contains(user, StringComparison.OrdinalIgnoreCase)).Select(x => BetterUserFormat(x));
            string possibleUsersString = (possibleUsers.Count() > 0) ? $"{Environment.NewLine}Possible users here: {string.Join(", ", possibleUsers)}" : "";

            bool sentHeader = false;
            foreach (string chunk in messageChunks) {
                if (!sentHeader) {
                    await BetterReplyAsync($"{log.Replace($"```{messages}```", "").RemoveEmptyLines()}{possibleUsersString}", $"{user} ({count})");
                    sentHeader = true;
                }

                await BetterReplyAsync($"```{chunk}```", $"{user} ({count})");
            }
        }

        private async Task<string> GetCBenniUserAsync(string user, int count) {
            Task<string> download = Http.SendRequestAsync($"https://cbenni.com/api/slack/?default_channel=paymoneywubby&text={user} {count}&lvtoken={SettingsManager.Configuration.CBenniToken}");
            string data = await download;

            if (download.IsCompletedSuccessfully) {
                return data;
            } else {
                LoggingManager.Log.Error(download.Exception);
                return string.Empty;
            }
        }

    }

}
