using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Entities.Discord;
using HeadNonSub.Extensions;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [BlacklistEnforced]
    [RequireContext(ContextType.Guild)]
    public class Poll : BetterModuleBase {

        [Command("trashpoll")]
        public async Task TrashPoll([Remainder]string input) {
            IGuildUser user = Context.User as IGuildUser;

            List<EmoteOrEmoji> items = Context.Message.Content.ParseDiscordMessageEmotes();
            string content = Context.Message.Content;
            content = content.Remove(0, "!trashpoll".Length);

            foreach (EmoteOrEmoji item in items) {
                content = content.Replace(item.ToString(), "");
            }

            EmbedBuilder builder = new EmbedBuilder() {
                Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                Description = content
            };

            builder.Author = new EmbedAuthorBuilder() {
                IconUrl = user.GetAvatarUrl(),
                Name = $"{(!string.IsNullOrWhiteSpace(user.Nickname) ? user.Nickname : user.Username)} asks"
            };

            builder.Footer = new EmbedFooterBuilder() {
                Text = "Answer using the reactions below"
            };

            await Context.Message.DeleteAsync();

            IUserMessage message = await BetterReplyAsync(builder.Build(), parameters: input);

            // Might throw if the bot does not have access to the emote
            foreach (EmoteOrEmoji item in items) {
                try {
                    if (item.IsEmoji) {
                        await message.AddReactionAsync(new Emoji(item.ToString()));
                    } else {
                        if (Emote.TryParse(item.ToString(), out Emote emote)) {
                            await message.AddReactionAsync(emote);
                        }
                    }
                } catch { }
            }
        }

    }

}
