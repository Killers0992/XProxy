using System;
using System.Collections.Generic;
using System.ComponentModel;
using XProxy.Shared.Enums;
using YamlDotNet.Serialization;

namespace XProxy.Shared.Models
{
    public class ConfigModel
    {
        [YamlIgnore]
        private System.Version _version;

        [Description("Enables debug logs.")]
        public bool Debug { get; set; }
        [Description("Language of messages.")]
        public string Langauge { get; set; } = "en";
        [Description("Port which proxy will use to listen for connections.")]
        public ushort Port { get; set; } = 7777;

        [Description("Email used for listing your server on SCP SL serverlist.")]
        public string Email { get; set; } = "example@gmail.com";

        [Description("Server name.")]
        public string ServerName { get; set; } = "Example server name.";

        [Description("Server information.")]
        public string Pastebin { get; set; } = "7wV681fT";

        [Description("Version of game.")]
        public string GameVersion { get; set; } = "13.5.1";

        [YamlIgnore]
        public System.Version GameVersionParsed
        {
            get
            {
                if (_version == null)
                {
                    string text = GameVersion.Contains("-") ? GameVersion.Split('-')[0] : GameVersion;

                    System.Version.TryParse(text, out _version);
                }

                return _version;
            }
        }

        [Description("Maximum amount of players which can connect to your proxy.")]
        public int MaxPlayers { get; set; } = 50;

        [Description("Priority servers used for first connection and fallback servers.")]
        public List<string> Priorities { get; set; } = new List<string>() { "lobby" };

        [Description("Available servers.")]
        public Dictionary<string, ServerModel> Servers { get; set; } = new Dictionary<string, ServerModel>()
        {
            { "lobby", new ServerModel()
                {
                    Name = "Lobby",
                    Ip = "127.0.0.1",
                    PublicIp = "127.0.0.1",
                    Port = 7777,
                    MaxPlayers = 50,
                    ConnectionType = ConnectionType.Simulated,
                    Simulation = "lobby",
                    SendIpAddressInPreAuth = false,
                } 
            },
            { "vanilla", new ServerModel()
                {
                    Name = "Vanilla",
                    Ip = "127.0.0.1",
                    PublicIp = "127.0.0.1",
                    Port = 7778,
                }
            }
        };

        [Description("User permissions")]
        public Dictionary<string, UserModel> Users { get; set; } = new Dictionary<string, UserModel>()
        {
            { "admin@admin", new UserModel()
                {
                     IgnoreMaintenance = true,
                }
            }
        };

        [Description("If maintenance mode is enabled.")]
        public bool MaintenanceMode { get; set; }
        [Description("Name of server visbile on serverlist when maintenance mode is enabled.")]
        public string MaintenanceServerName { get; set; } = "Maintenance mode";
        [Description("Auto updates proxy if needed.")]
        public bool AutoUpdater { get; set; } = true;
    }

    public class ServerModel
    {
        [Description("Name of server.")]
        public string Name { get; set; }

        [Description("IP Address of target server.")]
        public string Ip { get; set; } = "0.0.0.0";

        [Description("IP Address used for connecting to server via providing public ip in .connect command ( for GLOBAL MODERATION, STUDIO STAFF )")]
        public string PublicIp { get; set; } = "0.0.0.0";

        [Description("Port of target server.")]
        public ushort Port { get; set; }

        [Description("Maximum amount of players which can connect to server.")]
        public int MaxPlayers { get; set; } = 20;

        [Description("Connection type set to Proxied will connect players to specific server, simulation needs to have Simulation set to specific type example lobby")]
        public ConnectionType ConnectionType { get; set; } = ConnectionType.Proxied;
        [Description("Simulation set when player connects to server, plugins can register custom ones and you need to specify type here.")]
        public string Simulation { get; set; } = "-";

        [Description("PreAuth will contain IP Address of client and target server will set this ip address to that one only if enable_proxy_ip_passthrough is set to true and trusted_proxies_ip_addresses has your proxy ip!")]
        public bool SendIpAddressInPreAuth { get; set; } = true;
    }

    public class UserModel
    {
        [Description("If player can join when maintenance is enabled.")]
        public bool IgnoreMaintenance { get; set; }
    }
}
