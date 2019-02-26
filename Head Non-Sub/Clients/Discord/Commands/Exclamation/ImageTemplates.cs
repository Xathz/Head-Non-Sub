using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using HeadNonSub.Extensions;
using ImageMagick;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [RequireContext(ContextType.Guild)]
    public class ImageTemplates : ModuleBase<SocketCommandContext> {

        [Command("ttsays")]
        public Task TTSaysAsync([Remainder]string input) {

            using (MemoryStream stream = new MemoryStream(100))
            using (MagickImage image = new MagickImage(Path.Combine(Constants.ContentDirectory, "ttTemplate.png"))) {

                string text = string.Join(Environment.NewLine, input.SplitIntoChunks(26));
                int max = (text.Length <= 150 ? text.Length : 150);
                text = text.Substring(0, max);

                new Drawables()
                  .FontPointSize(40)
                  .Font(Path.Combine(Constants.FontsDirectory, "Sloppy-Hand.otf"))
                  .FillColor(new MagickColor("#B0AEAD"))
                  .TextAlignment(TextAlignment.Left)
                  .TextAntialias(true)
                  .TextEncoding(Encoding.UTF8)
                  .Text(218, 45, text)
                  .Draw(image);

                image.Write(stream, MagickFormat.Png);

                stream.Seek(0, SeekOrigin.Begin);

                ulong reply = Context.Message.Channel.SendFileAsync(stream, "ttSays.png").Result.Id;
                UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            }

            return Task.CompletedTask;
        }

    }

}
