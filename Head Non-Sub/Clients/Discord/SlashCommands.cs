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

        private bool _Registered;

        public SlashCommands(IServiceProvider services) {
            _Interactions = services.GetRequiredService<InteractionService>();
            _DiscordClient = services.GetRequiredService<DiscordSocketClient>();
            _Services = services;

            _Interactions.InteractionExecuted += Executed;
            _DiscordClient.InteractionCreated += InteractionCreated;
            _DiscordClient.Ready += Ready;
        }

        public async Task InitializeAsync() => await _Interactions.AddModulesAsync(typeof(SlashCommands).Assembly, _Services);

        private Task Ready() {
            // Ready fires on every reconnect, only register once and keep it off the gateway thread
            if (!_Registered) {
                _ = Task.Run(RegisterAsync);
            }

            return Task.CompletedTask;
        }

        private async Task RegisterAsync() {
            try {
                // Guild commands update right away, global commands can take up to an hour
                foreach (ulong guildId in new ulong[] { WubbysFunHouse.ServerId, Constants.XathzDiscordGuild }) {
                    if (_DiscordClient.GetGuild(guildId) is SocketGuild guild) {
                        await _Interactions.RegisterCommandsToGuildAsync(guild.Id, true);
                        LoggingManager.Log.Info($"Registered {_Interactions.SlashCommands.Count} slash commands in {guild.Name}");
                    }
                }

                _Registered = true;
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
                await EphemeralReplyAsync(context.Interaction, message);
            } catch (Exception replyEx) {
                LoggingManager.Log.Error(replyEx);
            }
        }

        /// <summary>
        /// Reply only the user running the command can see.
        /// </summary>
        /// <param name="interaction">Interaction to reply to.</param>
        /// <param name="message">Contents of the message.</param>
        public static async Task EphemeralReplyAsync(IDiscordInteraction interaction, string message) {
            if (!interaction.HasResponded) {
                await interaction.RespondAsync(message, ephemeral: true, allowedMentions: DiscordClient.UserMentionsOnly);
                return;
            }

            // The first follow up replaces a public "thinking" message and ignores ephemeral, so remove it first
            IUserMessage original = await interaction.GetOriginalResponseAsync();
            if (original.Flags.HasValue && original.Flags.Value.HasFlag(MessageFlags.Loading) && !original.Flags.Value.HasFlag(MessageFlags.Ephemeral)) {
                await interaction.DeleteOriginalResponseAsync();
            }

            await interaction.FollowupAsync(message, ephemeral: true, allowedMentions: DiscordClient.UserMentionsOnly);
        }

    }

}
