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
        public List<string> BlockUserids { get; set; } = new List<string>();
    }
                                                     
    public class ProxyData
    {
        public string TargetIP { get; set; }
        public int TargetPort { get; set; }
        public int MaxPlayers { get; set; } = 100;
    }
}
