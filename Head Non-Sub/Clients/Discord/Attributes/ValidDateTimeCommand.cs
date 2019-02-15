using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace HeadNonSub.Clients.Discord.Attributes {

    public class ValidDateTimeCommand : ParameterPreconditionAttribute {

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services) {

            return Task.FromResult(PreconditionResult.FromError("NYI"));

        }

    }

}
