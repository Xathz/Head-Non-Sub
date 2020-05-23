namespace HeadNonSub.Entities.Discord {

    public class MessageTag {

        public MessageTag(TagType tagType, ulong id, int index, int length, bool containsExclamation = false) {
            TagType = tagType;
            Id = id;
            Index = index;
            Length = length;
            ContainsExclamation = containsExclamation;
        }

        public TagType TagType { get; private set; }

        public ulong Id { get; private set; }

        public int Index { get; private set; }

        public int Length { get; private set; }

        public bool ContainsExclamation { get; private set; }

        /// <summary>
        /// Tag id formatted based on <see cref="TagType"/>.
        /// </summary>
        public override string ToString() => TagType switch {
            TagType.User => $"<@{(ContainsExclamation ? "!" : "")}{Id}>",
            TagType.Role => $"<@&{Id}>",
            TagType.Channel => $"<#{Id}>",
            TagType.Everyone => "@everyone",
            TagType.Here => "@here",
            _ => Id.ToString(),
        };

    }

    public enum TagType {
        User,
        Role,
        Channel,
        Everyone,
        Here
    }

}
