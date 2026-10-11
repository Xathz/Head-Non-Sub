using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using HeadNonSub.Entities.Discord;

namespace HeadNonSub.Extensions {

    public static class StringExt {

        private static readonly Regex _UrlRegex = new Regex(@"https?://[^\s]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex _EmojiRegex = new Regex(@"[0-9]?\#?\*?(\u00a9|\u00ae|[\u2000-\u3300]|\ud83c[\ud000-\udfff]|\ud83d[\ud000-\udfff]|\ud83e[\ud000-\udfff])+", RegexOptions.Compiled);

        private static readonly string[] _MarkdownSensitiveCharacters = { "\\", "*", "_", "~", "`", "|", ">" };

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
        /// Split a string by all possible new line and return characters.
        /// </summary>
        public static List<string> SplitByNewLines(this string input) => input.Split(new string[] { Environment.NewLine, "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries).ToList();

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
            return matches.Count > 0;
        }

        /// <summary>
        /// Get all urls from a string.
        /// </summary>
        public static List<string> GetUrls(this string input) {
            MatchCollection matches = _UrlRegex.Matches(input);
            List<string> urls = new List<string>();

            foreach (Match match in matches) {
                string url = match.Value;
                url = url.Replace("<", "");
                url = url.Replace(">", "");
                url = url.Replace("%3C", "", StringComparison.InvariantCultureIgnoreCase);
                url = url.Replace("%3E", "", StringComparison.InvariantCultureIgnoreCase);

                urls.Add(url);
            }

            return urls;
        }

        /// <summary>
        /// Get all unicode code points from a string.
        /// </summary>
        /// <remarks>https://stackoverflow.com/a/32141891</remarks>
        public static IEnumerable<int> GetUnicodeCodePoints(this string input) {
            for (int i = 0; i < input.Length; i++) {
                int unicodeCodePoint = char.ConvertToUtf32(input, i);
                if (unicodeCodePoint > 0xffff) {
                    i++;
                }

                yield return unicodeCodePoint;
            }
        }

        /// <summary>
        /// Tries to parse an <see cref="EmoteOrEmoji"/> from its raw format.
        /// </summary>
        /// <remarks>https://github.com/discord-net/Discord.Net/blob/0275f7df507a2ad3f74be326488de2aa69bfccde/src/Discord.Net.Core/Entities/Emotes/Emote.cs#L79</remarks>
        public static bool TryParseDiscordEmote(this string input, out EmoteOrEmoji result) {
            result = null;

            if (input.Length >= 4 && input[0] == '<' && (input[1] == ':' || (input[1] == 'a' && input[2] == ':')) && input[^1] == '>') {
                bool animated = input[1] == 'a';
                int startIndex = animated ? 3 : 2;

                int splitIndex = input.IndexOf(':', startIndex);
                if (splitIndex == -1) {
                    return false;
                }

                if (!ulong.TryParse(input.AsSpan(splitIndex + 1, input.Length - splitIndex - 2), NumberStyles.None, CultureInfo.InvariantCulture, out ulong id)) {
                    return false;
                }

                string name = input[startIndex..splitIndex];
                result = new EmoteOrEmoji(id, name, animated);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Parse a string for emotes.
        /// </summary>
        public static List<EmoteOrEmoji> ParseDiscordMessageEmotes(this string input) {
            List<EmoteOrEmoji> emotes = new List<EmoteOrEmoji>();
            int index = 0;

            while (true) {
                index = input.IndexOf('<', index);
                if (index == -1) { break; }

                int endIndex = input.IndexOf('>', index + 1);
                if (endIndex == -1) { break; }

                string content = input.Substring(index, endIndex - index + 1);

                if (TryParseDiscordEmote(content, out EmoteOrEmoji emote)) {
                    emotes.Add(new EmoteOrEmoji(emote.Id, emote.Name, emote.Animated));

                } else {
                    index += 1;
                    continue;
                }

                index = endIndex + 1;
            }

            MatchCollection matches = _EmojiRegex.Matches(input);
            foreach (Match match in matches) {
                emotes.Add(new EmoteOrEmoji(match.Value));
            }

            return emotes;
        }

        /// <summary>
        /// Sanitizes the string, safely escaping any markdown sequences.
        /// </summary>
        /// <remarks>https://git.io/JeY0Y</remarks>
        public static string SanitizeForMarkdown(this string input, bool usedInCodeBlock = false) {
            if (string.IsNullOrWhiteSpace(input)) { return input; }

            if (usedInCodeBlock) {
                input = input.Replace("`", "'");
            } else {
                foreach (string unsafeChar in _MarkdownSensitiveCharacters) {
                    input = input.Replace(unsafeChar, $"\\{unsafeChar}");
                }
            }

            return input.Trim();
        }

    }

}
