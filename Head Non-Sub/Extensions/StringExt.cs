using System;
using System.Collections.Generic;
using System.Linq;

namespace HeadNonSub.Extensions {

    public static class StringExt {

        /// <summary>
        /// Split a string into chunks based on max length.
        /// </summary>
        public static List<string> SplitIntoChunks(this string input, int maxLength) {
            string[] words = input.Split(' ');
            List<string> parts = new List<string>();
            string part = string.Empty;
            int partCounter = 0;

            foreach (string word in words) {
                if (part.Length + word.Length < maxLength) {
                    part += string.IsNullOrEmpty(part) ? word : $" {word}";
                } else {
                    parts.Add(part);

                    part = word;
                    partCounter++;
                }
            }
            parts.Add(part);

            return parts;
        }

        /// <summary>
        /// Truncate the string.
        /// </summary>
        /// <remarks>https://stackoverflow.com/a/2776720</remarks>
        public static string Truncate(this string input, int maxLength) => string.IsNullOrEmpty(input) ? input : input.Length <= maxLength ? input : input.Substring(0, maxLength);

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

    }

}
