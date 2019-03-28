using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Settings;

namespace HeadNonSub.Clients.Discord.Attributes {

    public class BlacklistEnforced : PreconditionAttribute {

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services) {
            if (context.User is SocketGuildUser user) {
                if (SettingsManager.Configuration.DiscordBlacklist.Any(x => x.Key == context.Guild.Id & x.Value.Contains(user.Id))) {
                    return Task.FromResult(PreconditionResult.FromError($"{context.User.Mention} you are blacklisted. You can not use any commands."));
                } else {
                    return Task.FromResult(PreconditionResult.FromSuccess());
                }
            }

            return Task.FromResult(PreconditionResult.FromError($"{context.User.Mention} you must be in a guild to run this command."));
        }

    }

}
