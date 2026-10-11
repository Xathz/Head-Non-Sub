using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace HeadNonSub.Clients.Discord {

    public class SlashCommands {

        private readonly InteractionService _Interactions;
        private readonly DiscordSocketClient _DiscordClient;
        private readonly IServiceProvider _Services;

        public SlashCommands(IServiceProvider services) {
            _Interactions = services.GetRequiredService<InteractionService>();
            _DiscordClient = services.GetRequiredService<DiscordSocketClient>();
            _Services = services;

            _Interactions.InteractionExecuted += Executed;
            _DiscordClient.InteractionCreated += InteractionCreated;
            _DiscordClient.Ready += Ready;
        }

        public async Task InitializeAsync() => await _Interactions.AddModulesAsync(typeof(SlashCommands).Assembly, _Services);

        private async Task Ready() {
            try {
                // Guild commands update right away, global commands can take up to an hour
                foreach (ulong guildId in new ulong[] { WubbysFunHouse.ServerId, Constants.XathzDiscordGuild }) {
                    if (_DiscordClient.GetGuild(guildId) is SocketGuild guild) {
                        await _Interactions.RegisterCommandsToGuildAsync(guild.Id, true);
                        LoggingManager.Log.Info($"Registered {_Interactions.SlashCommands.Count} slash commands in {guild.Name}");
                    }
                }
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex, "Failed to register slash commands");
            }
        }

        private async Task InteractionCreated(SocketInteraction interaction) {
#if DEBUG
            if (interaction.User.Id != Constants.XathzUserId) {
                if (interaction is SocketSlashCommand) {
                    await interaction.RespondAsync("I am currently being worked on, please try that again later.", ephemeral: true);
                }

                return;
            }
#endif

            SocketInteractionContext context = new SocketInteractionContext(_DiscordClient, interaction);
            await _Interactions.ExecuteCommandAsync(context, _Services);
        }

        private async Task Executed(ICommandInfo command, IInteractionContext context, IResult result) {
            if (result.IsSuccess) { return; }

            if (result is ExecuteResult executeResult && executeResult.Exception is Exception ex) {
                LoggingManager.Log.Error(ex, $"Command {command?.Name} failed");
            } else {
                LoggingManager.Log.Warn($"Command {command?.Name}: {result.ErrorReason}");
            }

            // Autocomplete can't be replied to with a message
            if (context.Interaction is IAutocompleteInteraction) { return; }

            string message = result.Error == InteractionCommandError.Exception ? "Something went wrong running that command." : result.ErrorReason;

            try {
                if (context.Interaction.HasResponded) {
                    await context.Interaction.FollowupAsync(message, ephemeral: true, allowedMentions: DiscordClient.UserMentionsOnly);
                } else {
                    await context.Interaction.RespondAsync(message, ephemeral: true, allowedMentions: DiscordClient.UserMentionsOnly);
                }
            } catch (Exception replyEx) {
                LoggingManager.Log.Error(replyEx);
            }
        }

    }

}
