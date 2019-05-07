using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Entities.Discord.MessageTag;
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

            await Context.Channel.TriggerTypingAsync();

            Moderator result = GetUserInfractions(Context.Guild.Id, user.Id);

            if (result is Moderator) {
                StringBuilder builder = new StringBuilder();

                foreach (Infraction infraction in result.Infractions) {

                    List<MessageTag> tags = infraction.Reason.ParseDiscordMessage();
                    string reason = infraction.Reason;

                    foreach (MessageTag tag in tags.Where(x => x.TagType == TagType.Channel)) {
                        SocketGuildChannel channel = Context.Guild.GetChannel(tag.Id);

                        if (channel is SocketGuildChannel) {
                            reason = reason.Replace(tag.ToString(), $"#{channel.Name}");
                        }
                    }

                    if (ulong.TryParse(infraction.AuthorId, out ulong authorId)) {
                        SocketUser authorUser = UserFromUserId(authorId);
                        builder.AppendLine($"{infraction.CreatedAtDateTime.ToString(Constants.DateTimeFormatMedium).ToLower()} utc ({infraction.Id}) {(authorUser == null ? "Unknown Moderator" : authorUser.ToString())} ● {reason}");
                    } else {
                        builder.AppendLine($"{infraction.CreatedAtDateTime.ToString(Constants.DateTimeFormatMedium).ToLower()} utc ({infraction.Id}) Unknown Moderator ● {reason}");
                    }
                }

                List<string> chunks = builder.ToString().SplitIntoChunksPreserveNewLines(1930);

                await Task.Delay(500);

                foreach (string chunk in chunks) {
                    await BetterReplyAsync($"● Infractions for {BetterUserFormat(user)} ```{chunk}```", user.Id.ToString());
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

        [Command("deleteallinfractions")]
        [DiscordStaffOnly]
        public async Task DeleteAllInfractions(SocketUser user = null) {
            if (user == null) {
                await BetterReplyAsync("You must mention a user to clear all their infractions.");
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            Moderator result = GetUserInfractions(Context.Guild.Id, user.Id);

            int deleteCount = 0;
            foreach (Infraction infraction in result.Infractions) {

                if (DeleteInfraction(Context.Guild.Id, infraction.Id)) {
                    deleteCount++;
                }
            }

            await BetterReplyAsync($"● **{deleteCount}** infractions for {BetterUserFormat(user)} were deleted.", user.Id.ToString());
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
