using System;
using System.Threading.Tasks;

namespace HeadNonSub.Clients.Discord {

    /// <summary>
    /// A ! text command that is an alias for a slash command, like !ttsays for /says.
    /// </summary>
    public sealed class PrefixAlias {

        /// <param name="name">Name typed after the prefix, like ttsays.</param>
        /// <param name="slashCommand">Full name of the slash command this is an alias for, like says or blacklist add. Its preconditions run first, the same as using it.</param>
        /// <param name="runAsync">Runs the command once the preconditions pass. Checks that are inside the slash command method, like per-template cooldowns, are up to it.</param>
        public PrefixAlias(string name, string slashCommand, Func<PrefixContext, Task> runAsync) {
            Name = name;
            SlashCommand = slashCommand;
            RunAsync = runAsync;
        }

        public string Name { get; }
        public string SlashCommand { get; }
        public Func<PrefixContext, Task> RunAsync { get; }

    }

}
