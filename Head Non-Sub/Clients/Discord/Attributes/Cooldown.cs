using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

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
            if (context.User is SocketGuildUser user) {

                // If the user is an admin bypass cooldown
                if (user.Roles.Any(x => x.Permissions.Administrator)) {
                    return Task.FromResult(PreconditionResult.FromSuccess());
                }

                // Discord staff
                if (user.Roles.Any(x => WubbysFunHouse.DiscordStaffRoles.Contains(x.Id))) {
                    return Task.FromResult(PreconditionResult.FromSuccess());
                }
            }

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

                return Task.FromResult(result);
            }
        }

    }

}
