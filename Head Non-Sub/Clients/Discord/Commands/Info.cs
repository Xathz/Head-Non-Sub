using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Extensions;

namespace HeadNonSub.Clients.Discord.Commands {

    [BlacklistEnforced]
    [CommandContextType(InteractionContextType.Guild)]
    public class Info : BetterModuleBase {

        [SlashCommand("help", "Bot information and commands")]
        public async Task Help() {
            EmbedBuilder builder = new EmbedBuilder() {
                Color = new Color(Constants.GeneralColor.R, Constants.GeneralColor.G, Constants.GeneralColor.B),
                Title = $"{Context.Client.CurrentUser.Username} Information and Commands"
            };

            builder.AddField("Commands", $"Type `/` to see the commands, or [click here for the full list]({Constants.CommandsHelpUrl}).");

            builder.Footer = new EmbedFooterBuilder() {
                Text = $"{Constants.ApplicationName} by {Constants.Creator}"
            };

            await BetterReplyAsync(builder.Build());
        }

        [SlashCommand("ping", "Check the bot's response time")]
        public async Task Ping() {
            DateTime now = DateTime.UtcNow;
            DateTime created = Context.Interaction.CreatedAt.UtcDateTime;

            await BetterReplyAsync($"{now.Subtract(created).TotalMilliseconds:N0}ms (gateway {Context.Client.Latency:N0}ms)" +
                $"```Ping: {created.ToString(Constants.DateTimeFormatFull)}{Environment.NewLine}Pong: {now.ToString(Constants.DateTimeFormatFull)}```");
        }

        [SlashCommand("prices", "Media share prices and info")]
        public async Task Prices() => await BetterReplyAsync("https://tangia.co/paymoneywubby?media_share <https://www.reddit.com/r/PaymoneyWubby/comments/1cl57nv/new_media_share_info_if_you_are_submitting/>");

        [SlashCommand("emotes", "List the server emotes and who can use them")]
        [DefaultMemberPermissions(GuildPermission.ManageMessages)]
        [DiscordStaffOnly]
        public async Task Emotes() {
            StringBuilder builder = new StringBuilder();

            foreach (GuildEmote emote in Context.Guild.Emotes) {
                List<string> allowedRoleNames = new List<string>();
                foreach (ulong id in emote.RoleIds) {
                    allowedRoleNames.Add(Context.Guild.Roles.FirstOrDefault(x => x.Id == id)?.Name ?? id.ToString());
                }

                string allowedRoles = allowedRoleNames.Count > 0 ? string.Join(", ", allowedRoleNames) : "Everyone";

                List<string> flagsList = new List<string>();
                if (emote.Animated) { flagsList.Add("Animated"); }
                if (emote.IsManaged) { flagsList.Add("Managed"); }

                builder.AppendLine($"{emote.Name} ({emote.Id}) {emote.CreatedAt.DateTime.ToString(Constants.DateTimeFormatShort).ToLower()} utc ● {(flagsList.Count > 0 ? $"{string.Join(", ", flagsList)} ● " : "")}{allowedRoles}");
            }

            List<string> chunks = builder.ToString().SplitIntoChunksPreserveNewLines(1950);

            if (chunks.Count == 0) {
                await BetterReplyAsync("There are no emotes.");
                return;
            }

            foreach (string chunk in chunks) {
                await BetterReplyAsync($"```{chunk}```");
            }
        }

    }

}
