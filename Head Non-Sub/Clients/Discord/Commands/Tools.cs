using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Extensions;

namespace HeadNonSub.Clients.Discord.Commands {

    [BlacklistEnforced]
    [RequireContext(ContextType.Guild)]
    public class Tools : BetterModuleBase {

        [Command("ping")]
        public Task Ping() {
            DateTime now = DateTime.Now.ToUniversalTime();

            return BetterReplyAsync($"{now.Subtract(Context.Message.CreatedAt.DateTime).TotalMilliseconds:N0}ms" +
                $"```Ping: {Context.Message.CreatedAt.DateTime.ToString(Constants.DateTimeFormatFull)}{Environment.NewLine}Pong: {now.ToString(Constants.DateTimeFormatFull)}```");
        }

        [Command("emotes")]
        [DiscordStaffOnly]
        public async Task Emotes() {
            IReadOnlyCollection<GuildEmote> emotes = Context.Guild.Emotes;
            StringBuilder builder = new StringBuilder();

            foreach (GuildEmote emote in emotes) {
                List<string> allowedRoleNames = new List<string>();
                foreach (ulong id in emote.RoleIds) {
                    allowedRoleNames.Add(Context.Guild.Roles.FirstOrDefault(x => x.Id == id).Name);
                }
                string allowedRoles = allowedRoleNames.Count > 0 ? string.Join(", ", allowedRoleNames) : "Everyone";

                List<string> flagsList = new List<string>();
                if (emote.Animated) { flagsList.Add("Animated"); }
                if (emote.IsManaged) { flagsList.Add("Managed"); }

                builder.AppendLine($"{emote.Name} ({emote.Id}) {emote.CreatedAt.DateTime.ToString(Constants.DateTimeFormatShort).ToLower()} utc ● {(flagsList.Count > 0 ? $"{string.Join(", ", flagsList)} ● " : "")}{allowedRoles}");
            }

            List<string> chunks = builder.ToString().SplitIntoChunksPreserveNewLines(1950);

            foreach (string chunk in chunks) {
                await BetterReplyAsync($"```{chunk}```");
            }
        }

    }

}
