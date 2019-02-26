using System.Collections.Generic;

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

    }

}
