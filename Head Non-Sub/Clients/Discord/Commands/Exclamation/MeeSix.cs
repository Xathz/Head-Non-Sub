using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Entities.Discord;
using HeadNonSub.Entities.MeeSix.Moderator;
using HeadNonSub.Extensions;
using HeadNonSub.Settings;
using Newtonsoft.Json;
using TagType = HeadNonSub.Entities.Discord.TagType;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [BlacklistEnforced]
    [RequireContext(ContextType.Guild)]
    public class MeeSix : BetterModuleBase {

        [Command("infractions")]
        [Cooldown(28)]
        public async Task Infractions(SocketUser user = null) {
            if (user == null) { return; }

            await Context.Channel.TriggerTypingAsync();

            Moderator result = await GetUserInfractionsAsync(Context.Guild.Id, user.Id);

            if (result is Moderator) {
                StringBuilder builder = new StringBuilder();

                foreach (Infraction infraction in result.Infractions) {

                    List<MessageTag> tags = infraction.Reason.ParseDiscordMessageTags();
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

        [Command("warn")]
        public async Task Warn(SocketUser user = null, [Remainder]string reason = "") {
            if (user == null) { return; }

            if (Context.User is SocketGuildUser contextUser) {
                if (contextUser.Roles.Any(x => x.Permissions.Administrator) || contextUser.Roles.Any(x => WubbysFunHouse.DiscordStaffRoles.Contains(x.Id))) {

                    if (Context.Channel is SocketTextChannel channel) {
                        IAsyncEnumerable<IMessage> messages = channel.GetMessagesAsync(200).Flatten();
                        IEnumerable<IMessage> foundMessages = messages.Where(x => x.Author.Id == user.Id).OrderByDescending(x => x.CreatedAt).Take(10).ToEnumerable();

                        StringBuilder builder = new StringBuilder();
                        foreach (IMessage message in foundMessages) {

                            string header = "";
                            if (message.Attachments.Count > 0) {
                                header = "<There is a file attachment with this message> ";
                            }

                            string content = "";
                            if (!string.IsNullOrWhiteSpace(message.Content)) {
                                content = message.Content;
                            }

                            builder.AppendLine($"{message.CreatedAt.ToString(Constants.DateTimeFormatMedium).ToLower()} utc: {header}{content}");
                        }

                        await Task.Delay(750);

                        await LogMessageAsync($"● Recent messages from {BetterUserFormat(user)}", builder.ToString());

                    } else {
                        await BetterReplyAsync($"Failed to retrieve context around the warn for {BetterUserFormat(user)}.");
                    }
                }
            } else {
                await BetterReplyAsync($"Failed to retrieve context around the warn for {BetterUserFormat(user)}.");
            }
        }

        [Command("deleteinfraction")]
        [DiscordStaffOnly]
        public async Task DeleteInfraction(string id = "") {
            if (string.IsNullOrWhiteSpace(id)) {
                await BetterReplyAsync("You need to supply a infraction id. To get a list of id's use `!infractions @User` as normal.");
                return;
            }

            bool result = await DeleteInfractionAsync(Context.Guild.Id, id);

            if (result) {
                await BetterReplyAsync("The infraction was deleted.", id);
                await LogMessageEmbedAsync("MEE6 infraction deleted", $"Id: {id}");
            } else {
                await BetterReplyAsync("The infraction was **not** deleted.", id);
                await LogMessageEmbedAsync("Failed MEE6 infraction deletion", $"Attempted id: {id}");
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

            Moderator result = await GetUserInfractionsAsync(Context.Guild.Id, user.Id);

            int deleteCount = 0;
            foreach (Infraction infraction in result.Infractions) {

                if (await DeleteInfractionAsync(Context.Guild.Id, infraction.Id)) {
                    deleteCount++;
                }
            }

            await BetterReplyAsync($"● **{deleteCount}** infractions for {BetterUserFormat(user)} were deleted.", user.Id.ToString());
            await LogMessageEmbedAsync("All MEE6 infractions deleted", user: user);
        }

        private async Task<Moderator> GetUserInfractionsAsync(ulong serverId, ulong userId) {
            try {
                using (HttpClient client = new HttpClient()) {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://mee6.xyz/api/plugins/moderator/guilds/{serverId}/infractions?page=0&limit=1000&user_id={userId}");
                    request.Headers.TryAddWithoutValidation("Authorization", SettingsManager.Configuration.MeeSixToken);

                    using (HttpResponseMessage response = await client.SendAsync(request)) {

                        if (response.IsSuccessStatusCode) {
                            using (HttpContent content = response.Content) {
                                string json = await content.ReadAsStringAsync();

                                using (StringReader jsonReader = new StringReader(json)) {
                                    JsonSerializer jsonSerializer = new JsonSerializer {
                                        DateTimeZoneHandling = DateTimeZoneHandling.Utc
                                    };

                                    return jsonSerializer.Deserialize(jsonReader, typeof(Moderator)) as Moderator;
                                }
                            }
                        } else {
                            throw new HttpRequestException($"{response.StatusCode}; {response.ReasonPhrase}");
                        }
                    }
                }
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
                return null;
            }
        }

        private async Task<bool> DeleteInfractionAsync(ulong serverId, string infractionId) {
            try {
                using (HttpClient client = new HttpClient()) {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, $"https://mee6.xyz/api/plugins/moderator/guilds/{serverId}/infractions/{infractionId}");
                    request.Headers.TryAddWithoutValidation("Authorization", SettingsManager.Configuration.MeeSixToken);

                    using (HttpResponseMessage response = await client.SendAsync(request)) {
                        return response.IsSuccessStatusCode;
                    }
                }
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
                return false;
            }
        }

    }

}
