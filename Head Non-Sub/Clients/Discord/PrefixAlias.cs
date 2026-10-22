using System;
using System.Threading.Tasks;

namespace HeadNonSub.Clients.Discord {

    /// <summary>
    /// A ! text command that is an alias for a slash command, like !ttsays for /says.
    /// </summary>
    public sealed class PrefixAlias {

        /// <param name="name">Name typed after the prefix, like ttsays.</param>
        /// <param name="runAsync">Runs the command. Blacklisted users are already filtered out, everything else is up to the command.</param>
        public PrefixAlias(string name, Func<PrefixContext, Task> runAsync) {
            Name = name;
            RunAsync = runAsync;
        }

        public string Name { get; }
        public Func<PrefixContext, Task> RunAsync { get; }

    }

}
