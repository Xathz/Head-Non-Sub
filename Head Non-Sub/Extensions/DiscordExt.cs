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
                    // Null when the channel is not visible to the bot
                    string name = tag.Value is IChannel channel ? channel.Name : tag.Key.ToString();
                    message.Replace($"<#{tag.Key}>", $"{encaseWith}#{name}{encaseWith}");

                } else if (tag.Type == TagType.Emoji) {
                    IEmote emote = tag.Value as IEmote;
                    message.Replace($"<:{emote.Name}:{tag.Key}>", $"{encaseWith}:{emote.Name}:{encaseWith}");
                    message.Replace($"<a:{emote.Name}:{tag.Key}>", $"{encaseWith}:{emote.Name}:{encaseWith}");

                } else if (tag.Type == TagType.EveryoneMention) {
                    message.Replace("@everyone", $"{encaseWith}@everyone{encaseWith}");

                } else if (tag.Type == TagType.HereMention) {
                    message.Replace("@here", $"{encaseWith}@here{encaseWith}");

                } else if (tag.Type == TagType.RoleMention) {
                    // Null when the role was deleted
                    string name = tag.Value is IRole role ? role.Name : tag.Key.ToString();
                    message.Replace($"<@&{tag.Key}>", $"{encaseWith}@{name}{encaseWith}");

                } else if (tag.Type == TagType.UserMention) {
                    IUser user = tag.Value as IUser;
                    message.Replace($"<@!{tag.Key}>", $"{encaseWith}@{user}{encaseWith}");
                    message.Replace($"<@{tag.Key}>", $"{encaseWith}@{user}{encaseWith}");

                }
            }

            return message.ToString();
        }

    }

}
