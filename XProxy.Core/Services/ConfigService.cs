using System.IO;
using XProxy.Shared.Serialization;
using XProxy.Models;
using XProxy.Core;

namespace XProxy.Services
{
    public class ConfigService
    {
        public static string MainDirectory;

        public static ConfigService Singleton { get; internal set; }

        string _configPath { get; } = "config.yml";
        string _languagesDir { get; set; } = "Languages";
        
        string _defaultLanguagePath { get; } = "messages_en.yml";

        public ConfigService()
        {
            if (!Directory.Exists(Path.Combine(MainDirectory, _languagesDir)))
                Directory.CreateDirectory(Path.Combine(MainDirectory, _languagesDir));

            Load(true);

            Logger.Info(Messages.ProxyVersion.Replace("%version%", BuildInformation.VersionText).Replace("%gameVersion%", string.Join(", ", BuildInformation.SupportedGameVersions)), "XProxy");
        }

        public ConfigModel Value { get; private set; } = new ConfigModel();
        public MessagesModel Messages { get; private set; } = new MessagesModel();

        public MessagesModel GetMessagesForLanguage(string language)
        {
            string path = Path.Combine(MainDirectory, this._languagesDir, $"messages_{language}.yml");

            if (!File.Exists(path))
            {
                // Get fallback.
                CreateIfMissing();
                return YamlParser.Deserializer.Deserialize<MessagesModel>(File.ReadAllText(Path.Combine(MainDirectory, _defaultLanguagePath)));
            }

            return YamlParser.Deserializer.Deserialize<MessagesModel>(File.ReadAllText(path));
        }

        void CreateIfMissing()
        {
            if (!File.Exists(Path.Combine(MainDirectory, _configPath)))
                File.WriteAllText(Path.Combine(MainDirectory, _configPath), YamlParser.Serializer.Serialize(new ConfigModel()));

            if (!File.Exists(Path.Combine(MainDirectory, _languagesDir, _defaultLanguagePath)))
                File.WriteAllText(Path.Combine(MainDirectory, _languagesDir, _defaultLanguagePath), YamlParser.Serializer.Serialize(new MessagesModel()));
        }

        public void Load(bool intial = false)
        {
            CreateIfMissing();
            Value = YamlParser.Deserializer.Deserialize<ConfigModel>(File.ReadAllText(Path.Combine(MainDirectory, _configPath)));
            Logger.DebugMode = Value.Debug;
            Messages = GetMessagesForLanguage(Value.Langauge);

            if (!intial)
                Listener.UpdateServers = true;

            Save();
            Logger.Debug(Messages.ConfigLoadedMessage, "ConfigService");
        }

        public void Save()
        {
            File.WriteAllText(Path.Combine(MainDirectory, _configPath), YamlParser.Serializer.Serialize(Value));

            string path = Path.Combine(MainDirectory, this._languagesDir, $"messages_{Value.Langauge}.yml");
            if (File.Exists(path))
                File.WriteAllText(path, YamlParser.Serializer.Serialize(Messages));

            Logger.Debug(Messages.ConfigSavedMessage, "ConfigService");
        }
    }
}
