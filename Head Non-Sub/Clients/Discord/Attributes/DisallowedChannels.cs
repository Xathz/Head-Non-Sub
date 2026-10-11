using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;

namespace HeadNonSub.Clients.Discord.Attributes {

    /// <summary>
    /// Disallow a command in a channel or list of channels.
    /// </summary>
    public sealed class DisallowedChannels : PreconditionAttribute {

        private readonly List<ulong> _DisallowedChannels = new List<ulong>();

        public DisallowedChannels(params ulong[] disallowedChannels) => _DisallowedChannels.AddRange(disallowedChannels);

        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services) {
            if (_DisallowedChannels.Contains(context.Channel.Id)) {
                return Task.FromResult(PreconditionResult.FromError("This command can not be used in this channel."));
            } else {
                return Task.FromResult(PreconditionResult.FromSuccess());
            }
        }

    }

}
