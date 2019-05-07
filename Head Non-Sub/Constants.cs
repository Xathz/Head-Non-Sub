using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace HeadNonSub {

    public static class Constants {

        /// <summary>
        /// Application name.
        /// </summary>
        public const string ApplicationName = "Head Non-Sub";

        /// <summary>
        /// Application name with no spaces. 
        /// </summary>
        public const string ApplicationNameFormatted = "HeadNonSub";

        /// <summary>
        /// Application creators name. 
        /// </summary>
        public const string Creator = "Xathz (Xathz#6861); https://github.com/Xathz";

        /// <summary>
        /// Discord trims double spaces in embeds, this prevents that.
        /// </summary>
        public const string DoubleSpace = " \u200B ";

        /// <summary>
        /// Zero width space.
        /// </summary>
        public const char ZeroWwidthSpace = '\u200B';

        /// <summary>
        /// Date and time format used in messages.
        /// </summary>
        public const string DateTimeFormat = "MM/dd/yyyy hh:mm:ss.fff tt";

        /// <summary>
        /// Date and time format used in messages.
        /// </summary>
        public const string DateTimeFormatMedium = "MM/dd/yyyy hh:mm:ss tt";

        /// <summary>
        /// Date and time format used in messages.
        /// </summary>
        public const string DateTimeFormatShort = "MM/dd/yyyy hh:mm tt";

        /// <summary>
        /// Wubby's Fun House.
        /// </summary>
        public const ulong WubbyDiscordGuild = 328300333010911242;

        /// <summary>
        /// Xathz's Dev Emporium.
        /// </summary>
        public const ulong XathzDiscordGuild = 337715715668705291;

        /// <summary>
        /// Xathz's user id.
        /// </summary>
        public const ulong XathzUserId = 227088829079617536;

        /// <summary>
        /// Loading gif url.
        /// </summary>
        public const string LoadingGifUrl = "https://cdn.discordapp.com/attachments/559869208976949278/559869759257051136/Loading.gif";

        /// <summary>
        /// Twitch logo.
        /// </summary>
        public const string TwitchLogoTransparent = "https://cdn.discordapp.com/attachments/559869208976949278/566009004778848296/Twitch.png";

        /// <summary>
        /// Commands help file url.
        /// </summary>
        public const string CommandsHelpUrl = "https://github.com/Xathz/Head-Non-Sub/tree/master/Head%20Non-Sub/Clients/Discord/Commands/Commands.md";

        /// <summary>
        /// The general color of the bot, used for Discord embeds.
        /// </summary>
        public static (int R, int G, int B) GeneralColor => (200, 40, 150);

        /// <summary>
        /// Current process id.
        /// </summary>
        public static int ProcessId => Process.GetCurrentProcess().Id;

        /// <summary>
        /// Application version.
        /// </summary>
        public static string ApplicationVersion => typeof(Program).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        /// <summary>
        /// The application location including filename.
        /// </summary>
        public static string ApplicationLocation => typeof(Program).GetTypeInfo().Assembly.Location;

        /// <summary>
        /// The directory the application is in.
        /// </summary>
        public static string ApplicationDirectory => new FileInfo(ApplicationLocation).DirectoryName;

        /// <summary>
        /// Current executable name minus the extension.
        /// </summary>
        public static string ExecutableName => Path.GetFileNameWithoutExtension(ApplicationLocation);

        /// <summary>
        /// Working directory for the application.
        /// </summary>
        public static string WorkingDirectory => Path.Combine(ApplicationDirectory, ExecutableName);

        /// <summary>
        /// Log files for the application.
        /// </summary>
        public static string LogDirectory => Path.Combine(WorkingDirectory, "Logs");

        /// <summary>
        /// Temporary files for the application.
        /// </summary>
        public static string TemporaryDirectory => Path.Combine(WorkingDirectory, "Temp");

        /// <summary>
        /// Runtime files for the application.
        /// </summary>
        public static string RuntimesDirectory => Path.Combine(WorkingDirectory, "Runtimes");

        /// <summary>
        /// Content files for the application.
        /// </summary>
        public static string ContentDirectory => Path.Combine(WorkingDirectory, "Content");

        /// <summary>
        /// Image templates for the application.
        /// </summary>
        public static string TemplatesDirectory => Path.Combine(ContentDirectory, "Templates");

        /// <summary>
        /// Audio content files for the application.
        /// </summary>
        public static string AudioDirectory => Path.Combine(ContentDirectory, "Audio");

        /// <summary>
        /// Font files for the application.
        /// </summary>
        public static string FontsDirectory => Path.Combine(ContentDirectory, "Fonts");

        /// <summary>
        /// Settings file location.
        /// </summary>
        public static string SettingsFile => Path.Combine(WorkingDirectory, $"{ExecutableName}.settings");

        /// <summary>
        /// Process id file location.
        /// </summary>
        public static string ProcessIdFile => Path.Combine(WorkingDirectory, $"{ExecutableName}.pid");

        /// <summary>
        /// Fail fast file.
        /// </summary>
        public static string FailFastFile => Path.Combine(WorkingDirectory, $"{ExecutableName}.failfast");

        /// <summary>
        /// Common words
        /// </summary>
        public static List<string> CommonWords => new List<string>() { "a", "able", "about", "after", "all", "an", "and", "as", "ask", "at", "bad", "be", "big", "but",
            "by", "call", "case", "come", "company", "day", "different", "do", "early", "eye", "fact", "feel", "few", "find", "first", "for", "from", "get",
            "give", "go", "good", "government", "great", "group", "hand", "have", "he", "her", "high", "his", "i", "important", "in", "into", "it", "know", "large",
            "last", "leave", "life", "like", "little", "long", "look", "make", "man", "my", "new", "next", "not", "number", "of", "old", "on", "one", "or", "other",
            "over", "own", "part", "person", "place", "point", "problem", "public", "right", "same", "say", "see", "seem", "she", "small", "take", "tell", "that", "the",
            "their", "there", "they", "thing", "think", "this", "time", "to", "try", "up", "use", "want", "was", "way", "week", "went", "will", "with", "work",
            "world", "would", "year", "you", "young" };

    }

}
