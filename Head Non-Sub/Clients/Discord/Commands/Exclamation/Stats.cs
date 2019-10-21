using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Extensions;
using HeadNonSub.Statistics;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [BlacklistEnforced]
    [RequireContext(ContextType.Guild)]
    public class Stats : BetterModuleBase {

        [Command("truecount")]
        public async Task TrueCount() {
            await Context.Channel.TriggerTypingAsync();

            long count = StatisticsManager.GetTrueCount(Context.Guild.Id);

            await BetterReplyAsync($"There are {count.ToString("N0")} truths here.");
        }

        [Command("sayscount")]
        public async Task SaysCount() {
            await Context.Channel.TriggerTypingAsync();

            List<KeyValuePair<string, long>> saysCommands = StatisticsManager.GetSaysCount(Context.Guild.Id);

            StringBuilder builder = new StringBuilder();

            foreach (KeyValuePair<string, long> command in saysCommands) {
                builder.AppendLine($"{Constants.ZeroWidthSpace}{command.Value.ToString("N0").PadLeft(5)}: {command.Key}");
            }

            List<string> chunks = builder.ToString().SplitIntoChunksPreserveNewLines(1930);

            if (chunks.Count == 0) {
                await BetterReplyAsync("There is no top `!says` command data.");
                return;
            }

            foreach (string chunk in chunks) {
                await BetterReplyAsync($"● Top `!says` commands ```{chunk}```");
            }
        }

    }

}
