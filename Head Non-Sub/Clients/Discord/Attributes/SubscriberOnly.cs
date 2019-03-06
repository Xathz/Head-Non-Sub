using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace HeadNonSub.Clients.Discord.Attributes {

    public class SubscriberOnly : PreconditionAttribute {

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services) {
            if (context.User is SocketGuildUser user) {

                // 'Non-sub' on 'Wubby's Fun House (328300333010911242)'
                if (user.Roles.Any(x => x.Id == 508752510216044547)) {              
                    return Task.FromResult(PreconditionResult.FromError($"{context.User.Mention} you must be subscribed! Sub <https://wub.by/subttv> or link your Twitch <https://wub.by/linkttv>."));
                } else {
                    return Task.FromResult(PreconditionResult.FromSuccess());
                }

            }

            return Task.FromResult(PreconditionResult.FromError($"{context.User.Mention} you must be in a guild to run this command."));
        }

    }

}
