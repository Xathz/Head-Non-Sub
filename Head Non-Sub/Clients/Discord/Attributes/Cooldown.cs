﻿using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Database;
using Humanizer;

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

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services) {
            if (context.User is SocketGuildUser user) {

                if (WubbysFunHouse.IsDiscordStaff(user)) {
                    return Task.FromResult(PreconditionResult.FromSuccess());
                }

                DateTimeOffset? offset = DatabaseManager.Cooldowns.Check(context.Guild.Id, context.User.Id, command.Name, _PerUser);

                if (offset.HasValue) {
                    if (offset.Value.AddSeconds(_Seconds) >= DateTimeOffset.UtcNow) {
                        string remaining = (offset.Value.AddSeconds(_Seconds) - DateTimeOffset.UtcNow).TotalSeconds.Seconds().Humanize();

                        if (_PerUser) {
                            return Task.FromResult(PreconditionResult.FromError($"{context.User.Mention} You need to wait {remaining} before you can use `{command.Name}` again. Cooldown is per-user."));
                        } else {
                            return Task.FromResult(PreconditionResult.FromError($"You need to wait {remaining} before you can use `{command.Name}` again. Cooldown is server wide."));
                        }
                    } else {
                        LoggingManager.Log.Debug($"Command \"{command.Name}\" {(_PerUser ? "per-user" : "server wide")} cooldown finished {(_PerUser ? $"for {context.User}" : "")}in {context.Guild.Name}");

                        DatabaseManager.Cooldowns.Delete(context.Guild.Id, context.User.Id, command.Name, _PerUser);
                        return Task.FromResult(PreconditionResult.FromSuccess());
                    }
                } else {
                    LoggingManager.Log.Debug($"Command \"{command.Name}\" ({_Seconds}sec) {(_PerUser ? "per-user" : "server wide")} cooldown started {(_PerUser ? $"for {context.User}" : "")}in {context.Guild.Name}");

                    DatabaseManager.Cooldowns.Insert(context.Guild.Id, context.User.Id, command.Name);
                    return Task.FromResult(PreconditionResult.FromSuccess());
                }
            } else {
                return Task.FromResult(PreconditionResult.FromError($"`{command.Name}` has a cooldown and can only be used on a server, not via direct messages."));
            }
        }

    }

}
