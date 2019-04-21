using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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

    }

}
