namespace XProxy.API.Core;

public class Server
{
    public static Dictionary<string, Server> RegisteredServers = new Dictionary<string, Server>();
    public static List<Server> List { get; set; } = new List<Server>();

    public static void Register<TServer>(TServer server) where TServer : Server
    {
        string ipAddress = $"{server.IpAddress}:{server.Port}";

        if (RegisteredServers.ContainsKey(ipAddress))
            return;

        RegisteredServers.Add(ipAddress, server);
        RegisteredServers.Add(server.Name.ToLower(), server);
        List.Add(server);
    }

    public static Server Get<TServer>(string name = null, string ip = null, int port = -1) where TServer : Server
    {
        if (!string.IsNullOrEmpty(ip))
        {
            if (RegisteredServers.TryGetValue($"{ip}:{port}", out Server server))
                return server;
        }
        else if (!string.IsNullOrEmpty(name))
        {
            if (RegisteredServers.TryGetValue($"{name.ToLower()}", out Server server))
                return server;
        }
        else
        {
            var pair = RegisteredServers.FirstOrDefault();

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

    public List<Client> Clients { get; } = new List<Client>();

    public Server(string name, string ip, int port, bool isSimulated, bool forwardIpAddress)
    {
        Name = name;
        IpAddress = ip;
        Port = port;

        IsSimulated = isSimulated;
        ForwardIpAddress = forwardIpAddress;

        ProxyLogger.Info($"{Tag} Server registered (f=green){IpAddress}:{Port}(f=white)", "Listener");
    }

    public bool InternalClientConnecting(Client client)
    {
        bool canJoin = OnClientConnecting(client);

        return canJoin;
    }

    public void InternalClientConnected(Client client)
    {
        Clients.Add(client);
        OnClientConnected(client);
    }


    public void InternalClientDisconnected(Client client)
    {
        Clients.Remove(client);
        OnClientDisconnected(client);
    }

    public virtual bool OnClientConnecting(Client client) => false;
    
    public virtual void OnClientConnected(Client client) { }
    public virtual void OnClientDisconnected(Client client) { }

    public virtual void OnClientReady(Client client) { }
    public virtual void OnClientSpawnPlayer(Client client) { }

    public virtual void OnClientSpawned(Client client) { }

    public virtual void OnClientSSSReponse(Client client, int id) { }

    public virtual void OnUpdate() { }

    public override string ToString() => $"{IpAddress}:{Port}";
}
