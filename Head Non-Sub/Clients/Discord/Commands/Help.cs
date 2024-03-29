﻿using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using HeadNonSub.Clients.Discord.Attributes;

namespace HeadNonSub.Clients.Discord.Commands {

    [BlacklistEnforced]
    [RequireContext(ContextType.Guild)]
    public class Help : BetterModuleBase {

        [Command("help")]
        public Task HelpGeneral() {
            EmbedBuilder builder = new EmbedBuilder() {
                Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                Title = $"{Context.Client.CurrentUser.Username} Information and Commands"
            };

            builder.AddField("Commands", $"There are too many to list here, [click here for the list]({Constants.CommandsHelpUrl}).");

            builder.Footer = new EmbedFooterBuilder() {
                Text = $"{Constants.ApplicationName} by {Constants.Creator}"
            };

            return BetterReplyAsync(builder.Build());
        }

    }

}
