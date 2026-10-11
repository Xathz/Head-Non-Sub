using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace HeadNonSub.Clients.Discord.Attributes {

    /// <summary>
    /// Restrict how often a command can be used.
    /// </summary>
    public sealed class Cooldown : PreconditionAttribute {

        private readonly int _Seconds;
        private readonly bool _PerUser;

        public Cooldown(int seconds, bool perUser = false) {
            _Seconds = seconds;
            _PerUser = perUser;
        }

        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services) {
            if (context.User is SocketGuildUser user) {
                string error = Cooldowns.Check(user, commandInfo.Name, _Seconds, _PerUser);

                if (error is null) {
                    return Task.FromResult(PreconditionResult.FromSuccess());
                } else {
                    return Task.FromResult(PreconditionResult.FromError(error));
                }
            }

            return Task.FromResult(PreconditionResult.FromError($"`{commandInfo.Name}` has a cooldown and can only be used on a server."));
        }

    }

}
