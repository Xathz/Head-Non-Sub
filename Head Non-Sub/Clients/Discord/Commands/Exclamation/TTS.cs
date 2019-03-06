using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Entities.Streamlabs;
using HeadNonSub.Extensions;
using HeadNonSub.Statistics;
using Newtonsoft.Json;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [BlacklistEnforced]
    [RequireContext(ContextType.Guild)]
    public class TTS : ModuleBase<SocketCommandContext> {

        private string RequestedBy {
            get {
                if (Context.User is SocketGuildUser user) {
                    return $"{(!string.IsNullOrWhiteSpace(user.Nickname) ? user.Nickname : user.Username)} `{user.ToString()}`";
                } else {
                    return $"{Context.User.Username} `{Context.User.ToString()}`";
                }
            }
        }

        [Command("tts")]
        [Cooldown(60)]
        public Task JoannaAsync([Remainder]string input) {
            GenerateAndSend(input, "Joanna");
            return Task.CompletedTask;
        }

        [Command("tts2")]
        [Cooldown(60)]
        public Task JustinAsync([Remainder]string input) {
            GenerateAndSend(input, "Justin");
            return Task.CompletedTask;
        }

        [Command("tts3")]
        [Cooldown(60)]
        public Task BrianAsync([Remainder]string input) {
            GenerateAndSend(input, "Brian");
            return Task.CompletedTask;
        }

        [Command("tts4")]
        [Cooldown(60)]
        public Task MizukiAsync([Remainder]string input) {
            GenerateAndSend(input, "Mizuki");
            return Task.CompletedTask;
        }

        private void GenerateAndSend(string text, string voice) {
            string clean = text.RemoveNewLines();

            if (string.IsNullOrWhiteSpace(clean)) {
                ulong reply = ReplyAsync("You need to enter text... for _text_ to speech to work you idiot.").Result.Id;
                UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
                return;
            }

            if (clean.Length > 550) {
                ulong reply = ReplyAsync("The text must be 550 or less characters including spaces.").Result.Id;
                UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
                return;
            }

            Context.Message.DeleteAsync();

            string filename = clean.Truncate(40).ToLower();
            if (filename.EndsWith("_")) { filename = filename.Remove(filename.Length - 1, 1); }
            Stream oggFile = Generate(clean, voice);

            if (oggFile is Stream) {
                ulong reply = Context.Message.Channel.SendFileAsync(oggFile, $"{filename}.ogg", text: $"● {RequestedBy}{Environment.NewLine}```{clean}```").Result.Id;
                UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
                StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
                StatisticsManager.Statistics.Commands(Context.Guild.Id).TTSMessage(clean);
            } else {
                ulong reply = ReplyAsync("Failed to generate the text to speech.").Result.Id;
                UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
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
