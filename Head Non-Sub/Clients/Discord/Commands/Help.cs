using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace HeadNonSub.Clients.Discord.Commands {

    public class Help : ModuleBase<SocketCommandContext> {

        // https://discordapp.com/developers/docs/resources/channel#embed-limits

        [Command("help")]
        [RequireContext(ContextType.Guild)]
        public Task HelpAsync() {
            EmbedBuilder builder = new EmbedBuilder() {
                Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                Title = $"{Context.Client.CurrentUser.Username} Information and Commands"
            };

            builder.AddField("Information", "Some commands are hidden :eyes:");

            builder.AddField("Commands", string.Join(Environment.NewLine,
                                        $"`!yum` {Constants.DoubleSpace} Mmmm tasty",
                                        $"`!gimme` {Constants.DoubleSpace} U get thing",
                                        $"`!what` {Constants.DoubleSpace} tt confuse",
                                        $"`!moneyshot` {Constants.DoubleSpace} what she sees",
                                        $"`!fuckedup` {Constants.DoubleSpace} Definitely not getting the deposit back",
                                        $"`!rnk` {Constants.DoubleSpace} See your rank. People love ranks",
                                        $"`@{Context.Guild.CurrentUser.Username} random <role>` {Constants.DoubleSpace} Select a random user based on role",
                                        $"`@{Context.Guild.CurrentUser.Username} undo` {Constants.DoubleSpace} Undo your and the bots recent message",
                                        $"`oof oof` {Constants.DoubleSpace} oof.",
                                        $"`oof floof` {Constants.DoubleSpace} Awww."));

            builder.Footer = new EmbedFooterBuilder() {
                Text = $"{Constants.ApplicationName} by {Constants.Creator}"
            };

            ulong reply = ReplyAsync(embed: builder.Build()).Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            return Task.CompletedTask;
        }

    }

}
