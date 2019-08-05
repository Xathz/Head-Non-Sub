using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using HeadNonSub.Entities.TwitchStocks;
using Newtonsoft.Json;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [RequireContext(ContextType.Guild)]
    public class Stock : BetterModuleBase {

        private readonly DateTime _Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        [Command("stock")]
        public async Task PaymoneyWubbyStock() {
            await Context.Channel.TriggerTypingAsync();

            bool fromCache = false;
            KeyValuePair<long, double>? recent;

            if (Cache.Get("stock:paymoneywubby") is KeyValuePair<long, double> validStock) {
                fromCache = true;
                recent = validStock;
            } else {
                recent = await GetRecentStockValueAsync();

                if (recent is null) {
                    await BetterReplyAsync("Failed to retrieve price data from Streamlabs.");
                    return;
                }

                Cache.Add("stock:paymoneywubby", recent);
            }

            EmbedBuilder builder = new EmbedBuilder() {
                Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                Title = $"PaymoneyWubby (PYMNY) Stock Value",
                ThumbnailUrl = WubbysFunHouse.WubbyMoneyEmoteUrl,
                Description = $"[More data on TwitchStocks]({WubbysFunHouse.TwitchStocksUrl})"
            };

            builder.AddField("Value", $"**${recent.Value.Value.ToString("N2")}** _${recent.Value.Value.ToString("N6")}_");

            builder.Footer = new EmbedFooterBuilder() {
                Text = $"As of {FromUnixTime(recent.Value.Key).ToString().ToLower()} utc{(fromCache ? "; from cache" : "")}"
            };

            await BetterReplyAsync(builder.Build());
        }

        private async Task<KeyValuePair<long, double>?> GetRecentStockValueAsync() {
            Dictionary<long, double> stocks = await GetStocksAsync();

            if (stocks.Count == 0) {
                return null;
            }

            return stocks.OrderByDescending(x => x.Key).FirstOrDefault();
        }

        private async Task<Dictionary<long, double>> GetStocksAsync() {
            Dictionary<long, double> returnValues = new Dictionary<long, double>();

            Task<string> download = Http.SendRequestAsync($"https://api.twitchstocks.com/api/v1/stocks/38251312/history/1hr",
                new Dictionary<string, string> { { "Referer", "https://twitchstocks.com" } });

            string data = await download;

            if (download.IsCompletedSuccessfully) {
                Values values = new Values();

                using (StringReader jsonReader = new StringReader(data)) {
                    JsonSerializer jsonSerializer = new JsonSerializer {
                        DateTimeZoneHandling = DateTimeZoneHandling.Utc
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
            } else {
                LoggingManager.Log.Error(download.Exception);
            }

            return returnValues;
        }

        private DateTime FromUnixTime(long unixTime) => _Epoch.AddMilliseconds(unixTime);

    }

}
