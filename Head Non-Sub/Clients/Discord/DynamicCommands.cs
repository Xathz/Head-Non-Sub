using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Database;
using Microsoft.Extensions.DependencyInjection;

namespace HeadNonSub.Clients.Discord {

    public class DynamicCommands {

        private readonly CommandService _Commands;
        private readonly DiscordSocketClient _DiscordClient;
        private readonly IServiceProvider _Services;

        private ModuleInfo _DynamicModule;

        public DynamicCommands(IServiceProvider services) {
            _Commands = services.GetRequiredService<CommandService>();
            _DiscordClient = services.GetRequiredService<DiscordSocketClient>();
            _Services = services;

            BuildCommands().GetAwaiter().GetResult();

            _Commands.CommandExecuted += Executed;
            _DiscordClient.MessageReceived += MessageReceived;
        }

        public async Task BuildCommands() {
            if (_DynamicModule != null) {
                await _Commands.RemoveModuleAsync(_DynamicModule);
            }

            _DynamicModule = await _Commands.CreateModuleAsync("", module => {
                module.Name = "Dynamic";

                foreach (KeyValuePair<string, string> command in DatabaseManager.GetDynamicCommands()) {

                    module.AddCommand(command.Key, async (context, args, info, task) => {
                        await context.Channel.SendMessageAsync(command.Value);
                    }, builder => {
                        //builder.AddAliases(tag.Aliases.ToArray());
                    });
                }

            });
        }

        private async Task MessageReceived(SocketMessage socketMessage) {
            if (!(socketMessage is SocketUserMessage userMessage)) { return; }
            if (userMessage.Source != MessageSource.User) { return; }

            int argPos = 0;
            if (!userMessage.HasStringPrefix("-", ref argPos)) { return; }

            SocketCommandContext context = new SocketCommandContext(_DiscordClient, userMessage);

            await _Commands.ExecuteAsync(context, argPos, _Services);
        }

        private async Task Executed(Optional<CommandInfo> command, ICommandContext context, IResult result) {
            if (!command.IsSpecified) { return; }

            if (!result.IsSuccess) {
                LoggingManager.Log.Warn(result.ErrorReason);

                switch (result.Error) {
                    case CommandError.UnmetPrecondition:
                        await context.Channel.SendMessageAsync(result.ErrorReason);
                        break;

                    case CommandError.ObjectNotFound:
                        await context.Channel.SendMessageAsync(result.ErrorReason);
                        break;

                    case CommandError.ParseFailed:
                        await context.Channel.SendMessageAsync(result.ErrorReason);
                        break;

                    default:
                        break;
                }
            }
        }

    }

}
