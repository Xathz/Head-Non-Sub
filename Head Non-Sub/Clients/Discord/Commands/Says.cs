using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Extensions;
using ImageMagick;
using ImageMagick.Drawing;

namespace HeadNonSub.Clients.Discord.Commands {

    [BlacklistEnforced]
    [SubscriberOnly]
    [CommandContextType(InteractionContextType.Guild)]
    public class Says : BetterModuleBase {

        public enum Template {
            [ChoiceDisplay("tt2468")]
            TT,
            [ChoiceDisplay("1024")]
            TenTwentyFour,
            [ChoiceDisplay("Amanda")]
            Amanda,
            [ChoiceDisplay("Satazero")]
            Sata,
            [ChoiceDisplay("jiberjiber")]
            Jiber,
            [ChoiceDisplay("Strong Bad")]
            StrongBad
        }

        [SlashCommand("says", "Make someone say something")]
        public async Task SaysCommand([Summary("who", "Who should say it")] Template who, [Summary("text", "What they should say")] string text) {
            // Cooldown keys and statistics names match the old text commands
            (string cooldownKey, int cooldownSeconds, string statisticsName) = who switch {
                Template.TT => ("ttsays", 86400, "TTSays"),
                Template.TenTwentyFour => ("1024says", 1200, "TenTwentyFourSays"),
                Template.Amanda => ("amandasays", 1200, "AmandaSays"),
                Template.Sata => ("satasays", 1200, "SataSays"),
                Template.Jiber => ("jibersays", 1200, "JiberSays"),
                _ => ("sbsays", 1200, "StrongBadSays")
            };

            string cooldown = Cooldowns.Check((SocketGuildUser)Context.User, cooldownKey, cooldownSeconds, true);
            if (cooldown != null) {
                await EphemeralReplyAsync(cooldown);
                return;
            }

            await DeferAsync();

            if (who == Template.TenTwentyFour) {
                await BetterSendFileAsync(Cache.GetStream("1024notfound.gif"), "1024notfound.gif", $"● {BetterUserFormat()}", parameters: text, command: statisticsName);
                return;
            }

            using MemoryStream stream = who switch {
                Template.TT => DrawText(Random.Shared.Next(0, 100) >= 95 ? "ttsays.png" : "ttsays_old.png", null, text, 26, 150, "Sloppy-Hand.otf", 40, "#B0AEAD", 218, 45),
                Template.Amanda => DrawText("amandasays.png", null, text, 30, 220, "ConcertOne-Regular.ttf", 48, "#404040", 730, 260),
                Template.Sata => DrawText("satasays_ariana.png", null, text, 34, 230, "ConcertOne-Regular.ttf", 48, "#EFE1E1", 440, 62),
                Template.Jiber => DrawText("jibersays.png", null, text, 33, 245, "Courgette-Regular.ttf", 64, "#CFB491", 500, 290),
                _ => DrawText("strongbadsays.png", "strongbadsays_overlay.png", text, 47, 580, "VT323-Regular.ttf", 58, "#CECCCC", 290, 190)
            };

            await BetterSendFileAsync(stream, $"{statisticsName}.png", $"● {BetterUserFormat()}", parameters: text, command: statisticsName);
        }

        [SlashCommand("warm", "Warm someone up")]
        [Cooldown(300, true)]
        public async Task Warm([Summary("user", "Who to warm")] SocketGuildUser user, [Summary("reason", "Why they are being warmed")] string reason = "") {
            await DeferAsync();

            string avatarUrl = user.GetGuildAvatarUrl(ImageFormat.Png, 256) ?? user.GetDisplayAvatarUrl(ImageFormat.Png, 256);

            Task<MemoryStream> download = Http.GetStreamAsync(avatarUrl);
            using MemoryStream data = await download;

            if (!download.IsCompletedSuccessfully || data is null) {
                await BetterReplyAsync("Failed to get their avatar. You've just warmed yourself idiot.", parameters: $"{user} ({user.Id}); {reason}");
                return;
            }

            using MemoryStream stream = new MemoryStream(256);
            using MagickImage image = new MagickImage(Cache.GetStream("warm.png"));
            using MagickImage avatar = new MagickImage(data) {
                BackgroundColor = new MagickColor(0, 0, 0, 0)
            };

            avatar.Resize(225, 225);
            avatar.Rotate(-5.5);

            image.Composite(avatar, 160, 132, CompositeOperator.Atop);

            image.Resize(256, 256);
            image.Write(stream, MagickFormat.Png);

            stream.Seek(0, SeekOrigin.Begin);

            await BetterSendFileAsync(stream, "warm.png", $"{BetterUserFormat(user)} has been warmed{(string.IsNullOrWhiteSpace(reason) ? "" : $" for `{reason.Replace("`", "")}`")}.", parameters: $"{user} ({user.Id}); {reason}");
        }

        /// <summary>
        /// Draw text onto a cached template image.
        /// </summary>
        private static MemoryStream DrawText(string template, string overlay, string input, int lineLength, int maxLength, string font, int fontSize, string color, int x, int y) {
            MemoryStream stream = new MemoryStream(256);
            using MagickImage image = new MagickImage(Cache.GetStream(template));

            string text = string.Join(Environment.NewLine, input.SplitIntoChunks(lineLength));
            text = text.Substring(0, Math.Min(text.Length, maxLength));

            new Drawables()
              .FontPointSize(fontSize)
              .Font(Path.Combine(Constants.FontsDirectory, font))
              .FillColor(new MagickColor(color))
              .TextAlignment(TextAlignment.Left)
              .EnableTextAntialias()
              .TextEncoding(Encoding.UTF8)
              .Text(x, y, text)
              .Draw(image);

            if (overlay != null) {
                using MagickImage overlayImage = new MagickImage(Cache.GetStream(overlay));
                image.Composite(overlayImage, CompositeOperator.Atop);
            }

            image.Write(stream, MagickFormat.Png);
            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }

    }

}
