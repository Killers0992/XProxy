using System;
using System.Collections.Generic;
using System.Text;

namespace XProxy
{
    public class ProxyConfig
    {
        public int proxyPort { get; set; } = 7887;
        public int mainServerID { get; set; } = 0;
        public Dictionary<int, ProxyServerData> servers { get; set; } = new Dictionary<int, ProxyServerData>() { { 0, new ProxyServerData() } };
    }

    public class ProxyServerData
    {
        public string Address { get; set; } = "localhost";
        public int Port { get; set; } = 9999;
    }
}
