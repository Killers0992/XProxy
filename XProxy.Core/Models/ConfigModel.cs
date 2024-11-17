using System.Collections.Generic;
using System.ComponentModel;
using XProxy.Core.Models;
using XProxy.Enums;
using YamlDotNet.Serialization;

namespace XProxy.Models
{
    public class ConfigModel
    {
        [Description("Enables debug logs.")]
        public bool Debug { get; set; }

        [Description("If set to true updater will not try to update XProxy on startup.")]
        public bool DisableUpdater { get; set; } = false;

        [Description("For which version of game XProxy will be downloaded")]
        public string GameVersion { get; set; } = "latest";

        [Description("Language of messages.")]
        public string Langauge { get; set; } = "en";

        [Description("Email used for listing your server on SCP SL serverlist.")]
        public string Email { get; set; } = "example@gmail.com";

        [Description("All listeners which will host own proxy instance.")]
        public Dictionary<string, ListenerServer> Listeners { get; set; } = new Dictionary<string, ListenerServer>()
        {
            { "main", new ListenerServer() }
        };

        [Description("Northwood staff ignores maximum amount of players which can connect to proxy.")]
        public bool NorthwoodStaffIgnoresSlots { get; set; } = false;

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
        [YamlMember(ScalarStyle = YamlDotNet.Core.ScalarStyle.Literal)]
        public string MaintenanceServerName { get; set; } = 
@"Example server name - Maintenance
Proxy Server";

        [Description("Player will be added to queue for first server from priorities if its full.")]
        public bool AutoJoinQueueInLobby { get; set; } = false;
        [Description("For how long queue slot will be valid for player upon joining target server. ( started connecting to target server >-( TIME IN SECONDS )-> connected to target server ) ")]
        public int QueueTicketLifetime { get; set; } = 15;

        public bool TryGetServer(string serverName, out ServerModel serverModel)
        {
            if (Servers.TryGetValue(serverName, out serverModel))
                return true;

            serverModel = null;
            return false;
        }
    }
}
