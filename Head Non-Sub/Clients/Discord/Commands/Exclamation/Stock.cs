using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [RequireContext(ContextType.Guild)]
    public class Stock : ModuleBase<SocketCommandContext> {

        private readonly DateTime _Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private Dictionary<long, double> _Values = new Dictionary<long, double>();

        [Command("stock")]
        public Task StockAsync() {
            GetStocks();
            KeyValuePair<long, double> recent = _Values.OrderByDescending(x => x.Key).FirstOrDefault();

            EmbedBuilder builder = new EmbedBuilder() {
                Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                Title = $"PaymoneyWubby (PYMNY) Stock Value",
                ThumbnailUrl = "https://cdn.discordapp.com/emojis/522586653114499072.png",
                Description = "[More data on TwitchStocks](https://twitchstocks.com/stock/pymny)"
            };

            builder.AddField("Value", $"**${recent.Value.ToString("N2")}** _${recent.Value.ToString("N6")}_");

            builder.Footer = new EmbedFooterBuilder() {
                Text = $"As of {FromUnixTime(recent.Key)} utc"
            };

            ulong reply = ReplyAsync(embed: builder.Build()).Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            return Task.CompletedTask;
        }

        private void GetStocks() {
            Values values = new Values();
            WebClient webClient = new WebClient();
            webClient.Headers.Add(HttpRequestHeader.Referer, "https://twitchstocks.com");
            string json = webClient.DownloadString("https://api.twitchstocks.com/api/v1/stocks/38251312/history/1hr");

            using (StringReader jsonReader = new StringReader(json)) {
                JsonSerializer jsonSerializer = new JsonSerializer {
                    DateTimeZoneHandling = DateTimeZoneHandling.Local
                };

                values = jsonSerializer.Deserialize(jsonReader, typeof(Values)) as Values;
            }

            foreach (List<double> pair in values.Data) {
                double? value = null;
                long? timestamp = null;

                foreach (double item in pair) {
                    if (!value.HasValue) {
                        value = item;
                        continue;
                    }

                    if (!timestamp.HasValue) {
                        timestamp = Convert.ToInt64(item);
                    }
                }

                _Values.Add(timestamp.Value, value.Value);
            }

        }

        private DateTime FromUnixTime(long unixTime) => _Epoch.AddMilliseconds(unixTime);

        private class Values {

            [JsonProperty("data")]
            public List<List<double>> Data { get; set; }

        }

    }

}
