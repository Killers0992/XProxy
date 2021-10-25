using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Threading;
using System.IO;
using Newtonsoft.Json;
using XProxy.ServerList;
using System.Linq;

namespace XProxy
{
    class Program
    {
        public static ProxyConfig config;
        public static ProxyServer server;
        public static ServerConsole sconsole;

        static List<ProxyServer> servers = new List<ProxyServer>();

        static void Main(string[] args)
        {
            if (!File.Exists($"./config.json"))
                File.WriteAllText("./config.json", JsonConvert.SerializeObject(new ProxyConfig(), Formatting.Indented));
            if (!File.Exists("./centralcache.txt"))
                File.WriteAllText($"./centralcache.txt", "");
            if (!File.Exists("./centralkeysignature.txt"))
                File.WriteAllText($"./centralkeysignature.txt", "");
            config = JsonConvert.DeserializeObject<ProxyConfig>(File.ReadAllText("./config.json"));
            File.WriteAllText("./config.json", JsonConvert.SerializeObject(config, Formatting.Indented));
            foreach(var server in config.proxyServers)
            {
                servers.Add(new ProxyServer(config, server.Key, server.Value.targetServerID));
            }
            while (true)
            {
                var line = Console.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    var cmdArgs = line.Split(' ');
                    switch (cmdArgs[0].ToUpper())
                    {
                        case "CTARGET":
                            server.RedirectAllClients(cmdArgs[1], int.Parse(cmdArgs[2]));
                            break;
                    }
                    foreach(var server in servers)
                    {
                        server.serverlist.RunCentralServerCommand(cmdArgs[0], string.Join(" ", cmdArgs.Skip(1)));
                    }
                }
            }
        }
    }
}