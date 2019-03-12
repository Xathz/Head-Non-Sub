using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Entities.TwitchStocks;
using HeadNonSub.Statistics;
using Newtonsoft.Json;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    // Wubby's Fun House
    [BlacklistEnforced, AllowedGuilds(328300333010911242)]
    [RequireContext(ContextType.Guild)]
    public class Stock : ModuleBase<SocketCommandContext> {

        private readonly DateTime _Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        [Command("stock")]
        public Task StockAsync() {
            bool fromCache = false;
            KeyValuePair<long, double> recent;

            if (Cache.Get("stock:paymoneywubby") is KeyValuePair<long, double> validStock) {
                fromCache = true;
                recent = validStock;
            } else {
                recent = GetRecentStockValue();
                Cache.Add("stock:paymoneywubby", recent);
            }

            EmbedBuilder builder = new EmbedBuilder() {
                Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                Title = $"PaymoneyWubby (PYMNY) Stock Value",
                ThumbnailUrl = "https://cdn.discordapp.com/emojis/522586653114499072.png",
                Description = "[More data on TwitchStocks](https://twitchstocks.com/stock/pymny)"
            };

            builder.AddField("Value", $"**${recent.Value.ToString("N2")}** _${recent.Value.ToString("N6")}_");

            builder.Footer = new EmbedFooterBuilder() {
                Text = $"As of {FromUnixTime(recent.Key).ToString().ToLower()} utc{(fromCache ? "; from cache" : "")}"
            };

            ulong reply = ReplyAsync(embed: builder.Build()).Result.Id;

            UndoTracker.Track(Context.Guild.Id, Context.Channel.Id, Context.User.Id, Context.Message.Id, reply);
            StatisticsManager.Statistics.Commands(Context.Guild.Id).Executed();
            return Task.CompletedTask;
        }

        private KeyValuePair<long, double> GetRecentStockValue() {
            Dictionary<long, double> stocks = GetStocks();
            return stocks.OrderByDescending(x => x.Key).FirstOrDefault();
        }

        private Dictionary<long, double> GetStocks() {
            Dictionary<long, double> returnValues = new Dictionary<long, double>();
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

                returnValues.Add(timestamp.Value, value.Value);
            }

            return returnValues;
        }

        private DateTime FromUnixTime(long unixTime) => _Epoch.AddMilliseconds(unixTime);

    }

}
