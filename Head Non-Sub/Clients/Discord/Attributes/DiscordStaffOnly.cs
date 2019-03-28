using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace HeadNonSub.Clients.Discord.Attributes {

    public class DiscordStaffOnly : PreconditionAttribute {

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services) {
            if (context.User is SocketGuildUser user) {
                IApplication application = context.Client.GetApplicationInfoAsync().Result;

                // Xathz
                if (user.Id == Constants.XathzUserId) {
                    return Task.FromResult(PreconditionResult.FromSuccess());
                }

                // Bot owner
                if (user.Id == application.Owner.Id) {
                    return Task.FromResult(PreconditionResult.FromSuccess());
                }

                // Administrator
                if (user.Roles.Any(x => x.Permissions.Administrator)) {
                    return Task.FromResult(PreconditionResult.FromSuccess());
                }

                // Admins
                if (user.Roles.Any(x => x.Id == WubbysFunHouse.AdminsRoleId)) {
                    return Task.FromResult(PreconditionResult.FromSuccess());
                }

                // Mods
                if (user.Roles.Any(x => x.Id == WubbysFunHouse.ModsRoleId)) {
                    return Task.FromResult(PreconditionResult.FromSuccess());
                }

                // Mod-lites
                if (user.Roles.Any(x => x.Id == WubbysFunHouse.ModLiteRoleId)) {
                    return Task.FromResult(PreconditionResult.FromSuccess());
                }

                return Task.FromResult(PreconditionResult.FromError($"{user.Mention} this is a Discord staff only command."));
            }

            return Task.FromResult(PreconditionResult.FromError($"{context.User.Mention} you must be in a guild to run this command."));
        }

    }

}
