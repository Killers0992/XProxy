using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace XProxy
{
    public class ProxyConfig
    {
        public Dictionary<int, ProxyData> proxyServers { get; set; } = new Dictionary<int, ProxyData>();
        public string ServerName { get; set; }
        public string ProxyPastebin { get; set; }
        public string GameVersion { get; set; }
        public bool IsPrivateBeta { get; set; }
        public string Email { get; set; }
        public Dictionary<int, ProxyServerData> servers { get; set; } = new Dictionary<int, ProxyServerData>();
    }
                                                     
    public class ProxyServerData
    {
        public string SimpleName { get; set; } = "Server1";
        public string Address { get; set; } = "localhost";
        public int Port { get; set; } = 9999;
        [JsonIgnore]
        public int Players { get; set; }
        public int MaxPlayers { get; set; } = 55;
    }

    public class ProxyData
    {
        public int MaxPlayers { get; set; } = 100;
    }
}
