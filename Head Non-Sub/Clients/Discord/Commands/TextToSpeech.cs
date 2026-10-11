using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Entities.Streamlabs;
using HeadNonSub.Extensions;
using Humanizer;

namespace HeadNonSub.Clients.Discord.Commands {

    [BlacklistEnforced]
    [SubscriberOnly]
    [CommandContextType(InteractionContextType.Guild)]
    public class TextToSpeech : BetterModuleBase {

        public enum Voice {
            Joanna,
            Justin,
            Brian,
            Mizuki,
            Russell
        }

        [SlashCommand("tts", "Turn text into speech")]
        public async Task Tts([Summary("text", "What to say, 550 characters or less")] string text, [Summary("voice", "Which voice to use")] Voice voice = Voice.Joanna) {
            string command = $"tts_{voice}";
            string clean = text.RemoveNewLines();

            if (string.IsNullOrWhiteSpace(clean)) {
                await EphemeralReplyAsync("You need to enter text... for _text_ to speech to work you idiot.");
                return;
            }

            if (clean.Length > 550) {
                await EphemeralReplyAsync("The text must be 550 or less characters including spaces.");
                return;
            }

            string cooldown = Cooldowns.Check((SocketGuildUser)Context.User, "tts", 30, true);
            if (cooldown != null) {
                await EphemeralReplyAsync(cooldown);
                return;
            }

            await DeferAsync();

            string filename = clean.Truncate(40).ToLower();
            if (filename.EndsWith('_')) { filename = filename.Remove(filename.Length - 1, 1); }

            using MemoryStream oggFile = await GenerateAsync(clean, voice.ToString());

            if (oggFile is MemoryStream) {
                await BetterSendFileAsync(oggFile, $"{filename}.ogg", $"● {BetterUserFormat()}{Environment.NewLine}```{clean.Replace("`", "")}```", parameters: clean, command: command);
            } else {
                await BetterReplyAsync("Failed to generate the text to speech.", parameters: clean, command: command);
            }
        }

        private static async Task<MemoryStream> GenerateAsync(string text, string voice) {
            Task<string> pollyRequest = Http.SendRequestAsync("https://streamlabs.com/polly/speak",
                parameters: new Dictionary<string, string> { { "text", text }, { "voice", voice } }, method: Http.Method.Post);

            string pollyRequestData = await pollyRequest;

            if (pollyRequest.IsCompletedSuccessfully) {
                Polly polly = Http.DeserializeJson<Polly>(pollyRequestData);

                if (polly is Polly && polly.Success) {
                    Task<MemoryStream> pollyStream = Http.GetStreamAsync(polly.SpeakUrl);
                    MemoryStream pollyStreamData = await pollyStream;

                    if (pollyStream.IsCompletedSuccessfully) {
                        return pollyStreamData;
                    } else {
                        LoggingManager.Log.Error(pollyStream.Exception);
                    }
                }
            } else {
                LoggingManager.Log.Error(pollyRequest.Exception);
            }

            return null;
        }

    }

}
