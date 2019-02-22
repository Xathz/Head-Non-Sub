using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;

namespace HeadNonSub.Clients.Discord.Attributes {

    public class AllowedGuilds : PreconditionAttribute {

        private readonly List<ulong> _AllowedGuilds = new List<ulong>();

        /// <summary>
        /// Restrict command to only these guilds (servers).
        /// </summary>
        public AllowedGuilds(params ulong[] allowedGuilds) => _AllowedGuilds.AddRange(allowedGuilds);

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services) {
            if (_AllowedGuilds.Contains(context.Guild.Id)) {
                return Task.FromResult(PreconditionResult.FromSuccess());
            } else {
                return Task.FromResult(PreconditionResult.FromError("Not a valid guild (server) for this command."));
            }
        }

    }

}
