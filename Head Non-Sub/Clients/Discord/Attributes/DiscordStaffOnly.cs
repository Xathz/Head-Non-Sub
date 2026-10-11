using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;

namespace HeadNonSub.Clients.Discord.Attributes {

    /// <summary>
    /// Restrict a command to Discord staff. Pair with <see cref="DefaultMemberPermissionsAttribute"/> so non-staff don't see it.
    /// </summary>
    public sealed class DiscordStaffOnly : PreconditionAttribute {

        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services) {
            if (WubbysFunHouse.IsDiscordStaff(context.User)) {
                return Task.FromResult(PreconditionResult.FromSuccess());
            }

            return Task.FromResult(PreconditionResult.FromError("This is a Discord staff only command."));
        }

    }

}
