using System.ComponentModel;

namespace XProxy.Core.Models
{
    public class PluginExtensionModel
    {
        [Description("ConnectionKey is used for secure connection, plugin needs to send same key to make connection with proxy.")]
        public string ConnectionKey { get; set; } = "<SECURE KEY>";

        [Description("AllowedConnections can limit which ips are allowed to connect to proxy.")]
        public string[] AllowedConnections { get; set; } = new string[0];

        [Description("Normally by default proxy don't have any clue if server is online, by setting this to true proxy will see this server as online if plugin connected to proxy.")]
        public bool UseAccurateOnlineStatus { get; set; } = false;
    }
}
