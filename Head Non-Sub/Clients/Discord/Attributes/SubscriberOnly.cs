using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace HeadNonSub.Clients.Discord.Attributes {

    public sealed class SubscriberOnly : PreconditionAttribute {

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services) {
            if (context.User is SocketGuildUser user) {

                if (user.Roles.Any(x => x.Id == WubbysFunHouse.TwitchSubscriberRoleId || x.Id == WubbysFunHouse.PatronRoleId)) {
                    return Task.FromResult(PreconditionResult.FromSuccess());
                } else {
                    return Task.FromResult(PreconditionResult.FromError($"{context.User.Mention} you must be subscribed! Sub <{WubbysFunHouse.TwitchSubscribeUrl}> or link your Twitch <{WubbysFunHouse.LinkTwitchToDiscordUrl}>."));
                }

            }

            return Task.FromResult(PreconditionResult.FromError($"{context.User.Mention} you must be in a guild to run this command."));
        }

    }

}
