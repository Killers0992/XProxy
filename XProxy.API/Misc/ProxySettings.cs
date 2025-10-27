using System.ComponentModel;

namespace XProxy.API.Misc;

/// <summary>
/// Represents the global configuration for XProxy.
/// Handles loading, saving, and reloading proxy settings from a YAML file.
/// </summary>
public class ProxySettings
{
    /// <summary>
    /// The default path to the YAML configuration file.
    /// </summary>
    public const string SettingsPath = "settings.yml";

    /// <summary>
    /// The globally accessible, loaded instance of the proxy settings.
    /// </summary>
    public static ProxySettings Singleton { get; private set; }

    /// <summary>
    /// Loads the proxy configuration from disk.
    /// Creates a new file with default values if it does not exist.
    /// </summary>
    public static void Load()
    {
        if (!File.Exists(SettingsPath))
            File.WriteAllText(SettingsPath, YamlParser.Serializer.Serialize(new ProxySettings()));

        string settingsContent = File.ReadAllText(SettingsPath);

        try
        {
            Singleton = YamlParser.Deserializer.Deserialize<ProxySettings>(settingsContent);
        }
        catch (Exception ex)
        {
            ProxyLogger.Error($"Failed to deserialize config! {ex}");
            return;
        }

        Save();
    }

    /// <summary>
    /// Reloads the proxy configuration from disk.
    /// </summary>
    public static void Reload() => Load();

    /// <summary>
    /// Saves the current proxy configuration to disk.
    /// </summary>
    public static void Save()
    {
        File.WriteAllText(SettingsPath, YamlParser.Serializer.Serialize(Singleton));
    }

    /// <summary>
    /// The maximum number of players allowed across all connected servers.
    /// Set to -1 to disable the limit.
    /// </summary>
    [Description("Maximum player limit across all servers. Use -1 for unlimited.")]
    public int PlayerLimit { get; set; } = -1;

    /// <summary>
    /// The list of listeners that define how XProxy accepts player connections.
    /// Each listener can have its own port, game version, and server list configuration.
    /// </summary>
    [Description("List of listeners defining how the proxy accepts player connections.")]
    public List<ListenerSettings> Listeners { get; set; } = new List<ListenerSettings>()
    {
        new ListenerSettings()
    };

    /// <summary>
    /// The list of backend servers managed by XProxy.
    /// These entries define connection targets and display names for each proxied server.
    /// </summary>
    [Description("List of backend servers that the proxy connects players to.")]
    public List<ServerSettings> Servers { get; set; } = new List<ServerSettings>()
    {
        new ServerSettings()
    };

    /// <summary>
    /// The ordered list of server names (from <see cref="Servers"/>) that appear in the server selector menu.
    /// </summary>
    [Description("List of server names displayed in the in-game server selector menu.")]
    public string[] ServersInSelector { get; set; } = ["default"];
}