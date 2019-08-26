using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;

namespace HeadNonSub.Clients.Discord.Attributes {

    /// <summary>
    /// Restrict command to only these users.
    /// </summary>
    public class AllowedUsers : PreconditionAttribute {

        private readonly List<ulong> _AllowedUsers = new List<ulong>();

        public AllowedUsers(params ulong[] allowedUsers) => _AllowedUsers.AddRange(allowedUsers);

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services) {
            if (_AllowedUsers.Contains(context.User.Id)) {
                return Task.FromResult(PreconditionResult.FromSuccess());
            } else {
                return Task.FromResult(PreconditionResult.FromError($"{context.User.Mention} you are not allowed to run this command."));
            }
        }

    }

}
