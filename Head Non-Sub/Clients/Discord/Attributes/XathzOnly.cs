using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace HeadNonSub.Clients.Discord.Attributes {

    public sealed class XathzOnly : PreconditionAttribute {

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services) {
            if (context.User is SocketGuildUser user) {

                if (user.Id == Constants.XathzUserId) {
                    return Task.FromResult(PreconditionResult.FromSuccess());
                }

                return Task.FromResult(PreconditionResult.FromError($"{user.Mention} you are not allowed to run this command."));
            }

            return Task.FromResult(PreconditionResult.FromError($"{context.User.Mention} you must be in a guild to run this command."));
        }

    }

}
