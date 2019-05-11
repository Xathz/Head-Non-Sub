namespace HeadNonSub.Entities.Discord {

    public class EmoteOrEmoji {

        public EmoteOrEmoji(string emoji) {
            Id = 0;
            Name = string.Empty;
            Animated = false;
            Emoji = emoji;
        }

        public EmoteOrEmoji(ulong id, string name, bool animated) {
            Id = id;
            Name = name;
            Animated = animated;
            Emoji = string.Empty;
        }

        public ulong Id { get; private set; }

        public string Name { get; private set; }

        public bool Animated { get; private set; }

        public string Emoji { get; private set; }

        public bool IsEmote => !IsEmoji;

        public bool IsEmoji => string.IsNullOrEmpty(Emoji) ? false : true;

        /// <summary>
        /// Emote or emoji formatted.
        /// </summary>
        public override string ToString() {
            if (string.IsNullOrEmpty(Emoji)) {
                return $"<{(Animated ? "a" : "")}:{Name}:{Id}>";
            } else {
                return Emoji;
            }
        }

    }

}
