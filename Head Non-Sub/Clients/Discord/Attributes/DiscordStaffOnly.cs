﻿using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace HeadNonSub.Clients.Discord.Attributes {

    public sealed class DiscordStaffOnly : PreconditionAttribute {

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services) {
            if (context.User is SocketGuildUser user) {

                if (WubbysFunHouse.IsDiscordStaff(user)) {
                    return Task.FromResult(PreconditionResult.FromSuccess());
                }

                return Task.FromResult(PreconditionResult.FromError($"{user.Mention} this is a Discord staff only command."));
            }

            return Task.FromResult(PreconditionResult.FromError($"{context.User.Mention} you must be in a guild to run this command."));
        }

    }

}
