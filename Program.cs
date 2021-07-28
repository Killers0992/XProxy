using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Threading;
using System.IO;
using Newtonsoft.Json;

namespace XProxy
{
    class Program
    {
        private static ProxyConfig config;
        private static ProxyServer server;
        static void Main(string[] args)
        {
            if (!File.Exists($"./config.json"))
                File.WriteAllText("./config.json", JsonConvert.SerializeObject(new ProxyConfig(), Formatting.Indented));
            config = JsonConvert.DeserializeObject<ProxyConfig>(File.ReadAllText("./config.json"));
            server = new ProxyServer(config);
            Console.ReadKey();
        }
    }
}