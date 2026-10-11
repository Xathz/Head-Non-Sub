using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;

namespace HeadNonSub.Clients.Discord.Commands {

    [BlacklistEnforced]
    [CommandContextType(InteractionContextType.Guild)]
    public class Images : BetterModuleBase {

        /// <summary>
        /// A picture or bit of art that can be posted with /image.
        /// </summary>
        public sealed class Entry {

            /// <param name="name">Name shown in the command and used as the cooldown key.</param>
            /// <param name="statisticsName">Command name recorded in statistics, matches the old text command.</param>
            /// <param name="file">Cached file to send, <see langword="null"/> to send <paramref name="text"/> instead.</param>
            /// <param name="text">Text to send when there is no file, or extra text after the user for a file.</param>
            /// <param name="cooldownSeconds">Cooldown length in seconds.</param>
            /// <param name="perUser">Is the cooldown per-user (true) or server wide (false).</param>
            /// <param name="aliases">Other names that find this entry.</param>
            public Entry(string name, string statisticsName, string file, string text = "", int cooldownSeconds = 20, bool perUser = false, params string[] aliases) {
                Name = name;
                StatisticsName = statisticsName;
                File = file;
                Text = text;
                CooldownSeconds = cooldownSeconds;
                PerUser = perUser;
                Aliases = aliases;
            }

            public string Name { get; }
            public string StatisticsName { get; }
            public string File { get; }
            public string Text { get; }
            public int CooldownSeconds { get; }
            public bool PerUser { get; }
            public string[] Aliases { get; }

        }

        public static readonly IReadOnlyList<Entry> Entries = new List<Entry> {
            new Entry("bigoof", "BigOof", "big_oof.gif"),
            new Entry("dab", "Dab", "dab.gif"),
            new Entry("dickdowndennis", "DickdownDennis", "dickdowndennis.png"),
            new Entry("fuckedup", "FuckedUp", "fucked_up.png", aliases: "ripdeposit"),
            new Entry("gimme", "Gimme", null, "<:wubbydrugs:361993520040640516>"),
            new Entry("gongboy", "GongBoy", "gongboy.png"),
            new Entry("gottem", "Gottem", "gottem.gif", aliases: "gotem"),
            new Entry("juan", "Juan", "juan.png"),
            new Entry("keyboard", "WubbyKeyboard", "wubbykeyboard.png"),
            new Entry("lucky", "Lucky", "lucky.png"),
            new Entry("moneyshot", "MoneyShot", "moneyshot.png"),
            new Entry("night", "Night", null, NightText),
            new Entry("1024nude", "TenTwentyFourNude", "1024_nude.png"),
            new Entry("potato", "Potato", "potato.png"),
            new Entry("radar", "Radar", "radar.png", ", created by SimplyMoose"),
            new Entry("relax", "Relax", "relax.png"),
            new Entry("rnk", "Rnk", "rnk.png", cooldownSeconds: 3600, perUser: true),
            new Entry("star", "Star", "star.gif"),
            new Entry("true", "ThatsTrue", "true.png", aliases: "thatstrue"),
            new Entry("weebmas", "Weebmas", "wubbypadoru_xmas.gif", ", created by Xaffiri"),
            new Entry("weebshit", "WeebShit", "weebshit.png"),
            new Entry("what", "What", "what.png"),
            new Entry("wubbycheeto", "WubbyCheeto", "wubbycheeto.png", ", created by icy_ann"),
            new Entry("yum", "Yum", "yum.png")
        };

        /// <summary>
        /// Find an entry by name or alias.
        /// </summary>
        /// <param name="name">Name or alias to find.</param>
        public static Entry Find(string name) => Entries.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase) || x.Aliases.Contains(name, StringComparer.OrdinalIgnoreCase));

        [SlashCommand("image", "Post a picture")]
        public async Task Image([Summary("name", "Which picture to post"), Autocomplete(typeof(EntryAutocomplete))] string name) {
            Entry entry = Find(name);

            if (entry is null) {
                await EphemeralReplyAsync($"There is no picture named `{name}`.");
                return;
            }

            string cooldown = Cooldowns.Check((SocketGuildUser)Context.User, entry.Name, entry.CooldownSeconds, entry.PerUser);
            if (cooldown != null) {
                await EphemeralReplyAsync(cooldown);
                return;
            }

            // Large gifs can take a while to upload
            await DeferAsync();

            if (entry.File is null) {
                await BetterReplyAsync(entry.Text, command: entry.StatisticsName);
            } else {
                await BetterSendFileAsync(Cache.GetStream(entry.File), entry.File, $"● {BetterUserFormat()}{entry.Text}", command: entry.StatisticsName);
            }
        }

        public class EntryAutocomplete : AutocompleteHandler {

            public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services) {
                string input = autocompleteInteraction.Data.Current.Value?.ToString() ?? "";

                // Discord shows at most 25 suggestions
                IEnumerable<AutocompleteResult> results = Entries
                    .Where(x => x.Name.Contains(input, StringComparison.OrdinalIgnoreCase) || x.Aliases.Any(a => a.Contains(input, StringComparison.OrdinalIgnoreCase)))
                    .Take(25)
                    .Select(x => new AutocompleteResult(x.Name, x.Name));

                return Task.FromResult(AutocompletionResult.FromSuccess(results));
            }

        }

        private const string NightText = @".　　　　　　　　　　 ✦ 　　　　   　 　　　˚　　　　　　　　　　　　　　*　　　　　　   　　　　　　　　　　　　　　　.　　　　　　　　　　　　　　. 　　 　　　　　　　 ✦ 　　　　　　　　　　 　 ‍ ‍ ‍ ‍ 　　　　 　　　　　　　　　　　　,　　   　

.　　　　　　　　　　　　　.　　　ﾟ　  　　　.　　　　　　　　　　　　　.

　　　　　　,　　　　　　　.　　　　　　    　　　　 　　　　　　　　　　　　　　　　　　 ☀️ 　　　　　　　　　　　　　　　　　　    　      　　　　　        　　　　　　　　　　　　　. 　　　　　　　　　　.　　　　　　　　　　　　　. 　　　　　　　　　　　　　　　　       　   　　　　 　　　　　　　　　　　　　　　　       　   　　　　　　　　　　　　　　　　       　    ✦ 　   　　　,　　　　　　　　　　　    🚀 　　　　 　　,　　　 ‍ ‍ ‍ ‍ 　 　　　　　　　　　　　　.　　　　　 　　 　　　.　　　　　　　　　　　　　 　           　　　　　　　　　　　　　　　　　　　˚　　　 　   　　　　,　　　　　　　　　　　       　    　　　　　　　　　　　　　　　　.　　　  　　    　　　　　 　　　　　.　　　　　　　　　　　　　.　　　　　　　　　　　　　　　* 　　   　　　　　 ✦ 　　　　　　　         　        　　　　 　　 　　　　　　　 　　　　　.　　　　　　　　　　　　　　　　　　.　　　　　    　　. 　 　　　　　.　　　　 🌑 　　　　　   　　　　　.　　　　　　　　　　　.　　　　　　　　　　   　

　˚　　　　　　　　　　　　　　　　　　　　　ﾟ　　　　　.　　　　　　　　　　　　　　　. 　　 　 🌎 ‍ ‍ ‍ ‍ ‍ ‍ ‍ ‍ ‍ ‍ ,　 　　　　　　　　　　　　　　* .　　　　　 　　　　　　　　　　　　　　.　　　　　　　　　　 ✦ 　　　　   　 　　　˚　　　　　　　　　　　　　　*　　　　　　 ";

    }

}
