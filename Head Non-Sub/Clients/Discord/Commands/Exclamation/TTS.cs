using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Discord.Commands;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Entities.Streamlabs;
using HeadNonSub.Extensions;
using Humanizer;
using Newtonsoft.Json;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [BlacklistEnforced, SubscriberOnly]
    [RequireContext(ContextType.Guild)]
    public class TTS : BetterModuleBase {

        [Command("tts")]
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

        private async Task GenerateAndSend(string text, string command, string voice) {
            await Context.Channel.TriggerTypingAsync();

            string clean = text.RemoveNewLines();

            if (string.IsNullOrWhiteSpace(clean)) {
                await BetterReplyAsync("You need to enter text... for _text_ to speech to work you idiot.");
                return;
            }

            if (clean.Length > 550) {
                await BetterReplyAsync("The text must be 550 or less characters including spaces.");
                return;
            }

            await Context.Message.DeleteAsync();

            string filename = clean.Truncate(40).ToLower();
            if (filename.EndsWith("_")) { filename = filename.Remove(filename.Length - 1, 1); }
            Stream oggFile = Generate(clean, voice);

            if (oggFile is Stream) {
                await BetterSendFileAsync(oggFile, $"{filename}.ogg", $"● {BetterUserFormat()}{Environment.NewLine}```{clean}```", clean, $"{command}_{voice}");
            } else {
                await BetterReplyAsync("Failed to generate the text to speech.");
            }
        }

        private Stream Generate(string text, string voice) {
            Dictionary<string, string> values = new Dictionary<string, string> { { "text", text }, { "voice", voice } };
            Polly polly = new Polly();

            HttpClient client = new HttpClient();
            FormUrlEncodedContent post = new FormUrlEncodedContent(values);
            HttpResponseMessage jsonResponse = client.PostAsync("https://streamlabs.com/polly/speak", post).Result;

            string json = jsonResponse.Content.ReadAsStringAsync().Result;

            using (StringReader jsonReader = new StringReader(json)) {
                JsonSerializer jsonSerializer = new JsonSerializer {
                    DateTimeZoneHandling = DateTimeZoneHandling.Local
                };

                polly = jsonSerializer.Deserialize(jsonReader, typeof(Polly)) as Polly;
            }

            if (polly is Polly & polly.Success) {
                HttpResponseMessage speakResponse = client.GetAsync(polly.SpeakUrl).Result;
                return speakResponse.Content.ReadAsStreamAsync().Result;
            }

            return null;
        }

    }

}
