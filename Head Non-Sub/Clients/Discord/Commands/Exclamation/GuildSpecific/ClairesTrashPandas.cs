using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Extensions;
using HeadNonSub.Statistics;
using ImageMagick;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation.GuildSpecific {

    // Claire's Trash Pandas
    [BlacklistEnforced, AllowedGuilds(471045301407449088)]
    [RequireContext(ContextType.Guild)]
    public class ClairesTrashPandas : ModuleBase<SocketCommandContext> {

        private string RequestedBy {
            get {
                if (Context.User is SocketGuildUser user) {
                    return $"{(!string.IsNullOrWhiteSpace(user.Nickname) ? user.Nickname : user.Username)} `{user.ToString()}`";
                } else {
                    return $"{Context.User.Username} `{Context.User.ToString()}`";
                }
            }
        }

        [Command("sbsays")]
        public Task StrongBadSaysAsync([Remainder]string input) {

            using (MemoryStream stream = new MemoryStream(100))
            using (MagickImage image = new MagickImage(Cache.GetStream("strongbadsays.png")))
            using (MagickImage overlay = new MagickImage(Cache.GetStream("strongbadsays_overlay.png"))) {

                string text = string.Join(Environment.NewLine, input.SplitIntoChunks(47));
                int max = (text.Length <= 580 ? text.Length : 580);
                text = text.Substring(0, max);

                new Drawables()
                  .FontPointSize(58)
                  .Font(Path.Combine(Constants.FontsDirectory, "VT323-Regular.ttf"))
                  .FillColor(new MagickColor("#CECCCC"))
                  .TextAlignment(TextAlignment.Left)
                  .TextAntialias(true)
                  .TextEncoding(Encoding.UTF8)
                  .Text(290, 190, text)
                  .Composite(new MagickGeometry(0, 0), overlay)
                  .Draw(image);

                image.Write(stream, MagickFormat.Png);

                stream.Seek(0, SeekOrigin.Begin);

                ulong reply = Context.Message.Channel.SendFileAsync(stream, "strongbadSays.png", text: $"● {RequestedBy}").Result.Id;
                if (!Context.IsPrivate) {
                    UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
                    StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
                }
            }

            return Task.CompletedTask;
        }

    }

}
