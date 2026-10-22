using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;

namespace HeadNonSub.Clients.Discord.Attributes {

    /// <summary>
    /// Restrict a command to Twitch and Patreon subscribers.
    /// </summary>
    public sealed class SubscriberOnly : PreconditionAttribute {

        /// <summary>
        /// Reply for users who are not subscribed.
        /// </summary>
        public static string Message => $"You must be subscribed! Sub <{WubbysFunHouse.TwitchSubscribeUrl}> or link your Twitch <{WubbysFunHouse.LinkTwitchToDiscordUrl}>.";

        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services) {
            if (WubbysFunHouse.IsSubscriber(context.User)) {
                return Task.FromResult(PreconditionResult.FromSuccess());
            }

            return Task.FromResult(PreconditionResult.FromError(Message));
        }

    }

}
