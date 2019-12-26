using System.Text;
using Discord;
using TagType = Discord.TagType;

namespace HeadNonSub.Extensions {

    public static class DiscordExt {

        /// <summary>
        /// Resolve tags in a message to their display names.
        /// </summary>
        /// <param name="input">Discord message.</param>
        /// <param name="encaseWith">Encase tags with this string.</param>
        public static string ResolveTags(this IMessage input, string encaseWith = "`") {
            StringBuilder message = new StringBuilder(input.Content);

            foreach (ITag tag in input.Tags) {
                if (tag.Type == TagType.ChannelMention) {
                    IChannel channel = tag.Value as IChannel;
                    message.Replace($"<#{tag.Key}>", $"{encaseWith}#{channel.Name}{encaseWith}");

                } else if (tag.Type == TagType.Emoji) {
                    IEmote emote = tag.Value as IEmote;
                    message.Replace($"<:{emote.Name}::{tag.Key}:>", $"{encaseWith}:{emote.Name}:{encaseWith}");

                } else if (tag.Type == TagType.EveryoneMention) {
                    message.Replace("@everyone", $"{encaseWith}@everyone{encaseWith}");

                } else if (tag.Type == TagType.HereMention) {
                    message.Replace("@here", $"{encaseWith}@everyone{encaseWith}");

                } else if (tag.Type == TagType.RoleMention) {
                    IRole role = tag.Value as IRole;
                    message.Replace($"<@&{tag.Key}>", $"{encaseWith}@{role.Name}{encaseWith}");

                } else if (tag.Type == TagType.UserMention) {
                    IUser user = tag.Value as IUser;
                    message.Replace($"<@!{tag.Key}>", $"{encaseWith}@{user.ToString()}{encaseWith}");
                    message.Replace($"<@{tag.Key}>", $"{encaseWith}@{user.ToString()}{encaseWith}");

                }
            }

            return message.ToString();
        }

    }

}
