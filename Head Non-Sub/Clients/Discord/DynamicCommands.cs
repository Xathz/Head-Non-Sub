using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Clients.Discord.Attributes;
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

        public async Task InitializeAsync() => await _Commands.AddModuleAsync<Commands.Exclamation.Dynamic>(_Services);

        public async Task<(bool successful, string reason)> AddCommand(ulong ownerId, string command, string text) {
            (bool successful, string reason) = DatabaseManager.DynamicCommands.Insert(ownerId, command, text);

            if (successful) {
                await BuildCommands();
            }

            return (successful, reason);
        }

        public async Task BuildCommands() {
            if (_DynamicModule != null) {
                await _Commands.RemoveModuleAsync(_DynamicModule);
            }

            _DynamicModule = await _Commands.CreateModuleAsync("", module => {
                module.Name = "DynamicCommands";

                foreach (KeyValuePair<string, string> command in DatabaseManager.DynamicCommands.GetAll()) {
                    module.AddCommand(command.Key, async (context, args, info, task) => {
                        await context.Channel.SendMessageAsync(command.Value);
                    }, builder => {
                        builder.WithPriority(1);
                        builder.AddPrecondition(new BlacklistEnforced());
                        builder.AddPrecondition(new SubscriberOnly());
                        builder.AddPrecondition(new AllowedChannels(WubbysFunHouse.ActualFuckingSpamChannelId));
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

                    case CommandError.BadArgCount:
                        await context.Channel.SendMessageAsync($"{context.User.Mention} Invalid... whatever you tried to do. Type `-claim <command> <whatever>` to claim one. Choose wisely because you can only claim **one**.");
                        break;

                    case CommandError.UnknownCommand:
                        await context.Channel.SendMessageAsync($"{context.User.Mention} `{command.Value}` is not claimed. Type `-claim <command> <whatever>` to claim one. Choose wisely because you can only claim **one**.");
                        break;

                    default:
                        break;
                }
            }
        }

    }

}
