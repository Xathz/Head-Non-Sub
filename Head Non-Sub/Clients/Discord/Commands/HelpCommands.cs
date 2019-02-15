using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace HeadNonSub.Clients.Discord.Commands {

    public class HelpCommands : ModuleBase<SocketCommandContext> {

        // https://discordapp.com/developers/docs/resources/channel#embed-limits

        [Command("help")]
        [RequireContext(ContextType.Guild)]
        public Task HelpAsync() {
            EmbedBuilder builder = new EmbedBuilder() {
                Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                Title = $"{Context.Client.CurrentUser.Username} Information and Commands"
            };

            builder.AddField("Am I a real non-sub?", ":eyes:");

            builder.Footer = new EmbedFooterBuilder() {
                Text = $"{Constants.ApplicationName} by {Constants.Creator}"
            };

            return ReplyAsync(embed: builder.Build());
        }

    }

}
