using System;
using System.IO;
using XProxy.Shared.Serialization;

namespace XProxy.Core
{
    public abstract class Plugin<T> : Plugin where T : class, new()
    {
        string _configPath => Path.Combine(PluginDirectory, "config.yml");

        public T Config { get; private set; }

        public override void LoadConfig()
        {
            if (!Directory.Exists(PluginDirectory))
                Directory.CreateDirectory(PluginDirectory);

            if (!File.Exists(_configPath))
            {
                File.WriteAllText(_configPath, YamlParser.Serializer.Serialize(Activator.CreateInstance(typeof(T))));
            }

            string text = File.ReadAllText(_configPath);
            Config = YamlParser.Deserializer.Deserialize<T>(text);
            SaveConfig();
        }

        public override void SaveConfig()
        {
            if (!Directory.Exists(PluginDirectory))
                Directory.CreateDirectory(PluginDirectory);

            File.WriteAllText(_configPath, YamlParser.Serializer.Serialize(Config));
        }
    }
}
