using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Extensions;
using HeadNonSub.Statistics;

namespace HeadNonSub.Clients.Discord.Commands {

    [Group("stats", "Command statistics")]
    [BlacklistEnforced]
    [CommandContextType(InteractionContextType.Guild)]
    public class Stats : BetterModuleBase {

        [SlashCommand("true", "How many times the true picture was posted")]
        public async Task TrueCount() {
            long count = StatisticsManager.GetTrueCount(Context.Guild.Id);
            await BetterReplyAsync($"There are {count:N0} truths here.");
        }

        [SlashCommand("says", "The most used /says templates")]
        public async Task SaysCount() {
            List<KeyValuePair<string, long>> saysCommands = StatisticsManager.GetSaysCount(Context.Guild.Id);

            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string, long> command in saysCommands) {
                builder.AppendLine($"{Constants.ZeroWidthSpace}{command.Value,-5:N0}: {command.Key}");
            }

            List<string> chunks = builder.ToString().SplitIntoChunksPreserveNewLines(1930);

            if (chunks.Count == 0) {
                await BetterReplyAsync("There is no `/says` data yet.");
                return;
            }

            foreach (string chunk in chunks) {
                await BetterReplyAsync($"● Top `/says` templates ```{chunk}```");
            }
        }

    }

}
