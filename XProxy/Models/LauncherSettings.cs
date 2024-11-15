using System.ComponentModel;
using XProxy.Misc;
using XProxy.Serialization;

namespace XProxy.Models
{
    public class LauncherSettings
    {
        public const string LauncherPath = "./launcher_settings.yml";
        public const int MaxRetries = 5;

        public static LauncherSettings Value;

        public static bool Load()
        {
            int retryCount = 0;

            retryLoading:

            if (!File.Exists(LauncherPath))
            {
                File.WriteAllText(LauncherPath, ConsoleYamlParser.Serializer.Serialize(new LauncherSettings()));
            }

            try
            {
                string content = File.ReadAllText(LauncherPath);
                Value = ConsoleYamlParser.Deserializer.Deserialize<LauncherSettings>(content);
            }
            catch (Exception ex)
            {
                ConsoleLogger.Error("Failed loading config \"launcher_settings.yml\", press any key to retry");
                ConsoleLogger.Error(ex);

                Console.ReadKey();
                retryCount++;

                if (retryCount >= MaxRetries)
                    return false;

                goto retryLoading;
            }

            ConsoleLogger.AnsiDisabled = Value.DisableAnsiColors;

            return true;
        }

        [Description("Disables ansi colors in console logs")]
        public bool DisableAnsiColors { get; set; } = false;

        [Description("If set to true updater will not try to update XProxy on startup.")]
        public bool DisableUpdater { get; set; } = false;

        [Description("For which version of game XProxy will be downloaded")]
        public string GameVersion { get; set; } = "latest";
    }
}
