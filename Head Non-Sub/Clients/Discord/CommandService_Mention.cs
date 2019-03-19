using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace HeadNonSub.Clients.Discord {

    public class CommandService_Mention {

        private readonly CommandService _Commands;
        private readonly DiscordSocketClient _DiscordClient;
        private readonly IServiceProvider _Services;

        public CommandService_Mention(IServiceProvider services) {
            _Commands = services.GetRequiredService<CommandService>();
            _DiscordClient = services.GetRequiredService<DiscordSocketClient>();
            _Services = services;

            _Commands.CommandExecuted += ExecutedAsync;
            _DiscordClient.MessageReceived += MessageReceivedAsync;
        }

        public async Task InitializeAsync() {
            await _Commands.AddModuleAsync<Commands.Blacklist>(_Services);
            await _Commands.AddModuleAsync<Commands.FakeChat>(_Services);
            await _Commands.AddModuleAsync<Commands.Help>(_Services);
            await _Commands.AddModuleAsync<Commands.Tools>(_Services);
            await _Commands.AddModuleAsync<Commands.Whitelist>(_Services);
        }

        private async Task MessageReceivedAsync(SocketMessage socketMessage) {
            if (!(socketMessage is SocketUserMessage message)) { return; }
            if (message.Source != MessageSource.User) { return; }

            int argPos = 0;
            if (!message.HasMentionPrefix(_DiscordClient.CurrentUser, ref argPos)) { return; }

            SocketCommandContext context = new SocketCommandContext(_DiscordClient, message);

            await _Commands.ExecuteAsync(context, argPos, _Services);
        }

        private async Task ExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result) {
            if (!command.IsSpecified) { return; }

            string logLine = $"{context.Guild.Name}; {context.Channel.Name}; {context.User.ToString()}; {context.Message.Content}; {result.ToString()}";

            if (result.IsSuccess) {
                LoggingManager.Log.Info(logLine);
            } else {
                LoggingManager.Log.Warn(logLine);

                switch (result.Error) {
                    case CommandError.UnmetPrecondition:
                        await context.Channel.SendMessageAsync(result.ErrorReason);
                        break;

                    default:
                        break;
                }
            }
        }

    }

}
