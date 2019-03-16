using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Statistics;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [BlacklistEnforced]
    [RequireContext(ContextType.Guild)]
    public class Stats : BetterModuleBase {

        private readonly Dictionary<string, string> _CommandNames = new Dictionary<string, string>() {
            { "TTSays", "tt2468 Says" }, { "TenTwentyFourSays", "1024x768 Says" }, { "AmandaSays", "Amanda Says" },
            { "SataSays", "Satazero Says" }, { "JiberSays", "jiberjiber Says" }
        };

        [Command("truecount")]
        public Task TrueCount() {
            ulong count = StatisticsManager.Statistics.Commands(Context.Guild.Id).TimesExecuted("ThatsTrue");

            return BetterReplyAsync($"There are {count.ToString("N0")} truths here.");
        }

        [Command("toptts")]
        public Task TopTTS() {
            List<KeyValuePair<string, ulong>> topWords = StatisticsManager.Statistics.Commands(Context.Guild.Id).TTSTopWords();

            EmbedBuilder builder = new EmbedBuilder() {
                Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                Title = $"Top words used in text to speech commands"
            };

            builder.AddField("Word . . . Times Used", $"```{string.Join(Environment.NewLine, topWords.Select(x => $"{x.Key.PadRight(18, '.')} {x.Value}"))}```");

            return BetterReplyAsync(builder.Build());
        }

        [Command("sayscount")]
        public Task SaysCount() {
            List<KeyValuePair<string, ulong>> says = StatisticsManager.Statistics.Commands(Context.Guild.Id).SaysCount(_CommandNames);

            EmbedBuilder builder = new EmbedBuilder() {
                Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                Title = $"Top used 'says' command {string.Concat(Enumerable.Repeat(Constants.DoubleSpace, 20))}"
            };

            builder.AddField("Command . . . Times Used", $"```{string.Join(Environment.NewLine, says.Select(x => $"{x.Key.PadRight(20, '.')} {x.Value}"))}```");

            return BetterReplyAsync(builder.Build());
        }

    }

}
