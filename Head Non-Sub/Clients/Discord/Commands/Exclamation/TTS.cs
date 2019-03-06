using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Discord.Commands;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Entities.Streamlabs;
using HeadNonSub.Extensions;
using HeadNonSub.Statistics;
using Newtonsoft.Json;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [BlacklistEnforced]
    [RequireContext(ContextType.Guild)]
    public class TTS : ModuleBase<SocketCommandContext> {

        [Command("tts")]
        [Cooldown(60)]
        public Task JoannaAsync([Remainder]string input) {
            string clean = input.RemoveNewLines();
            Stream oggFile = Generate(clean, "Joanna");

            if (oggFile is Stream) {
                ulong reply = Context.Message.Channel.SendFileAsync(oggFile, $"{clean.Truncate(35)}.ogg").Result.Id;
                UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
                StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
                StatisticsManager.Statistics.Commands(Context.Guild.Id).TTSMessage(clean);
            } else {
                ulong reply = ReplyAsync("Failed to generate the text to speech.").Result.Id;
                UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            }

            return Task.CompletedTask;
        }

        [Command("tts2")]
        [Cooldown(60)]
        public Task JustinAsync([Remainder]string input) {
            string clean = input.RemoveNewLines();
            Stream oggFile = Generate(clean, "Justin");

            if (oggFile is Stream) {
                ulong reply = Context.Message.Channel.SendFileAsync(oggFile, $"{clean.Truncate(35)}.ogg").Result.Id;
                UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
                StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
                StatisticsManager.Statistics.Commands(Context.Guild.Id).TTSMessage(clean);
            } else {
                ulong reply = ReplyAsync("Failed to generate the text to speech.").Result.Id;
                UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            }

            return Task.CompletedTask;
        }

        [Command("tts3")]
        [Cooldown(60)]
        public Task BrianAsync([Remainder]string input) {
            string clean = input.RemoveNewLines();
            Stream oggFile = Generate(clean, "Brian");

            if (oggFile is Stream) {
                ulong reply = Context.Message.Channel.SendFileAsync(oggFile, $"{clean.Truncate(35)}.ogg").Result.Id;
                UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
                StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
                StatisticsManager.Statistics.Commands(Context.Guild.Id).TTSMessage(clean);
            } else {
                ulong reply = ReplyAsync("Failed to generate the text to speech.").Result.Id;
                UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            }

            return Task.CompletedTask;
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
