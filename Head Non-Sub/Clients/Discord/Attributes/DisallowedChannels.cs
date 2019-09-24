using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;

namespace HeadNonSub.Clients.Discord.Attributes {

    /// <summary>
    /// Disallow a command in a channel or list of channels.
    /// </summary>
    public class DisallowedChannels : PreconditionAttribute {

        private readonly List<ulong> _DisallowedChannels = new List<ulong>();

        public DisallowedChannels(params ulong[] disallowedChannels) => _DisallowedChannels.AddRange(disallowedChannels);

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services) {
            if (_DisallowedChannels.Contains(context.Channel.Id)) {
                return Task.FromResult(PreconditionResult.FromError("Invalid channel for this command."));
            } else {
                return Task.FromResult(PreconditionResult.FromSuccess());
            }
        }

    }

}
