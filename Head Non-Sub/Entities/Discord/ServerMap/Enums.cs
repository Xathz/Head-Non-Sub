using System.Text.Json.Serialization;

namespace HeadNonSub.Entities.Discord.ServerMap {

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ChannelType {
        Text,
        Voice,
        News
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PermissionTarget {
        Role,
        User
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PermissionValue {
        Allow,
        Deny,
        Inherit
    }

}
