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
using Mirror;

namespace XProxy
{
    class Program
    {
        public static ProxyConfig config;
        public static ProxyServer server;
        public static XProxy.ServerList.ServerConsole sconsole;

        static List<ProxyServer> servers = new List<ProxyServer>();

        public static bool ShowDebugLogsFromServer = false;

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
                servers.Add(new ProxyServer(config, server.Key));
            }
            while (true)
            {
                var line = Console.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    foreach(var srv in servers)
                    {
                        srv.serverlist.RunCentralServerCommand(line.Split(' ')[0], string.Join(" ", line.Split(' ').Skip(1)));
                    }
                    var cmdArgs = line.Split(' ');
                    switch (cmdArgs[0].ToUpper())
                    {
                        case "METHODHASH":
                            Console.WriteLine($"{cmdArgs[1].GetStableHashCode() * 503 + cmdArgs[2].GetStableHashCode()}");
                            break;
                        case "SHOWDEBUG":
                            ShowDebugLogsFromServer = !ShowDebugLogsFromServer;
                            Console.WriteLine($"Show debuglogs from server is set to {ShowDebugLogsFromServer}.");
                            break;
                    }
                }
            }
        }
    }
}