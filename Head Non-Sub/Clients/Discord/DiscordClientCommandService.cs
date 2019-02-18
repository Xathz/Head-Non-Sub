using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HeadNonSub.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace HeadNonSub.Clients.Discord {

    public class DiscordClientCommandService {

        private readonly CommandService _Commands;
        private readonly DiscordSocketClient _DiscordClient;
        private readonly IServiceProvider _Services;

        public DiscordClientCommandService(IServiceProvider services) {
            _Commands = services.GetRequiredService<CommandService>();
            _DiscordClient = services.GetRequiredService<DiscordSocketClient>();
            _Services = services;

            _Commands.CommandExecuted += CommandExecutedAsync;
            _DiscordClient.MessageReceived += MessageReceivedAsync;
        }

        // Add all
        //public async Task InitializeAsync() => await _Commands.AddModulesAsync(Assembly.GetEntryAssembly(), _Services);

        public async Task InitializeAsync() {
            await _Commands.AddModuleAsync<Commands.FakeChat>(_Services);
            await _Commands.AddModuleAsync<Commands.Help>(_Services);
            await _Commands.AddModuleAsync<Commands.Tools>(_Services);
            await _Commands.AddModuleAsync<Commands.Whitelist>(_Services);
        } 

        private async Task MessageReceivedAsync(SocketMessage rawMessage) {
            if (!(rawMessage is SocketUserMessage message)) { return; }
            if (message.Source != MessageSource.User) { return; }

            int argPos = 0;
            if (!message.HasMentionPrefix(_DiscordClient.CurrentUser, ref argPos)) { return; }

            SocketCommandContext context = new SocketCommandContext(_DiscordClient, message);

            await _Commands.ExecuteAsync(context, argPos, _Services);

            // TODO This is a really bad way to do this. Tried many attempts to correctly detect ManageMessages permissions.
            //      Need to find a good way to detect role, category, channel, and direct permissions.
            if (SettingsManager.Configuration.DeleteInvokingMessage) {
                try {
                    _ = message.DeleteAsync();
                } catch { }
            }
        }

        private async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result) {
            if (!command.IsSpecified) {
                // Disable message feedback if it was not a real command
                //await context.Channel.SendMessageAsync($"Invalid command. `@{_DiscordClient.CurrentUser.Username} help` for info and commands.");
                return;
            }

            string logLine = $"{context.User.Username} ({context.User.Id}); Message: {context.Message.Content}; Command: {command.Value.Name}; Result: {result.ToString()}";

            if (result.IsSuccess) {
                LoggingManager.Log.Info(logLine);
            } else {
                LoggingManager.Log.Warn(logLine);

                switch (result.Error) {
                    default:
                        // Disable message feedback for errors
                        //await context.Channel.SendMessageAsync($"{context.User.Mention} {result.ErrorReason}");
                        break;
                }
            }
        }

    }

}
