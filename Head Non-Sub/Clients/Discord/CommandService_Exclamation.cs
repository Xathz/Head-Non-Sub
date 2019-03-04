using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace HeadNonSub.Clients.Discord {

    public class CommandService_Exclamation {

        private readonly CommandService _Commands;
        private readonly DiscordSocketClient _DiscordClient;
        private readonly IServiceProvider _Services;
        private readonly HashSet<string> _ValidCommands = new HashSet<string>();

        public CommandService_Exclamation(IServiceProvider services) {
            _Commands = services.GetRequiredService<CommandService>();
            _DiscordClient = services.GetRequiredService<DiscordSocketClient>();
            _Services = services;

            _Commands.CommandExecuted += ExecutedAsync;
            _DiscordClient.MessageReceived += MessageReceivedAsync;

            // TODO Does not show who deleted the message, if the bot deletes a message it yells at the bot.
            //_DiscordClient.MessageDeleted += MessageDeletedAsync;
        }

        public async Task InitializeAsync() {
            await _Commands.AddModuleAsync<Commands.Exclamation.AudioSpam>(_Services);
            await _Commands.AddModuleAsync<Commands.Exclamation.Fake>(_Services);
            await _Commands.AddModuleAsync<Commands.Exclamation.ImageTemplates>(_Services);
            await _Commands.AddModuleAsync<Commands.Exclamation.Poll>(_Services);
            await _Commands.AddModuleAsync<Commands.Exclamation.Rave>(_Services);
            await _Commands.AddModuleAsync<Commands.Exclamation.Rythm>(_Services);
            await _Commands.AddModuleAsync<Commands.Exclamation.Spam>(_Services);
            await _Commands.AddModuleAsync<Commands.Exclamation.Stock>(_Services);
            await _Commands.AddModuleAsync<Commands.Exclamation.Strawpoll>(_Services);
            await _Commands.AddModuleAsync<Commands.Exclamation.TTS>(_Services);
            await _Commands.AddModuleAsync<Commands.Exclamation.Yam>(_Services);

            _Commands.Commands.ToList().ForEach(x => {
                _ValidCommands.Add(x.Name);
                x.Aliases.ToList().ForEach(a => _ValidCommands.Add(a));
            });
        }

        private async Task MessageReceivedAsync(SocketMessage rawMessage) {
            if (!(rawMessage is SocketUserMessage message)) { return; }
            if (message.Source != MessageSource.User) { return; }

            int argPos = 0;
            if (!message.HasStringPrefix("!", ref argPos)) { return; }

            SocketCommandContext context = new SocketCommandContext(_DiscordClient, message);

            await _Commands.ExecuteAsync(context, argPos, _Services);
        }

        private async Task MessageDeletedAsync(Cacheable<IMessage, ulong> cache, ISocketMessageChannel channel) {
            if (!(cache.Value is SocketUserMessage message)) { return; }
            if (message.Source != MessageSource.User) { return; }

            try {
                int argPos = 0;
                if (message.HasStringPrefix("!", ref argPos)) {
                    string command = message.Content.Substring(1, (message.Content.Contains(' ') ? message.Content.IndexOf(' ') - 1 : message.Content.Length - 1));

                    if (_ValidCommands.Contains(command)) {
                        await channel.SendMessageAsync($"Please do not delete invoking commands. ```{message.CreatedAt.ToString(Constants.DateTimeFormat)} utc; {message.Author.ToString()}{Environment.NewLine}" +
                            $"{message.Content}```");
                    }
                }
            } catch { }
        }

        private async Task ExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result) {
            if (!command.IsSpecified) { return; }

            string logLine = $"{context.User.Username} ({context.User.Id}); Message: {context.Message.Content}; Command: {command.Value.Name}; Result: {result.ToString()}";

            if (result.IsSuccess) {
                LoggingManager.Log.Info(logLine);
            } else {
                LoggingManager.Log.Warn(logLine);

                switch (result.Error) {
                    case CommandError.UnmetPrecondition:
                        await context.Channel.SendMessageAsync(result.ErrorReason);
                        break;

                    default:
                        //await context.Channel.SendMessageAsync(result.ErrorReason);
                        break;
                }
            }
        }

    }

}
