using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.Webhook;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [BlacklistEnforced]
    [RequireContext(ContextType.Guild)]
    [SubscriberOnly]
    public class Webhooks : BetterModuleBase {

        [Command("puppet")]
        [Cooldown(60, true)]
        public async Task Puppet(SocketUser user = null, [Remainder]string say = "") {
            if (user == null) {
                await BetterReplyAsync("You must mention a user to make them talk.", parameters: $"user null; {say}");
                return;
            }

            if (string.IsNullOrWhiteSpace(say)) {
                await BetterReplyAsync($"You didn't tell {BetterUserFormat(user)} what to say.", parameters: $"{user} ({user.Id}); {say}");
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            if (Context.Channel is SocketTextChannel channel) {
                Task<MemoryStream> download = Http.GetStreamAsync(user.GetAvatarUrl(ImageFormat.Png, 256));

                using MemoryStream avatar = await download;
                if (download.IsCompletedSuccessfully) {
                    RestWebhook webhook = await channel.CreateWebhookAsync(BetterUserFormat(user, true), avatar);

                    try {
                        using (DiscordWebhookClient webhookClient = new DiscordWebhookClient(webhook)) {
                            await webhookClient.SendMessageAsync(say);
                        }

                        await LogUserMessageAsync("Head-Non Sub Puppet", $"User {Context.User} ({Context.User.Id}) made {user} ({user.Id}) say: {say}");

                    } catch (Exception ex) {
                        LoggingManager.Log.Error(ex);
                    } finally {
                        await webhook.DeleteAsync();
                    }
                }
            }

        }

    }

}
