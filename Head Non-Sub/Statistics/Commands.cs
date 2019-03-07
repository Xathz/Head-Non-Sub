using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HeadNonSub.Extensions;
using Newtonsoft.Json;

namespace HeadNonSub.Statistics {

    public class Commands : IDirty {

        /// <summary>
        /// If anything changed.
        /// </summary>
        [JsonIgnore]
        public bool IsDirty { get; private set; } = false;

        /// <summary>
        /// Set <see cref="IsDirty"/> to <see langword="false" />.
        /// </summary>
        public void MarkClean() => IsDirty = false;

        #region Execute Count

        [JsonProperty("executeCount")]
        private ConcurrentDictionary<string, ulong> _ExecuteCount = new ConcurrentDictionary<string, ulong>();

        /// <summary>
        /// Increase the execute count by one. Will use <see cref="CallerMemberNameAttribute"/> if <paramref name="caller"/> is not specified.
        /// </summary>
        public void Executed([CallerMemberName] string caller = "") {
            if (string.IsNullOrWhiteSpace(caller)) { return; }

            _ExecuteCount.AddOrUpdate(caller, 1, (key, value) => ++value);
            IsDirty = true;
        }

        /// <summary>
        /// Get the number of times executed. Will use <see cref="CallerMemberNameAttribute"/> if <paramref name="caller"/> is not specified.
        /// </summary>
        /// <returns>Zero if invalid caller/key does not exist or the execute count if a valid caller.</returns>
        public ulong TimesExecuted([CallerMemberName] string caller = "") {
            bool valid = _ExecuteCount.TryGetValue(caller, out ulong value);

            if (valid) {
                return value;
            } else {
                return 0;
            }
        }

        #endregion

        #region TTS Word Count

        [JsonProperty("ttsWordCount")]
        private ConcurrentDictionary<string, ulong> _TTSWordCount = new ConcurrentDictionary<string, ulong>();

        /// <summary>
        /// Separate each word and count how many times uses overall.
        /// </summary>
        public void TTSMessage(string input) {
            if (string.IsNullOrWhiteSpace(input)) { return; }

            List<string> words = input.SplitBySpace();
            foreach (string word in words) {
                _TTSWordCount.AddOrUpdate(word.Trim().ToLower(), 1, (key, value) => ++value);
            }

            IsDirty = true;
        }

        /// <summary>
        /// Get the top words used in TTS messages.
        /// </summary>
        public List<KeyValuePair<string, ulong>> TTSTopWords(int count = 10, int minimumWordLength = 2) => _TTSWordCount.Where(x => x.Key.Length > minimumWordLength).OrderByDescending(x => x.Value).Take(count).ToList();

        #endregion

    }

}
