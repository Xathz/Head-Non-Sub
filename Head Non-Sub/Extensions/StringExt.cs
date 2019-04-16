using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace HeadNonSub.Extensions {

    public static class StringExt {

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

        #region Url Parsing
        // Taken from Chatty since they did it better than I could - https://github.com/chatty/chatty
        // https://github.com/chatty/chatty/blob/888116a8b8f404eb635880457111db15ff969acb/src/chatty/Helper.java#L557

        // gtld/cctld list: https://data.iana.org/TLD/tlds-alpha-by-domain.txt

        private static readonly string _TLD = $"(?:{string.Join("|", Cache.TLDs)})";

        private static readonly string _MID = "[-A-Z0-9+&@#/%=~_|$?!:,;.()]";

        private static readonly string _END = "[A-Z0-9+&@#/%=~_|$)]";

        private static readonly string _S1 = "(?:(?:https?)://|www\\.)";

        private static readonly string _S2 = $"(?:[A-Z0-9.-]+[A-Z0-9]\\.{_TLD}\\b)";

        // Complete URL (match both _S1 and _S2)
        private static readonly string _T1 = $"(?:(?:{_S1}{_S2}){_MID}*{_END})";

        // Complete URL (only domain)
        private static readonly string _T2 = $"(?:{_S2})";

        // The regex String for finding URLs in messages.
        private static readonly string _Pattern = $"(?i)\\b{_T1}|{_T2}";

        private static readonly Regex _Regex = new Regex(_Pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Check if string contains at least 1 url.
        /// </summary>
        public static bool ContainsUrls(this string input) {
            MatchCollection matches = _Regex.Matches(input);
            return (matches.Count > 0) ? true : false;
        }

        /// <summary>
        /// Get all urls from a string.
        /// </summary>
        public static List<string> GetUrls(this string input) {
            MatchCollection matches = _Regex.Matches(input);
            List<string> urls = new List<string>();

            foreach (Match match in matches) {
                urls.Add(match.Value);
            }

            return urls;
        }
        #endregion

    }

}
