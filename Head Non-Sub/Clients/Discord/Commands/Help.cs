using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using HeadNonSub.Statistics;

namespace HeadNonSub.Clients.Discord.Commands {

    [RequireContext(ContextType.Guild)]
    public class Help : ModuleBase<SocketCommandContext> {

        [Command("help")]
        public Task HelpAsync() {
            EmbedBuilder builder = new EmbedBuilder() {
                Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                Title = $"{Context.Client.CurrentUser.Username} Information and Commands"
            };

            builder.AddField("Information", "Some commands are hidden :eyes:");

            builder.AddField("Useful", string.Join(Environment.NewLine,
                            $"`!strawpoll Poll Title | Option 1 | Option 2 | Option 3` {Constants.DoubleSpace} Make a new strawpoll",
                            $"`!strawpollr <strawpoll url>` {Constants.DoubleSpace} Get results of a strawpoll",
                            $"`@{Context.Guild.CurrentUser.Username} random <role>` {Constants.DoubleSpace} Select a random user based on role",
                            $"`@{Context.Guild.CurrentUser.Username} undo` {Constants.DoubleSpace} Undo your and the bots recent message"));

            builder.AddField("Memes and Trash", string.Join(Environment.NewLine,
                                        $"`!yum` {Constants.DoubleSpace} Mmmm tasty",
                                        $"`!gimme` {Constants.DoubleSpace} You get thing",
                                        $"`!what` {Constants.DoubleSpace} tt confuse",
                                        $"`!moneyshot` {Constants.DoubleSpace} What she sees",
                                        $"`!fuckedup` {Constants.DoubleSpace} Definitely not getting the deposit back",
                                        $"`!rnk` {Constants.DoubleSpace} See your rank. People love ranks",
                                        $"`!bigoof` {Constants.DoubleSpace} oof... a big oof",
                                        $"`!1024nude` {Constants.DoubleSpace} Yes the photo is really 1024x768",
                                        $"`oof oof` {Constants.DoubleSpace} oof.",
                                        $"`oof floof` {Constants.DoubleSpace} Awww."));

            builder.Footer = new EmbedFooterBuilder() {
                Text = $"{Constants.ApplicationName} by {Constants.Creator}"
            };

            ulong reply = ReplyAsync(embed: builder.Build()).Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
            return Task.CompletedTask;
        }

    }

}
