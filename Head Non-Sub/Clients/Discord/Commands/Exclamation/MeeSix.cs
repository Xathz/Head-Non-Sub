using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Entities.MeeSix.Moderator;
using HeadNonSub.Extensions;
using HeadNonSub.Settings;
using Newtonsoft.Json;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [BlacklistEnforced]
    [RequireContext(ContextType.Guild)]
    public class MeeSix : BetterModuleBase {

        [Command("infractions")]
        [Cooldown(28)]
        public async Task Infractions(SocketUser user = null) {
            if (user == null) { return; }

            Moderator result = GetUserInfractions(Context.Guild.Id, user.Id);

            if (result is Moderator) {
                StringBuilder builder = new StringBuilder();

                foreach (Infraction infraction in result.Infractions) {
                    if (ulong.TryParse(infraction.AuthorId, out ulong authorId)) {
                        SocketUser authorUser = UserFromUserId(authorId);
                        builder.AppendLine($"{infraction.CreatedAtDateTime.ToString(Constants.DateTimeFormatMedium).ToLower()} utc ({infraction.Id}) {(authorUser == null ? "Unknown Moderator" : authorUser.ToString())} ● {infraction.Reason}");
                    } else {
                        builder.AppendLine($"{infraction.CreatedAtDateTime.ToString(Constants.DateTimeFormatMedium).ToLower()} utc ({infraction.Id}) {infraction.Reason}");
                    }
                }

                List<string> chunks = builder.ToString().SplitIntoChunksPreserveNewLines(1950);

                await Task.Delay(500);

                foreach (string chunk in chunks) {
                    await BetterReplyAsync($"```{chunk}```", user.Id.ToString());
                }
            } else {
                await BetterReplyAsync($"Failed to retrieve infractions for {BetterUserFormat(user)}.");
            }
        }

        [Command("deleteinfraction")]
        [DiscordStaffOnly]
        public async Task DeleteInfraction(string id = "") {
            if (string.IsNullOrWhiteSpace(id)) {
                await BetterReplyAsync("You need to supply a infraction id. To get a list of id's use `!infractions @User` as normal.");
                return;
            }

            bool result = DeleteInfraction(Context.Guild.Id, id);

            if (result) {
                await BetterReplyAsync("The infraction was deleted.", id);
            } else {
                await BetterReplyAsync("The infraction was **not** deleted.", id);
            }
        }

        private Moderator GetUserInfractions(ulong serverId, ulong userId) {
            Moderator moderator = null;

            WebClient webClient = new WebClient();
            webClient.Headers.Add(HttpRequestHeader.Authorization, SettingsManager.Configuration.MeeSixToken);
            string json = webClient.DownloadString($"https://mee6.xyz/api/plugins/moderator/guilds/{serverId}/infractions?page=0&limit=1000&user_id={userId}");

            using (StringReader jsonReader = new StringReader(json)) {
                JsonSerializer jsonSerializer = new JsonSerializer {
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc
                };

                moderator = jsonSerializer.Deserialize(jsonReader, typeof(Moderator)) as Moderator;
            }

            return moderator;
        }

        private bool DeleteInfraction(ulong serverId, string infractionId) {
            using (HttpClient httpClient = new HttpClient()) {
                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Delete, $"https://mee6.xyz/api/plugins/moderator/guilds/{serverId}/infractions/{infractionId}");
                requestMessage.Headers.TryAddWithoutValidation("Authorization", SettingsManager.Configuration.MeeSixToken);

                return httpClient.SendAsync(requestMessage).Result.IsSuccessStatusCode;
            }
        }

    }

}
