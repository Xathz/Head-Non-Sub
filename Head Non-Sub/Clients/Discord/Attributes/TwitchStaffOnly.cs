using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace HeadNonSub.Clients.Discord.Attributes {

    public class TwitchStaffOnly : PreconditionAttribute {

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services) {
            if (context.User is SocketGuildUser user) {

                // Xathz
                if (user.Id == Constants.XathzUserId) {
                    return Task.FromResult(PreconditionResult.FromSuccess());
                }

                // Administrator
                if (user.Roles.Any(x => x.Permissions.Administrator)) {
                    return Task.FromResult(PreconditionResult.FromSuccess());
                }

                // Discord and Twitch staff
                if (user.Roles.Any(x => WubbysFunHouse.TwitchStaffRoles.Contains(x.Id))) {
                    return Task.FromResult(PreconditionResult.FromSuccess());
                }

                return Task.FromResult(PreconditionResult.FromError($"{user.Mention} this is a Discord staff or Twitch mod only command."));
            }

            return Task.FromResult(PreconditionResult.FromError($"{context.User.Mention} you must be in a guild to run this command."));
        }

    }

}
