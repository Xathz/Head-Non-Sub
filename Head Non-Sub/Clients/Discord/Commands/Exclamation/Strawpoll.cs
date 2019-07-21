using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using HeadNonSub.Clients.Discord.Attributes;
using StrawPollNET.Models;
using static StrawPollNET.API;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [BlacklistEnforced]
    [RequireContext(ContextType.Guild)]
    public class Strawpoll : BetterModuleBase {

        [Command("strawpoll")]
        public Task StrawpollCreate([Remainder]string input) {
            string[] messages = input.Split('|');

            CreatedPoll newPoll = null;
            try {
                newPoll = Create.CreatePoll(messages[0], messages.Skip(1).ToList(), false, StrawPollNET.Enums.DupCheck.Normal, false);
            } catch { }

            if (newPoll == null || string.IsNullOrEmpty(newPoll.PollUrl)) {
                return BetterReplyAsync("Failed to create the strawpoll. Example: `!strawpoll Poll Title | Option 1 | Option 2 | Option 3`");
            } else {
               return BetterReplyAsync($"{Context.User.Mention} <{newPoll.PollUrl}>", parameters: input);
            }
        }

        [Command("strawpollresults"), Alias("strawpollr")]
        public Task StrawpollResults([Remainder]string input) {
            Context.Message.DeleteAsync();

            FetchedPoll fetchedPoll = null;

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

                    return BetterReplyAsync(builder.Build(), parameters: input);

                } else {
                    return BetterReplyAsync($"Failed to retrieve the strawpoll.");
                }
            } catch {
                return BetterReplyAsync($"Failed to retrieve the strawpoll.");
            }
        }

    }

}
