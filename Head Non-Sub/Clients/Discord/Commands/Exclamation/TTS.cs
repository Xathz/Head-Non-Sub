using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Discord.Commands;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Entities.Streamlabs;
using HeadNonSub.Extensions;
using Newtonsoft.Json;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [BlacklistEnforced]
    [RequireContext(ContextType.Guild)]
    public class TTS : BetterModuleBase {

        [Command("tts")]
        [Cooldown(60)]
        public Task Joanna([Remainder]string input) {
            GenerateAndSend(input, "tts", "Joanna");
            return Task.CompletedTask;
        }

        [Command("tts2")]
        [Cooldown(60)]
        public Task Justin([Remainder]string input) {
            GenerateAndSend(input, "tts2", "Justin");
            return Task.CompletedTask;
        }

        [Command("tts3")]
        [Cooldown(60)]
        public Task Brian([Remainder]string input) {
            GenerateAndSend(input, "tts3", "Brian");
            return Task.CompletedTask;
        }

        [Command("tts4")]
        [Cooldown(60)]
        public Task Mizuki([Remainder]string input) {
            GenerateAndSend(input, "tts4", "Mizuki");
            return Task.CompletedTask;
        }

        private void GenerateAndSend(string text, string command, string voice) {
            string clean = text.RemoveNewLines();

            if (string.IsNullOrWhiteSpace(clean)) {
                _ = BetterReplyAsync("You need to enter text... for _text_ to speech to work you idiot.");
                return;
            }

            if (clean.Length > 550) {
                _ = BetterReplyAsync("The text must be 550 or less characters including spaces.");
                return;
            }

            Context.Message.DeleteAsync();

            string filename = clean.Truncate(40).ToLower();
            if (filename.EndsWith("_")) { filename = filename.Remove(filename.Length - 1, 1); }
            Stream oggFile = Generate(clean, voice);

            if (oggFile is Stream) {
                BetterSendFileAsync(oggFile, $"{filename}.ogg", $"● {BetterUserFormat()}{Environment.NewLine}```{clean}```", clean, $"{command}_{voice}").Wait();
            } else {
                _ = BetterReplyAsync("Failed to generate the text to speech.");
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
