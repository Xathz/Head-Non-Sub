using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;
using HeadNonSub.Extensions;

namespace HeadNonSub.Clients.Discord.Commands {

    public class Insult : ModuleBase<SocketCommandContext> {

        // https://discordapp.com/developers/docs/resources/channel#embed-limits

        [Command("you suck")]
        [RequireContext(ContextType.Guild)]
        public Task YouSuckAsync() {

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
                "Nani the FUCK?"
            };

            ulong reply = ReplyAsync(responses.PickRandom()).Result.Id;

            DiscordMessageTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            return Task.CompletedTask;
        }

    }

}
