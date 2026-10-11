using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;

namespace HeadNonSub.Clients.Discord.Attributes {

    /// <summary>
    /// Restrict a command to Xathz.
    /// </summary>
    public sealed class XathzOnly : PreconditionAttribute {

        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services) {
            if (context.User.Id == Constants.XathzUserId) {
                return Task.FromResult(PreconditionResult.FromSuccess());
            }

            return Task.FromResult(PreconditionResult.FromError("You are not allowed to run this command."));
        }

    }

}
