using System.ComponentModel;

namespace XProxy.Plugin
{
    public class Config
    {
        [Description("IP of proxy which will be used for making connection.")]
        public string ProxyIP { get; set; } = "localhost";

        [Description("Port of proxy.")]
        public ushort ProxyPort { get; set; } = 7777;

        [Description("Connection key is used for secure connection and only allows connection with correct key.")]
        public string ConnectionKey { get; set; } = "<SECURE KEY>";
    }
}
