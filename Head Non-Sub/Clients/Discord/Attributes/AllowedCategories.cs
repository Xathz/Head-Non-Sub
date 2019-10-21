using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace HeadNonSub.Clients.Discord.Attributes {

    /// <summary>
    /// Restrict command to only these categories.
    /// </summary>
    public sealed class AllowedCategories : PreconditionAttribute {

        private readonly List<ulong> _AllowedCategories = new List<ulong>();

        public AllowedCategories(params ulong[] allowedCategories) => _AllowedCategories.AddRange(allowedCategories);

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services) {
            if (context.Channel is SocketTextChannel textChannel) {
                if (textChannel.CategoryId.HasValue) {
                    if (_AllowedCategories.Contains(textChannel.CategoryId.Value)) {
                        return Task.FromResult(PreconditionResult.FromSuccess());
                    }
                }
            }

            return Task.FromResult(PreconditionResult.FromError("Invalid channel category for this command."));
        }

    }

}
