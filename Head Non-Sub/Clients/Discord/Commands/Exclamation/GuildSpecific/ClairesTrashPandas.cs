using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Extensions;
using ImageMagick;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation.GuildSpecific {

    // Claire's Trash Pandas
    [BlacklistEnforced, AllowedGuilds(471045301407449088)]
    [RequireContext(ContextType.Guild)]
    public class ClairesTrashPandas : BetterModuleBase {

        [Command("sbsays")]
        public Task StrongBadSays([Remainder]string input) {

            using (MemoryStream stream = new MemoryStream(256))
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

                BetterSendFileAsync(stream, "strongbadSays.png", $"● {BetterUserFormat()}").Wait();
            }

            return Task.CompletedTask;
        }

    }

}
