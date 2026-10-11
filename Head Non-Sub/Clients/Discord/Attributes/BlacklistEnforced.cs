using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using HeadNonSub.Settings;

namespace HeadNonSub.Clients.Discord.Attributes {

    /// <summary>
    /// Block users on the blacklist from using the command.
    /// </summary>
    public sealed class BlacklistEnforced : PreconditionAttribute {

        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services) {
            if (context.Guild is null) {
                return Task.FromResult(PreconditionResult.FromError("You must be in a server to run this command."));
            }

            if (SettingsManager.Configuration.DiscordBlacklist.Any(x => x.Key == context.Guild.Id & x.Value.Contains(context.User.Id))) {
                return Task.FromResult(PreconditionResult.FromError("You are blacklisted. You can not use any commands."));
            }

            return Task.FromResult(PreconditionResult.FromSuccess());
        }

    }

}
