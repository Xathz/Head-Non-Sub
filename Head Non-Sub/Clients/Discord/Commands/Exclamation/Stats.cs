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
    public class Stats : ModuleBase<SocketCommandContext> {

        [Command("truecount")]
        public Task TrueCountAsync() {
            ulong count = StatisticsManager.Statistics.Commands(Context.Guild.Id).TimesExecuted("TrueAsync");

            ulong reply = ReplyAsync($"There are {count.ToString("N0")} truths here.").Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
            return Task.CompletedTask;
        }

        [Command("toptts")]
        public Task TopTTSAsync() {
            List<KeyValuePair<string, ulong>> topWords = StatisticsManager.Statistics.Commands(Context.Guild.Id).TTSTopWords();

            EmbedBuilder builder = new EmbedBuilder() {
                Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                Title = $"Top words used in text to speech commands"
            };

            builder.AddField("Word . . . Times Used", $"```{string.Join(Environment.NewLine, topWords.Select(x => $"{x.Key.PadRight(16, '.')} {x.Value}"))}```");

            ulong reply = ReplyAsync(embed: builder.Build()).Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
            return Task.CompletedTask;
        }

    }

}
