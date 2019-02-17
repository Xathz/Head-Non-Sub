using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace HeadNonSub.Clients.Discord.Attributes {

    public class Cooldown : PreconditionAttribute {

        private readonly int _CooldownSeconds;
        private readonly bool _CooldownPerUser;

        /// <summary>
        /// Restrict how often a command can be used.
        /// </summary>
        public Cooldown(int seconds, bool perUser = false) {
            _CooldownSeconds = seconds;
            _CooldownPerUser = perUser;
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services) {
            (bool isAllowed, int secondsRemaining) = CooldownTracker.IsAllowed(context.Guild.Id, context.User.Id, command.Name, _CooldownSeconds, _CooldownPerUser);

            if (isAllowed) {
                CooldownTracker.Track(context.Guild.Id, context.User.Id, command.Name);

                return Task.FromResult(PreconditionResult.FromSuccess());
            } else {
                PreconditionResult result;
                string remaining = (secondsRemaining == 1 ? "1 more second" : $"{secondsRemaining} more seconds");

                if (_CooldownPerUser) {
                    result = PreconditionResult.FromError($"{context.User.Mention} you have to wait {remaining} before you can use the `{command.Name}` command again.");
                } else {
                    result = PreconditionResult.FromError($"You have to wait {remaining} before you can use the `{command.Name}` command again.");
                }

                context.Channel.SendMessageAsync(result.ErrorReason);
                return Task.FromResult(result);
            }
        }

    }

}
