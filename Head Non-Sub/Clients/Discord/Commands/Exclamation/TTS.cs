using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Entities.Streamlabs;
using HeadNonSub.Extensions;
using Humanizer;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [BlacklistEnforced, SubscriberOnly]
    [RequireContext(ContextType.Guild)]
    public class TTS : BetterModuleBase {

        [Command("tts"), Alias("ttsdonate", "ttssub")]
        [Cooldown(30, true)]
        public async Task Joanna([Remainder]string input) => await GenerateAndSend(input, "tts", "Joanna");

        [Command("tts2")]
        [Cooldown(30, true)]
        public async Task Justin([Remainder]string input) => await GenerateAndSend(input, "tts2", "Justin");

        [Command("tts3")]
        [Cooldown(30, true)]
        public async Task Brian([Remainder]string input) => await GenerateAndSend(input, "tts3", "Brian");

        [Command("tts4")]
        [Cooldown(30, true)]
        public async Task Mizuki([Remainder]string input) => await GenerateAndSend(input, "tts4", "Mizuki");

        [Command("tts5"), Alias("ttsbits")]
        [Cooldown(30, true)]
        public async Task Russell([Remainder]string input) => await GenerateAndSend(input, "tts4", "Russell");

        private async Task GenerateAndSend(string text, string command, string voice) {
            await Context.Channel.TriggerTypingAsync();

            string clean = text.RemoveNewLines();

            if (string.IsNullOrWhiteSpace(clean)) {
                await BetterReplyAsync("You need to enter text... for _text_ to speech to work you idiot.", parameters: clean, command: $"{command}_{voice}");
                return;
            }

            if (clean.Length > 550) {
                await BetterReplyAsync("The text must be 550 or less characters including spaces.", parameters: clean, command: $"{command}_{voice}");
                return;
            }

            await Context.Message.DeleteAsync();

            string filename = clean.Truncate(40).ToLower();
            if (filename.EndsWith("_")) { filename = filename.Remove(filename.Length - 1, 1); }

            using MemoryStream oggFile = await GenerateAsync(clean, voice);
            if (oggFile is MemoryStream) {
                await BetterSendFileAsync(oggFile, $"{filename}.ogg", $"● {BetterUserFormat()}{Environment.NewLine}```{clean}```", parameters: clean, command: $"{command}_{voice}");
            } else {
                await BetterReplyAsync("Failed to generate the text to speech.", parameters: clean, command: $"{command}_{voice}");
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
