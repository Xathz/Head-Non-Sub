using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using HeadNonSub.Statistics;
using StrawPollNET.Models;
using static StrawPollNET.API;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [RequireContext(ContextType.Guild)]
    public class Strawpoll : ModuleBase<SocketCommandContext> {

        [Command("strawpoll")]
        public Task StrawpollAsync([Remainder]string input) {
            string[] messages = input.Split('|');

            CreatedPoll newPoll = null;
            ulong? reply = null;

            newPoll = Create.CreatePoll(messages[0], messages.Skip(1).ToList(), false, StrawPollNET.Enums.DupCheck.Normal, false);

            if (newPoll == null || string.IsNullOrEmpty(newPoll.PollUrl)) {
                reply = ReplyAsync("Failed to create the strawpoll. Example: `!strawpoll Poll Title | Option 1 | Option 2 | Option 3`").Result.Id;
            } else {
                reply = ReplyAsync($"{Context.User.Mention} <{newPoll.PollUrl}>").Result.Id;
            }

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply.Value);
            StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
            return Task.CompletedTask;
        }

        [Command("strawpollresults")]
        [Alias("strawpollr")]
        public Task StrawpollResultsAsync([Remainder]string input) {
            Context.Message.DeleteAsync();

            FetchedPoll fetchedPoll = null;
            ulong? reply = null;

            try {
                string pollId = new Uri(input).Segments[1];
                if (pollId.Contains("/")) { pollId = pollId.Replace("/", ""); }

                bool pollIdAttempt = int.TryParse(pollId, out int pollIdResult);
                fetchedPoll = Get.GetPoll(pollIdResult);

                if (fetchedPoll is FetchedPoll & pollIdAttempt) {

                    EmbedBuilder builder = new EmbedBuilder() {
                        Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                        Title = $"{fetchedPoll.Title} Results"
                    };

                    StringBuilder resultBuilder = new StringBuilder();

                    foreach (KeyValuePair<string, int> result in fetchedPoll.Results.OrderByDescending(x => x.Value)) {
                        resultBuilder.Append($"**{result.Key.Trim()}:** {string.Format("{0:n0}", result.Value)}{Environment.NewLine}");
                    }

                    builder.Description = resultBuilder.ToString().TrimEnd();

                    reply = ReplyAsync(embed: builder.Build()).Result.Id;

                } else {
                    reply = ReplyAsync($"Failed to retrieve the strawpoll.").Result.Id;
                }
            } catch {
                reply = ReplyAsync($"Failed to retrieve the strawpoll.").Result.Id;
            }

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply.Value);
            StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
            return Task.CompletedTask;
        }

    }

}
