using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;
using HeadNonSub.Clients.Discord.Attributes;
using HeadNonSub.Database;

namespace HeadNonSub.Clients.Discord.Commands.Exclamation {

    [BlacklistEnforced, SubscriberOnly, AllowedChannels(WubbysFunHouse.ActualFuckingSpamChannelId)]
    [RequireContext(ContextType.Guild)]
    public class Dynamic : BetterModuleBase {

        private readonly DynamicCommands _DynamicCommands;

        private readonly List<string> _ProtectedCommands = new List<string>() { "claim", "abandon", "reclaim", "who", "command", "cmd", "delete" };

        public Dynamic(DynamicCommands service) => _DynamicCommands = service;

        [Command("claim")]
        [Priority(99)]
        public Task Claim(string command, [Remainder]string text) {
            string cleanCommand = command.ToLower();
            if (cleanCommand.StartsWith("-")) { cleanCommand = cleanCommand.Remove(0, 1); }

            if (_ProtectedCommands.Contains(cleanCommand)) {
                return BetterReplyAsync($"{Context.User.Mention} `-{cleanCommand}` is a protected command you can not claim.");
            }

            (bool successful, string reason) = _DynamicCommands.AddCommand(Context.User.Id, cleanCommand, text).Result;

            return BetterReplyAsync($"{Context.User.Mention} {reason}");
        }

        [Command("who")]
        [Priority(99)]
        public Task Who(string command) {
            string cleanCommand = command.ToLower();
            if (cleanCommand.StartsWith("-")) { cleanCommand = cleanCommand.Remove(0, 1); }

            ulong? user = DatabaseManager.WhoDynamicCommand(cleanCommand);

            if (user.HasValue) {
                return BetterReplyAsync($"`-{command}` was claimed by {BetterUserFormat(UserFromUserId(user.Value))}.");
            } else {
                return BetterReplyAsync($"{Context.User.Mention} `-{command}` was not found.");
            }
        }

    }

}
