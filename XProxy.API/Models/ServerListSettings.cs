using System.ComponentModel;

namespace XProxy.API.Models;

/// <summary>
/// Represents the configuration used for publishing this server to the
/// SCP: Secret Laboratory server list.
/// </summary>
public class ServerListSettings
{
    /// <summary>
    /// Determines whether this server should appear on the public server list.
    /// </summary>
    [Description("If true, the server will be visible on the public SCP:SL server list.")]
    public bool ShowServerOnServerList { get; set; } = false;

    /// <summary>
    /// The human-readable display name shown on the server list.
    /// </summary>
    [Description("The name shown publicly in the server list.")]
    public string DisplayName { get; set; } = "XProxy";

    /// <summary>
    /// The Pastebin ID that provides the server’s external listing configuration or MOTD data.
    /// </summary>
    [Description("Pastebin ID used by SCP:SL for listing metadata or MOTD content.")]
    public string Pastebin { get; set; } = "7wV681fT";

    /// <summary>
    /// The server owner’s contact email.
    /// This is not shown publicly, but is used by SCP:SL staff if they need to reach the server owner.
    /// </summary>
    [Description("Private contact email for SCP:SL staff to reach the server owner if necessary (not shown publicly).")]
    public string Email { get; set; } = "your-email@gmail.com";

    /// <summary>
    /// The public IP address or hostname that the server list should associate with this server.
    /// </summary>
    [Description("The external address used when listing the server (use 'auto' to detect automatically).")]
    public string PublicAddress { get; set; } = "auto";

    /// <summary>
    /// The name of another server from which to inherit or mirror player-count data.
    /// </summary>
    [Description("Optional server name to copy player count from (use empty to disable).")]
    public string TakePlayerCountFromServer { get; set; } = string.Empty;
}
