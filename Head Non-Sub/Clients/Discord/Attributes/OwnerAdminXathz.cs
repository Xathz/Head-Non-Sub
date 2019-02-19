using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace HeadNonSub.Clients.Discord.Attributes {

    public class OwnerAdminXathz : PreconditionAttribute {

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services) {
            if (context.User is SocketGuildUser user) {
                IApplication application = context.Client.GetApplicationInfoAsync().Result;

                // Bot owner
                if (user.Id == application.Owner.Id) {
                    return Task.FromResult(PreconditionResult.FromSuccess());
                }

                // Administrator
                if (user.Roles.Any(x => x.Permissions.Administrator)) {
                    return Task.FromResult(PreconditionResult.FromSuccess());
                }

                // Xathz
                if (user.Id == 227088829079617536) {
                    return Task.FromResult(PreconditionResult.FromSuccess());
                }
                
                return Task.FromResult(PreconditionResult.FromError($"{user.Mention} you are not allowed to run this command."));
            }

            return Task.FromResult(PreconditionResult.FromError($"{context.User.Mention} you must be in a guild to run this command."));
        }

    }

}
