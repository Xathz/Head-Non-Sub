using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;

namespace HeadNonSub.Clients.Discord.Attributes {

    public class AllowedChannels : PreconditionAttribute {

        private readonly List<ulong> _AllowedChannels = new List<ulong>();

        /// <summary>
        /// Restrict command to only these channels.
        /// </summary>
        public AllowedChannels(params ulong[] allowedChannels) => _AllowedChannels.AddRange(allowedChannels);

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services) {
            if (_AllowedChannels.Contains(context.Channel.Id)) {
                return Task.FromResult(PreconditionResult.FromSuccess());
            } else {
                return Task.FromResult(PreconditionResult.FromError("Invalid channel for this command."));
            }
        }

    }

}
