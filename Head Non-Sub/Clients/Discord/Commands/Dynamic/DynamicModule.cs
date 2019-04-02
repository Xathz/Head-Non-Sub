namespace HeadNonSub.Clients.Discord.Commands.Dynamic {

    public class DynamicModule : BetterModuleBase {

        private readonly DynamicCommands _DynamicCommands;

        public DynamicModule(DynamicCommands service) => _DynamicCommands = service;

    }

}
