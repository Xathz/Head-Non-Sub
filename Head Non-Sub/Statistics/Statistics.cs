using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace HeadNonSub.Statistics {

    public class Statistics : IDirty {

        /// <summary>
        /// Get if any classes are marked as dirty.
        /// </summary>
        [JsonIgnore]
        public bool IsDirty => _Commands.Any(x => x.Value.IsDirty);

        /// <summary>
        /// Set all classes as clean.
        /// </summary>
        public void MarkClean() => _Commands.ToList().ForEach(x => x.Value.MarkClean());

        [JsonProperty("commands")]
        private Dictionary<ulong, Commands> _Commands = new Dictionary<ulong, Commands>();

        /// <summary>
        /// Get the <see cref="HeadNonSub.Statistics.Commands"/> for a guild (server) id.
        /// </summary>
        public Commands Commands(ulong guild) {
            if (_Commands.ContainsKey(guild)) {
                return _Commands[guild];

            } else {
                Commands commands = new Commands();
                _Commands.Add(guild, commands);

                return commands;
            }
        }

    }

}
