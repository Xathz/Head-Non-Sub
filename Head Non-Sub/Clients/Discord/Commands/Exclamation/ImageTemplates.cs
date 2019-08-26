using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Extensions;
using ImageMagick;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [BlacklistEnforced]
    [SubscriberOnly]
    [RequireContext(ContextType.Guild)]
    public class ImageTemplates : BetterModuleBase {

        [Command("ttsays")]
        public async Task TTSays([Remainder]string input) {
            await Context.Channel.TriggerTypingAsync();

            string imageName = "ttsays.png";

            // 5% chance to be old tt
            if (new Random().Next(0, 100) >= 95) {
                imageName = "ttsays_old.png";
            }

            using (MemoryStream stream = new MemoryStream(256))
            using (MagickImage image = new MagickImage(Cache.GetStream(imageName))) {

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

                await BetterSendFileAsync(stream, "ttSays.png", $"● {BetterUserFormat()}", parameters: input);
            }
        }

        [Command("1024says")]
        public async Task TenTwentyFourSays([Remainder]string input = "") {
            await Context.Channel.TriggerTypingAsync();

            await BetterSendFileAsync(Cache.GetStream("1024notfound.gif"), "1024notfound.gif", $"● {BetterUserFormat()}", parameters: input);

            //using (MemoryStream stream = new MemoryStream(256))
            //using (MagickImage image = new MagickImage(Cache.GetStream("1024says.png"))) {

            //    string text = string.Join(Environment.NewLine, input.SplitIntoChunks(25));
            //    int max = (text.Length <= 200 ? text.Length : 200);
            //    text = text.Substring(0, max);

            //    new Drawables()
            //      .FontPointSize(124)
            //      .Font(Path.Combine(Constants.FontsDirectory, "VT323-Regular.ttf"))
            //      .Rotation(3.5)
            //      .SkewX(5.0)
            //      .FillColor(new MagickColor("#FFFFFF"))
            //      .TextAlignment(TextAlignment.Left)
            //      .TextAntialias(true)
            //      .TextEncoding(Encoding.UTF8)
            //      .Text(765, 400, text)
            //      .Draw(image);

            //    image.Write(stream, MagickFormat.Png);
            //    stream.Seek(0, SeekOrigin.Begin);

            //    await BetterSendFileAsync(stream, "1024Says.png", $"● {BetterUserFormat()}", parameters: input);
            //}
        }

        [Command("amandasays")]
        public async Task AmandaSays([Remainder]string input) {
            await Context.Channel.TriggerTypingAsync();

            using (MemoryStream stream = new MemoryStream(256))
            using (MagickImage image = new MagickImage(Cache.GetStream("amandasays.png"))) {

                string text = string.Join(Environment.NewLine, input.SplitIntoChunks(30));
                int max = (text.Length <= 220 ? text.Length : 220);
                text = text.Substring(0, max);

                new Drawables()
                  .FontPointSize(48)
                  .Font(Path.Combine(Constants.FontsDirectory, "ConcertOne-Regular.ttf"))
                  .FillColor(new MagickColor("#404040"))
                  .TextAlignment(TextAlignment.Left)
                  .TextAntialias(true)
                  .TextEncoding(Encoding.UTF8)
                  .Text(730, 260, text)
                  .Draw(image);

                image.Write(stream, MagickFormat.Png);
                stream.Seek(0, SeekOrigin.Begin);

                await BetterSendFileAsync(stream, "amandaSays.png", $"● {BetterUserFormat()}", parameters: input);
            }
        }

        [Command("satasays")]
        public async Task SataSays([Remainder]string input) {
            await Context.Channel.TriggerTypingAsync();

            using (MemoryStream stream = new MemoryStream(256))
            using (MagickImage image = new MagickImage(Cache.GetStream("satasays_ariana.png"))) {

                string text = string.Join(Environment.NewLine, input.SplitIntoChunks(34));
                int max = (text.Length <= 230 ? text.Length : 230);
                text = text.Substring(0, max);

                new Drawables()
                  .FontPointSize(48)
                  .Font(Path.Combine(Constants.FontsDirectory, "ConcertOne-Regular.ttf"))
                  .FillColor(new MagickColor("#EFE1E1"))
                  .TextAlignment(TextAlignment.Left)
                  .TextAntialias(true)
                  .TextEncoding(Encoding.UTF8)
                  .Text(440, 62, text)
                  .Draw(image);

                image.Write(stream, MagickFormat.Png);
                stream.Seek(0, SeekOrigin.Begin);

                await BetterSendFileAsync(stream, "sataSays.png", $"● {BetterUserFormat()}", parameters: input);
            }
        }

        [Command("jibersays")]
        public async Task JiberSays([Remainder]string input) {
            await Context.Channel.TriggerTypingAsync();

            using (MemoryStream stream = new MemoryStream(256))
            using (MagickImage image = new MagickImage(Cache.GetStream("jibersays.png"))) {

                string text = string.Join(Environment.NewLine, input.SplitIntoChunks(33));
                int max = (text.Length <= 245 ? text.Length : 245);
                text = text.Substring(0, max);

                new Drawables()
                  .FontPointSize(64)
                  .Font(Path.Combine(Constants.FontsDirectory, "Courgette-Regular.ttf"))
                  .FillColor(new MagickColor("#CFB491"))
                  .TextAlignment(TextAlignment.Left)
                  .TextAntialias(true)
                  .TextEncoding(Encoding.UTF8)
                  .Text(500, 290, text)
                  .Draw(image);

                image.Write(stream, MagickFormat.Png);
                stream.Seek(0, SeekOrigin.Begin);

                await BetterSendFileAsync(stream, "jiberSays.png", $"● {BetterUserFormat()}", parameters: input);
            }
        }

        [Command("sbsays")]
        public async Task StrongBadSays([Remainder]string input) {
            await Context.Channel.TriggerTypingAsync();

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
                  .Draw(image);

                image.Composite(overlay, CompositeOperator.Atop);

                image.Write(stream, MagickFormat.Png);
                stream.Seek(0, SeekOrigin.Begin);

                await BetterSendFileAsync(stream, "strongbadSays.png", $"● {BetterUserFormat()}", parameters: input);
            }
        }

        [Command("warm")]
        public async Task Warm(SocketUser user = null, [Remainder]string input = "") {
            if (user == null) {
                await BetterReplyAsync("You must provide a user to warm.", parameters: $"user null; {input}");
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            try {
                Task<MemoryStream> download = Http.GetStreamAsync(user.GetAvatarUrl(ImageFormat.Png, 256));

                using (MemoryStream data = await download) {
                    if (download.IsCompletedSuccessfully) {
                        using (MemoryStream stream = new MemoryStream(256))
                        using (MagickImage image = new MagickImage(Cache.GetStream("warm.png")))
                        using (MagickImage avatar = new MagickImage(data)) {

                            avatar.BackgroundColor = new MagickColor(0, 0, 0, 0);
                            avatar.Resize(225, 225);
                            avatar.Rotate(-5.5);

                            image.Composite(avatar, 160, 132, CompositeOperator.Atop);

                            image.Resize(256, 256);
                            image.Write(stream, MagickFormat.Png);

                            stream.Seek(0, SeekOrigin.Begin);

                            await BetterSendFileAsync(stream, "warm.png", $"{BetterUserFormat(user)} has been warmed{(string.IsNullOrWhiteSpace(input) ? "" : $" for `{input}`")}.", parameters: $"{user.ToString()} ({user.Id}); {input}");
                        }
                    } else {
                        throw download.Exception;
                    }
                }
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
            }
        }

    }

}
