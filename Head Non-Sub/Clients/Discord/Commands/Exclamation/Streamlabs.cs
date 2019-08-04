using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Humanizer;
using Newtonsoft.Json;
using StreamlabsEntities = HeadNonSub.Entities.Streamlabs.v6.Tip;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [RequireContext(ContextType.Guild)]
    public class Streamlabs : BetterModuleBase {

        [Command("prices"), Alias("mediashare")]
        public async Task Prices([Remainder]string input = "") {
            await Context.Channel.TriggerTypingAsync();

            bool fromCache = false;
            StreamlabsEntities.Tip streamlabsTip;

            if (Cache.Get("prices:paymoneywubby") is StreamlabsEntities.Tip validStreamlabsTip) {
                fromCache = true;
                streamlabsTip = validStreamlabsTip;
            } else {
                streamlabsTip = await GetTipDataAsync();

                if (streamlabsTip is null) {
                    await BetterReplyAsync("Failed to retrieve price data from Streamlabs.");
                    return;
                }

                Cache.Add("prices:paymoneywubby", streamlabsTip, 15);
            }

            EmbedBuilder builder = new EmbedBuilder() {
                Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                Title = $"Donation Media Prices",
                ThumbnailUrl = WubbysFunHouse.IconUrl,
                Url = "https://streamlabs.com/paymoneywubby"
            };

            bool minAmountToShareAttempt = double.TryParse(streamlabsTip.Settings.Media.MinAmountToShare, out double minAmountToShare);
            bool pricePerSecondeAttempt = double.TryParse(streamlabsTip.Settings.Media.PricePerSecond, out double pricePerSecond);
            _ = double.TryParse(streamlabsTip.Settings.Media.MaxDuration, out double maxDuration);

            bool mediaShareMinAmountToShareAttempt = double.TryParse(streamlabsTip.Settings.Media.AdvancedSettings.MinAmountToShare, out double mediaShareMinAmountToShare);
            bool mediaSharePricePerSecondAttempt = double.TryParse(streamlabsTip.Settings.Media.AdvancedSettings.PricePerSecond, out double mediaSharePricePerSecond);
            _ = double.TryParse(streamlabsTip.Settings.Media.AdvancedSettings.MaxDuration, out double mediaShareMaxDuration);

            builder.AddField("Regular Streams", $"Minimum amount to be shown on stream: **{(minAmountToShareAttempt ? minAmountToShare.ToString("C", CultureInfo.CurrentCulture) : streamlabsTip.Settings.Media.MinAmountToShare)}**{Environment.NewLine}" +
                    $"Price per second: **{(pricePerSecondeAttempt ? pricePerSecond.ToString("C", CultureInfo.CurrentCulture) : streamlabsTip.Settings.Media.PricePerSecond)}**{Environment.NewLine}" +
                    $"Maximum duration: **{TimeSpan.FromSeconds(maxDuration).Humanize(3)}**");

            builder.AddField("Media Share Streams", $"Minimum amount to be shown on stream: **{(mediaShareMinAmountToShareAttempt ? mediaShareMinAmountToShare.ToString("C", CultureInfo.CurrentCulture) : streamlabsTip.Settings.Media.AdvancedSettings.MinAmountToShare)}**{Environment.NewLine}" +
                    $"Price per second: **{(mediaSharePricePerSecondAttempt ? mediaSharePricePerSecond.ToString("C", CultureInfo.CurrentCulture) : streamlabsTip.Settings.Media.AdvancedSettings.PricePerSecond)}**{Environment.NewLine}" +
                    $"Maximum duration: **{TimeSpan.FromSeconds(mediaShareMaxDuration).Humanize(3)}**");

            //builder.AddField("Special Amounts", string.Join(Environment.NewLine, new string[] { $"These amounts will trigger different alerts on stream.",
            //            "420 Blazeit: **$4.20** / **$420.00**",
            //            "The devil: **$6.66** / **$666.00**",
            //            "The count: **$12.34** / **$1,234.00**",
            //            "Slurpee: **$7.11** / **$711.00**",
            //            "Heh 69: **$6.90** / **$69.00**",
            //            "Wake up wubby: **$50.00**" }));

            builder.Footer = new EmbedFooterBuilder() {
                Text = $"As of {streamlabsTip.Settings.CreatedAt.ToString().ToLower()} utc{(fromCache ? "; from cache" : "")}"
            };

            await BetterReplyAsync(builder.Build());
        }

        private async Task<StreamlabsEntities.Tip> GetTipDataAsync() {
            try {
                using (HttpResponseMessage response = await Http.Client.GetAsync("https://streamlabs.com/api/v6/1f510f07ca2978f/tip")) {
                    if (response.IsSuccessStatusCode) {
                        using (HttpContent content = response.Content) {
                            string json = await content.ReadAsStringAsync();

                            using (StringReader jsonReader = new StringReader(json)) {
                                JsonSerializer jsonSerializer = new JsonSerializer {
                                    DateTimeZoneHandling = DateTimeZoneHandling.Utc
                                };

                                return jsonSerializer.Deserialize(jsonReader, typeof(StreamlabsEntities.Tip)) as StreamlabsEntities.Tip;
                            }
                        }
                    } else {
                        throw new HttpRequestException($"{(int)response.StatusCode}; {response.ReasonPhrase}");
                    }
                }
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
                return null;
            }
        }

    }

}
