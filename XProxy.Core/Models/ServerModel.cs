using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XProxy.Enums;
using YamlDotNet.Serialization;

namespace XProxy.Core.Models
{
    public class ServerModel
    {
        [Description("Name of server.")]
        [YamlMember(ScalarStyle = YamlDotNet.Core.ScalarStyle.Literal)]
        public string Name { get; set; }

        [Description("IP Address of target server.")]
        public string Ip { get; set; } = "0.0.0.0";

        [Description("IP Address used for connecting to server via providing public ip in .connect command ( for GLOBAL MODERATION, STUDIO STAFF )")]
        public string PublicIp { get; set; } = "0.0.0.0";

        [Description("Port of target server.")]
        public ushort Port { get; set; }

        [Description("Maximum amount of players which can connect to server.")]
        public int MaxPlayers { get; set; } = 20;

        [Description("Enables queue system for this server.")]
        public bool IsQueueEnabled { get; set; } = false;

        [Description("Maximum amount of players which can be in queue for this server.")]
        public int QueueSlots { get; set; } = 50;

        [Description("Connection type set to Proxied will connect players to specific server, simulation needs to have Simulation set to specific type example lobby")]
        public ConnectionType ConnectionType { get; set; } = ConnectionType.Proxied;
        [Description("Simulation set when player connects to server, plugins can register custom ones and you need to specify type here.")]
        public string Simulation { get; set; } = "-";

        [Description("PreAuth will contain IP Address of client and target server will set this ip address to that one only if enable_proxy_ip_passthrough is set to true and trusted_proxies_ip_addresses has your proxy ip!")]
        public bool SendIpAddressInPreAuth { get; set; } = true;

        [Description("Fallback servers if defined are used for connecting players to online server if current server crashes or shutdowns.")]
        public List<string> FallbackServers { get; set; } = new List<string>() { "lobby" };

        [Description("These settings are related for XProxy.Plugin running on your server.")]
        public PluginExtensionModel PluginExtension { get; set; } = new PluginExtensionModel();
    }
}
