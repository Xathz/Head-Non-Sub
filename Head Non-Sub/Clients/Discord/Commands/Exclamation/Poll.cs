using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [RequireContext(ContextType.Guild)]
    public class Poll : ModuleBase<SocketCommandContext> {

        [Command("trashpoll")]
        public Task TrashPollAsync([Remainder]string input) {
            IGuildUser user = Context.User as IGuildUser;

            StringBuilder content = new StringBuilder();
            HashSet<IEmote> reactions = new HashSet<IEmote>();

            string[] split = input.Split(new char[] { '<', '>' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string item in split) {
                string workingItem = item.Trim();

                // Emotes
                if (workingItem.Count(x => x == ':') == 2) {
                    Emote.TryParse($"<{workingItem}>", out Emote emote);

                    if (emote is Emote) {
                        reactions.Add(emote);
                    }

                    continue;
                }

                // Emoji
                List<char> emoji = workingItem.Where(x => char.GetUnicodeCategory(x) == UnicodeCategory.OtherSymbol).ToList();
                emoji.ForEach(x => reactions.Add(new Emoji(x.ToString())));
                emoji.ForEach(x => workingItem = workingItem.Trim(x));

                // Normal characters
                bool isNormal = false;
                isNormal = workingItem.Any(x => char.IsLetterOrDigit(x) || char.IsPunctuation(x) || char.IsSymbol(x) || char.IsWhiteSpace(x) || char.IsControl(x));
                if (isNormal) {
                    if (!string.IsNullOrWhiteSpace(workingItem)) {
                        content.Append(workingItem);
                    }
                }
            }

            // Embed
            EmbedBuilder builder = new EmbedBuilder() {
                Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                Description = content.ToString()
            };

            builder.Author = new EmbedAuthorBuilder() {
                IconUrl = user.GetAvatarUrl(),
                Name = $"{(!string.IsNullOrWhiteSpace(user.Nickname) ? user.Nickname : user.Username)} asks"
            };

            builder.Footer = new EmbedFooterBuilder() {
                Text = "Answer using the reactions below"
            };

            Context.Message.DeleteAsync();

            IUserMessage message = ReplyAsync(embed: builder.Build()).Result;

            // Might throw if the bot does not have access to the emote
            foreach (IEmote reaction in reactions) {
                try {
                    message.AddReactionAsync(reaction);
                } catch { }
            }

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, message.Id);
            return Task.CompletedTask;
        }

    }

}
