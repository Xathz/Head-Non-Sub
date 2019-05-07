using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using HeadNonSub.Entities.Discord.MessageTag;

namespace HeadNonSub.Extensions {

    public static class StringExt {

        private static readonly Regex _UrlRegex = new Regex("https?://", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Split a string into chunks based on max length.
        /// </summary>
        public static List<string> SplitIntoChunks(this string input, int maxChunkSize) {
            string[] words = input.Split(' ');
            List<string> chunks = new List<string>();
            int index = 0;

            foreach (string word in words) {
                if (chunks.Count == index) {
                    chunks.Add("");
                }

                if ((chunks[index].Length + word.Length) <= maxChunkSize) {
                    chunks[index] += $" {word}";

                } else {
                    chunks.Add($" {word}");
                    index++;
                }
            }

            return chunks.Select(x => x.Trim()).ToList();
        }

        /// <summary>
        /// Split a string into chunks based on max length, preserving and not breaking words or before a new line. 
        /// </summary>
        public static List<string> SplitIntoChunksPreserveNewLines(this string input, int maxChunkSize) {
            string[] lines = input.Split(new string[] { Environment.NewLine, "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
            List<string> chunks = new List<string>();
            int index = 0;

            foreach (string line in lines) {
                string currentLine = $"{line}{Environment.NewLine}";

                if (currentLine.Length > maxChunkSize) {
                    throw new OverflowException("A single currentLine is longer than maxChunkSize.");
                }

                if (chunks.Count == index) {
                    chunks.Add("");
                }

                if ((chunks[index].Length + currentLine.Length) <= maxChunkSize) {
                    chunks[index] += currentLine;

                } else {
                    chunks.Add(currentLine);
                    index++;
                }
            }

            return chunks.Select(x => x.Trim()).ToList();
        }

        /// <summary>
        /// Extract a string between two strings.
        /// </summary>
        public static string Extract(this string input, string start, string end) {
            int from = input.IndexOf(start) + start.Length;
            int to = input.LastIndexOf(end);

            return input.Substring(from, to - from);
        }

        /// <summary>
        /// Remove all empty lines.
        /// </summary>
        public static string RemoveEmptyLines(this string input) {
            List<string> clean = input.SplitByNewLines();
            return string.Join(Environment.NewLine, clean);
        }

        /// <summary>
        /// Remove all new lines and line breaks from a string.
        /// </summary>
        public static string RemoveNewLines(this string input) => input.Replace(Environment.NewLine, " ").Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");

        /// <summary>
        /// Split a string by all possible new line and return characters.
        /// </summary>
        public static List<string> SplitByNewLines(this string input) => input.Split(new string[] { Environment.NewLine, "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries).ToList();

        /// <summary>
        /// Split a string by spaces.
        /// </summary>
        public static List<string> SplitBySpace(this string input) => input.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

        /// <summary>
        /// Count occurrences of a pattern.
        /// </summary>
        /// <remarks>https://www.dotnetperls.com/string-occurrence</remarks>
        public static int CountStringOccurrences(this string input, string pattern) {
            int count = 0;
            int i = 0;
            while ((i = input.IndexOf(pattern, i)) != -1) {
                i += pattern.Length;
                count++;
            }
            return count;
        }

        /// <summary>
        /// Check if string contains at least 1 url.
        /// </summary>
        public static bool ContainsUrls(this string input) {
            MatchCollection matches = _UrlRegex.Matches(input);
            return (matches.Count > 0) ? true : false;
        }

        /// <summary>
        /// Get all urls from a string.
        /// </summary>
        public static List<string> GetUrls(this string input) {
            MatchCollection matches = _UrlRegex.Matches(input);
            List<string> urls = new List<string>();

            foreach (Match match in matches) {
                urls.Add(match.Value);
            }

            return urls;
        }

        /// <summary>
        /// Tries to parse a provided user mention string.
        /// </summary>
        /// <remarks>https://github.com/discord-net/Discord.Net/blob/0275f7df507a2ad3f74be326488de2aa69bfccde/src/Discord.Net.Core/Utils/MentionUtils.cs#L53</remarks>
        public static bool TryParseDiscordUser(this string input, out ulong userId) {
            if (input.Length >= 3 && input[0] == '<' && input[1] == '@' && input[input.Length - 1] == '>') {
                input = input.Length >= 4 && input[2] == '!' ? input.Substring(3, input.Length - 4) : input.Substring(2, input.Length - 3);

                if (ulong.TryParse(input, NumberStyles.None, CultureInfo.InvariantCulture, out userId)) {
                    return true;
                }
            }

            userId = 0;
            return false;
        }

        /// <summary>
        /// Tries to parse a provided channel mention string.
        /// </summary>
        /// <remarks>https://github.com/discord-net/Discord.Net/blob/0275f7df507a2ad3f74be326488de2aa69bfccde/src/Discord.Net.Core/Utils/MentionUtils.cs#L82</remarks>
        public static bool TryParseDiscordChannel(this string input, out ulong channelId) {
            if (input.Length >= 3 && input[0] == '<' && input[1] == '#' && input[input.Length - 1] == '>') {
                input = input.Substring(2, input.Length - 3);

                if (ulong.TryParse(input, NumberStyles.None, CultureInfo.InvariantCulture, out channelId)) {
                    return true;
                }
            }

            channelId = 0;
            return false;
        }

        /// <summary>
        /// Tries to parse a provided role mention string.
        /// </summary>
        /// <remarks>https://github.com/discord-net/Discord.Net/blob/0275f7df507a2ad3f74be326488de2aa69bfccde/src/Discord.Net.Core/Utils/MentionUtils.cs#L108</remarks>
        public static bool TryParseDiscordRole(this string input, out ulong roleId) {
            if (input.Length >= 4 && input[0] == '<' && input[1] == '@' && input[2] == '&' && input[input.Length - 1] == '>') {
                input = input.Substring(3, input.Length - 4);

                if (ulong.TryParse(input, NumberStyles.None, CultureInfo.InvariantCulture, out roleId)) {
                    return true;
                }
            }

            roleId = 0;
            return false;
        }

        /// <summary>
        /// Parse a string for discord tags. Mentioned users, roles, channels, @everyone, and @here.
        /// </summary>
        /// <remarks>https://github.com/discord-net/Discord.Net/blob/0275f7df507a2ad3f74be326488de2aa69bfccde/src/Discord.Net.Rest/Entities/Messages/MessageHelper.cs#L98</remarks>
        public static List<MessageTag> ParseDiscordMessage(this string text) {
            List<MessageTag> tags = new List<MessageTag>();
            int index = 0;

            while (true) {
                index = text.IndexOf('<', index);
                if (index == -1) { break; }

                int endIndex = text.IndexOf('>', index + 1);
                if (endIndex == -1) { break; }

                string content = text.Substring(index, endIndex - index + 1);

                if (TryParseDiscordUser(content, out ulong userId)) {
                    tags.Add(new MessageTag(TagType.User, userId, index, content.Length));

                } else if (TryParseDiscordRole(content, out ulong roleId)) {
                    tags.Add(new MessageTag(TagType.Role, roleId, index, content.Length));

                } else if (TryParseDiscordChannel(content, out ulong channelId)) {
                    tags.Add(new MessageTag(TagType.Channel, channelId, index, content.Length));

                } else {
                    index = index + 1;
                    continue;
                }

                index = endIndex + 1;
            }

            index = 0;
            while (true) {
                index = text.IndexOf("@everyone", index);
                if (index == -1) { break; }

                int? tagIndex = FindIndex(tags, index);
                if (tagIndex.HasValue) {
                    tags.Add(new MessageTag(TagType.Everyone, 0, index, "@everyone".Length));
                }

                index++;
            }

            index = 0;
            while (true) {
                index = text.IndexOf("@here", index);
                if (index == -1) { break; }

                int? tagIndex = FindIndex(tags, index);
                if (tagIndex.HasValue) {
                    tags.Add(new MessageTag(TagType.Here, 0, index, "@here".Length));
                }

                index++;
            }

            return tags;
        }

        private static int? FindIndex(List<MessageTag> tags, int index) {
            int i = 0;
            for (; i < tags.Count; i++) {
                MessageTag tag = tags[i];
                if (index < tag.Index) {
                    break;
                }
            }

            return i > 0 && index < tags[i - 1].Index + tags[i - 1].Length ? null : (int?)i;
        }

    }

}
