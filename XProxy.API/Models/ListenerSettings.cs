using System.ComponentModel;

namespace XProxy.API.Models;

/// <summary>
/// Represents a listening endpoint configuration for XProxy.
/// Each listener defines how and where the proxy accepts incoming player connections,
/// along with its associated SCP:SL server list configuration.
/// </summary>
public class ListenerSettings
{
    /// <summary>
    /// The internal name of this listener.
    /// Used to identify and reference this listener in logs, plugins, or configuration files.
    /// </summary>
    [Description("Internal identifier for this listener, used by plugins or configuration references.")]
    public string Name { get; set; } = "main";

    /// <summary>
    /// The IP address on which the proxy will listen for incoming player connections.
    /// Use <c>0.0.0.0</c> to bind to all available network interfaces.
    /// </summary>
    [Description("Local IP address to listen on (use 0.0.0.0 to bind to all interfaces).")]
    public string ListenAddress { get; set; } = "0.0.0.0";

    /// <summary>
    /// The UDP port number on which the proxy listens for game clients.
    /// </summary>
    [Description("Port number that the proxy listens on for player connections.")]
    public int ListenPort { get; set; } = 7777;

    /// <summary>
    /// The SCP:SL game version this listener supports.
    /// Clients must match this version to connect successfully.
    /// </summary>
    [Description("The SCP:SL game version supported by this listener.")]
    public string GameVersion { get; set; } = "14.1.3";

    /// <summary>
    /// A list of server names (as defined in <see cref="ServerSettings"/>) that this listener
    /// should prioritize when selecting a backend server for player connections.
    /// </summary>
    [Description("List of backend server names to prioritize when routing player connections.")]
    public string[] Priorities { get; set; } = ["default"];

    /// <summary>
    /// Server list configuration used to control how this listener is published
    /// to the SCP:SL public server list or other listing systems.
    /// </summary>
    [Description("Configuration for how this listener is published on the SCP:SL server list.")]
    public ServerListSettings ServerList { get; set; } = new ServerListSettings();
}
