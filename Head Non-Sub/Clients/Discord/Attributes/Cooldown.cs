using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Database;
using Humanizer;

namespace HeadNonSub.Clients.Discord.Attributes {

    public class Cooldown : PreconditionAttribute {

        private readonly int _Seconds;
        private readonly bool _PerUser;

        /// <summary>
        /// Restrict how often a command can be used.
        /// </summary>
        public Cooldown(int seconds, bool perUser = false) {
            _Seconds = seconds;
            _PerUser = perUser;
        }

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

                // Discord staff
                if (user.Roles.Any(x => WubbysFunHouse.DiscordStaffRoles.Contains(x.Id))) {
                    return Task.FromResult(PreconditionResult.FromSuccess());
                }

            }

            DateTimeOffset? offset = DatabaseManager.Cooldowns.Check(context.Guild.Id, context.User.Id, command.Name, _PerUser);

            if (offset.HasValue) {
                if (offset.Value.AddSeconds(_Seconds) >= DateTimeOffset.Now) {
                    string remaining = (offset.Value.AddSeconds(_Seconds) - DateTimeOffset.Now).TotalSeconds.Seconds().Humanize();

                    if (_PerUser) {
                        return Task.FromResult(PreconditionResult.FromError($"{context.User.Mention} You need to wait {remaining} before you can use `{command.Name}` again. Cooldown is per-user."));
                    } else {
                        return Task.FromResult(PreconditionResult.FromError($"You need to wait {remaining} before you can use `{command.Name}` again. Cooldown is server wide."));
                    }
                } else {
                    DatabaseManager.Cooldowns.Delete(context.Guild.Id, context.User.Id, command.Name);
                    return Task.FromResult(PreconditionResult.FromSuccess());
                }
            } else {
                DatabaseManager.Cooldowns.Insert(context.Guild.Id, context.User.Id, command.Name);
                return Task.FromResult(PreconditionResult.FromSuccess());
            }
    
        }

    }

}
