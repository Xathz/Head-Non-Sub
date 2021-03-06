﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;

namespace HeadNonSub.Clients.Discord.Attributes {

    /// <summary>
    /// Restrict command to only these guilds (servers).
    /// </summary>
    public sealed class AllowedGuilds : PreconditionAttribute {

        private readonly List<ulong> _AllowedGuilds = new List<ulong>();

        public AllowedGuilds(params ulong[] allowedGuilds) => _AllowedGuilds.AddRange(allowedGuilds);

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services) {
            if (_AllowedGuilds.Contains(context.Guild.Id)) {
                return Task.FromResult(PreconditionResult.FromSuccess());
            } else {
                return Task.FromResult(PreconditionResult.FromError("Invalid guild (server) for this command."));
            }
        }

    }

}
