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
        /// Date and time format used in messages.
        /// </summary>
        public const string DateTimeFormat = "MM/dd/yyyy hh:mm:ss.fff";

        /// <summary>
        /// Date and time format used in messages.
        /// </summary>
        public const string DateTimeFormatShort = "MM/dd/yyyy hh:mm tt";

        /// <summary>
        /// The general color of the bot, used for Discord embeds.
        /// </summary>
        public static (int R, int G, int B) GeneralColor => (200, 40, 150);

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

    }

}
