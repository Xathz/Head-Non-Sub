using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace HeadNonSub.Clients.Discord {

    public class MentionCommands {

        private readonly CommandService _Commands;
        private readonly DiscordSocketClient _DiscordClient;
        private readonly IServiceProvider _Services;

        public MentionCommands(IServiceProvider services) {
            _Commands = services.GetRequiredService<CommandService>();
            _DiscordClient = services.GetRequiredService<DiscordSocketClient>();
            _Services = services;

            _Commands.CommandExecuted += Executed;
            _DiscordClient.MessageReceived += MessageReceived;
        }

        public async Task InitializeAsync() {
            await _Commands.AddModuleAsync<Commands.Blacklist>(_Services);
            await _Commands.AddModuleAsync<Commands.FakeChat>(_Services);
            await _Commands.AddModuleAsync<Commands.Help>(_Services);
            await _Commands.AddModuleAsync<Commands.RaidRecovery>(_Services);
            await _Commands.AddModuleAsync<Commands.Tools>(_Services);

            CommandList = _Commands.Commands.ToList().AsReadOnly();
        }

        public ReadOnlyCollection<CommandInfo> CommandList { get; private set; }

        private async Task MessageReceived(SocketMessage socketMessage) {
            if (!(socketMessage is SocketUserMessage message)) { return; }
            if (message.Source != MessageSource.User) { return; }

            int argPos = 0;
            if (!message.HasMentionPrefix(_DiscordClient.CurrentUser, ref argPos)) { return; }

            SocketCommandContext context = new SocketCommandContext(_DiscordClient, message);

#if DEBUG
            if (context.User.Id != Constants.XathzUserId) {
                await context.Channel.SendMessageAsync($"{context.User.Mention} I am currently being worked on, please try that again later.");
                return;
            }
#endif

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
