using Logger = XProxy.Misc.Logger;

namespace XProxy.Core;

public class Server
{
    public static Dictionary<Type, Dictionary<string, Server>> RegisteredServers = new Dictionary<Type, Dictionary<string, Server>>();

    public static void Register<TServer>(TServer server) where TServer : Server
    {
        if (!RegisteredServers.ContainsKey(typeof(TServer)))
            RegisteredServers.Add(typeof(TServer), new Dictionary<string, Server>());

        if (!RegisteredServers.TryGetValue(typeof(TServer), out Dictionary<string, Server> servers))
            return;

        string ipAddress = $"{server.IpAddress}:{server.Port}";

        if (servers.ContainsKey(ipAddress))
            return;

        servers.Add(ipAddress, server);
        servers.Add(server.Name.ToLower(), server);
    }

    public static Server Get<TServer>(string name = null, string ip = null, int port = -1) where TServer : Server
    {
        if (!RegisteredServers.TryGetValue(typeof(TServer), out Dictionary<string, Server> servers))
            return null;

        if (servers.Count == 0)
            return null;

        if (!string.IsNullOrEmpty(ip))
        {
            if (servers.TryGetValue($"{ip}:{port}", out Server server))
                return server;
        }
        else if (!string.IsNullOrEmpty(name))
        {
            if (servers.TryGetValue($"{name}", out Server server))
                return server;
        }
        else
        {
            var pair = servers.FirstOrDefault();

            if (pair.Value != null)
                return pair.Value;
        }

        return null;

    }

    public string Name { get; }
    public string IpAddress { get; }
    public int Port { get; }

    public bool IsSimulated { get; private set; }
    public bool ForwardIpAddress { get; private set; }

    public string Tag => $"[(f=yellow){Name.ToLower()}(f=white)]";

    public List<BaseClient> Clients { get; } = new List<BaseClient>();

    public Server(string name, string ip, int port, bool isSimulated, bool forwardIpAddress)
    {
        Name = name;
        IpAddress = ip;
        Port = port;

        IsSimulated = isSimulated;
        ForwardIpAddress = forwardIpAddress;

        Logger.Info($"{Tag} Server registered (f=green){IpAddress}:{Port}(f=white)", "Listener");
    }

    public bool InternalClientConnecting(BaseClient client)
    {
        bool canJoin = OnClientConnecting(client);

        return canJoin;
    }

    public void InternalClientConnected(BaseClient client)
    {
        Clients.Add(client);
        OnClientConnected(client);
    }


    public void InternalClientDisconnected(BaseClient client)
    {
        Clients.Remove(client);
        OnClientDisconnected(client);
    }

    public virtual bool OnClientConnecting(BaseClient client) => false;
    
    public virtual void OnClientConnected(BaseClient client) { }
    public virtual void OnClientDisconnected(BaseClient client) { }

    public virtual void OnClientReady(BaseClient client) { }
    public virtual void OnClientSpawnPlayer(BaseClient client) { }

    public virtual void OnUpdate() { }
}
