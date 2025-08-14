using Serialization;

namespace XProxy;

public class Settings
{
    public static Settings Singleton { get; private set; }

    public static void Load()
    {
        if (!File.Exists("settings.yml"))
            File.WriteAllText("settings.yml", YamlParser.Serializer.Serialize(new Settings()));

        Singleton = YamlParser.Deserializer.Deserialize<Settings>(File.ReadAllText("settings.yml"));
        Save();
    }

    public static void Save()
    {
        File.WriteAllText("settings.yml", YamlParser.Serializer.Serialize(Singleton));
    }

    public int PlayerLimit { get; set; } = -1;

    public List<ListenerSettings> Listeners { get; set; } = new List<ListenerSettings>()
    {
        new ListenerSettings()
    };

    public List<ServerSettings> Servers { get; set; } = new List<ServerSettings>()
    {
        new ServerSettings()
    };
}

public class ListenerSettings
{
    public string ListenAddress { get; set; } = "0.0.0.0";
    public int ListenPort { get; set; } = 7777;

    public string Address { get; set; } = "auto";

    public string ShortName { get; set; } = "Main";
    public string Name { get; set; } = "XProxy";

    public string GameVersion { get; set; } = "14.1.3";

    public string[] Priorities { get; set; } = new string[] { "default" };

    public bool DisplayOnServerList { get; set; }
    public string Pastebin { get; set; } = "7wV681fT";
    public string Email { get; set; } = "your-email@gmail.com";
    public string TakePlayerCountFromServer { get; set; } = string.Empty;
}

public class ServerSettings
{
    public string Address { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 7778;

    public string Name { get; set; } = "Default";

    public bool ForwardIpAddress { get; set; } = false;
}
























































