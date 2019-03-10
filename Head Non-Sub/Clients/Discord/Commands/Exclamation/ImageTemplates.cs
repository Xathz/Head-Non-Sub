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

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [BlacklistEnforced]
    public class ImageTemplates : ModuleBase<SocketCommandContext> {

        private string RequestedBy {
            get {
                if (Context.User is SocketGuildUser user) {
                    return $"{(!string.IsNullOrWhiteSpace(user.Nickname) ? user.Nickname : user.Username)} `{user.ToString()}`";
                } else {
                    return $"{Context.User.Username} `{Context.User.ToString()}`";
                }
            }
        }

        [Command("ttsays")]
        public Task TTSaysAsync([Remainder]string input) {

            using (MemoryStream stream = new MemoryStream(100))
            using (MagickImage image = new MagickImage(Cache.GetStream("ttsays.png"))) {

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

                ulong reply = Context.Message.Channel.SendFileAsync(stream, "ttSays.png", text: $"● {RequestedBy}").Result.Id;
                if (!Context.IsPrivate) {
                    UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
                    StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
                }
            }

            return Task.CompletedTask;
        }

        [Command("1024says")]
        public Task TenTwentyFourSaysAsync([Remainder]string input) {

            using (MemoryStream stream = new MemoryStream(100))
            using (MagickImage image = new MagickImage(Cache.GetStream("1024says.png"))) {

                string text = string.Join(Environment.NewLine, input.SplitIntoChunks(25));
                int max = (text.Length <= 200 ? text.Length : 200);
                text = text.Substring(0, max);

                new Drawables()
                  .FontPointSize(124)
                  .Font(Path.Combine(Constants.FontsDirectory, "VT323-Regular.ttf"))
                  .Rotation(3.5)
                  .SkewX(5.0)
                  .FillColor(new MagickColor("#FFFFFF"))
                  .TextAlignment(TextAlignment.Left)
                  .TextAntialias(true)
                  .TextEncoding(Encoding.UTF8)
                  .Text(765, 400, text)
                  .Draw(image);

                image.Write(stream, MagickFormat.Png);

                stream.Seek(0, SeekOrigin.Begin);

                ulong reply = Context.Message.Channel.SendFileAsync(stream, "1024Says.png", text: $"● {RequestedBy}").Result.Id;
                if (!Context.IsPrivate) {
                    UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
                    StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
                }
            }

            return Task.CompletedTask;
        }

        [Command("amandasays")]
        public Task AmandaSaysAsync([Remainder]string input) {

            using (MemoryStream stream = new MemoryStream(100))
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

                ulong reply = Context.Message.Channel.SendFileAsync(stream, "amandaSays.png", text: $"● {RequestedBy}").Result.Id;
                if (!Context.IsPrivate) {
                    UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
                    StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
                }
            }

            return Task.CompletedTask;
        }

        [Command("satasays")]
        public Task SataSaysAsync([Remainder]string input) {

            using (MemoryStream stream = new MemoryStream(100))
            using (MagickImage image = new MagickImage(Cache.GetStream("satasays.png"))) {

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

                ulong reply = Context.Message.Channel.SendFileAsync(stream, "sataSays.png", text: $"● {RequestedBy}").Result.Id;
                if (!Context.IsPrivate) {
                    UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
                    StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
                }
            }

            return Task.CompletedTask;
        }

        [Command("jibersays")]
        public Task JiberSaysAsync([Remainder]string input) {

            using (MemoryStream stream = new MemoryStream(100))
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

                ulong reply = Context.Message.Channel.SendFileAsync(stream, "jiberSays.png", text: $"● {RequestedBy}").Result.Id;
                if (!Context.IsPrivate) {
                    UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
                    StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
                }
            }

            return Task.CompletedTask;
        }

    }

}
