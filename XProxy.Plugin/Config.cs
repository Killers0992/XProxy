using System.ComponentModel;

namespace XProxy.Plugin
{
    public class Config
    {
        [Description("IP of proxy which will be used for making connection.")]
        public string ProxyIP { get; set; } = "127.0.0.1";

        [Description("Port of proxy.")]
        public ushort ProxyPort { get; set; } = 7777;

        [Description("Connection key is used for secure connection and only allows connection with correct key.")]
        public string ConnectionKey { get; set; } = "<SECURE KEY>";

        [Description("Should plugin only allow player connections from proxy.")]
        public bool OnlyAllowProxyConnections { get; set; } = false;

        [Description("Rejection message when player is not connecting from proxy.")]
        public string RejectionMessage { get; set; } = "You are not connecting from proxy. Use %ProxyIP%:%ProxyPort% instead!";
    }
}
