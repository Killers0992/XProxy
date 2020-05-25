using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using XProxy.ServerList;
using XProxy.Proxy;
using Utf8Json;

namespace XProxy
{
    class Program
    {
        static ServerConsole sc;
        static ServerStatus status;
        static ProxyConfig config;
        public static Server main_server;



        static void Main(string[] args)
        {
            sc = new ServerConsole();
            sc.RunServer();
            try
            {
                var configJson = System.IO.File.ReadAllText("config.json");

                config = JsonSerializer.Deserialize<ProxyConfig>(configJson);

                foreach(var server in config.servers)
                {
                    Server srv = server.Value;
                    srv.players_online = 0;
                    status.servers.Add(server.Key, srv);
                }

                if (config.servers.ContainsKey(config.main_server))
                    main_server = config.servers[config.main_server];

                Task.Run(() =>
                {
                    try
                    {
                        var proxy = new UdpProxy();
                        return proxy.Start(config.port);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to start : {ex.Message}");
                        throw ex;
                    }
                }).Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occured : {ex}");
            }
        }
    }

    public class ServerStatus
    {
        public Dictionary<string, Server> servers { get; set; } = new Dictionary<string, Server>();
    }

    public class ProxyConfig
    {
        public int player_limit { get; set; } = -1;
        public string ip { get; set; } = "localhost";
        public ushort port { get; set; } = 25565;
        public string main_server { get; set; } = "Unknown";
        public Dictionary<string, Server> servers { get; set; } = new Dictionary<string, Server>();
    }

    public class Server
    {
        public string server_name { get; set; } = "Unknowns server";
        public ushort port { get; set; } = 7777;
        public int players_online { get; set; }
    }
}