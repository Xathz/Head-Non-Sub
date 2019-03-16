using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Extensions;

namespace HeadNonSub.Clients.Discord.Commands {

    [BlacklistEnforced]
    [RequireContext(ContextType.Guild)]
    public class FakeChat : BetterModuleBase {

        [Command("you suck")]
        [Alias("i hate you", "fuck you", "fuck off", "die")]
        public Task YouSuck() {

            List<string> responses = new List<string> {
                "i am poor :cry:",
                ":open_mouth: y tho",
                "I wonder how many racist things I can say before the mods ban me.",
                "I was being purposefully retarded",
                "listen here fucko",
                "oof",
                "PLS unban me from discord I am sorry for sending that stupid word art, also this song was spammed a lot in 2009",
                "1024 warned me i am leving this place",
                $"Alright, {Context.Guild.CurrentUser.Mention} has been warned for '**Being a useless person**'... wait, what?",
                "but its F E E D I N G T I M E",
                "Nani the FUCK?",
                "ok?",
                "I'm subbed on youtube tho"
            };

            return BetterReplyAsync(responses.PickRandom());
        }

        [Command("you are great")]
        [Alias("i love you", "how are you", "nice to see you", "i like you")]
        public Task YouAreGreat() {

            List<string> responses = new List<string> {
                "awww",
                ":heart:",
                "I hope all subs are as nice as you!",
                "camryn might be 14, but she is 18 in my heart",
                "thats so sweet",
                "Thats really nice but please give me a sub",
                "d'awww",
                "people are much nicer here than the ninja discord",
                "do you think wubby could still love me as a non-sub?"
            };

            return BetterReplyAsync(responses.PickRandom());
        }

        [Command("good bot")]
        [AllowedUsers(227088829079617536)] // Xathz
        public Task GoodBot() {

            List<string> responses = new List<string> {
                "Damn right.",
                "Yea, fuck them.",
                "<@227088829079617536> thanks for making me!",
                "Yea everyone else sucks."
            };

            return BetterReplyAsync(responses.PickRandom());
        }

    }

}
