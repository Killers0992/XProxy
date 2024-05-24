using XProxy.Patcher.Models;
using XProxy.Shared.Models;
using XProxy.Shared.Serialization;

namespace XProxy.Services
{
    public class ConfigService
    {
        public static ConfigService Instance { get; private set; }

        string _configPath { get; } = "./config_patcher.yml";

        public ConfigService()
        {
            Instance = this;
            Load();
        }

        public PatcherConfig Value { get; private set; }

        void CreateIfMissing()
        {
            if (!File.Exists(_configPath))
                File.WriteAllText(_configPath, YamlParser.Serializer.Serialize(new PatcherConfig()));
        }

        public void Load()
        {
            CreateIfMissing();
            Value = YamlParser.Deserializer.Deserialize<PatcherConfig>(File.ReadAllText(_configPath));
            Save();
        }

        public void Save()
        {
            File.WriteAllText(_configPath, YamlParser.Serializer.Serialize(Value));

        }
    }
}
