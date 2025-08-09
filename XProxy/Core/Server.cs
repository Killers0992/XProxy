namespace XProxy.Core;

public class Server
{
    public static Dictionary<Type, Server> RegisteredServers = new Dictionary<Type, Server>();

    public static void Register<TServer>(TServer server) where TServer : Server
    {
        if (RegisteredServers.ContainsKey(typeof(TServer)))
        {
            throw new InvalidOperationException($"Server of type {typeof(TServer).Name} is already registered.");
        }

        RegisteredServers[typeof(TServer)] = server;
    }

    public static Server Get<TServer>() where TServer : Server
    {
        if (!RegisteredServers.TryGetValue(typeof(TServer), out Server server))
            return null;

        return server;
    }

    public string Name { get; }
    public string IpAddress { get; }
    public int Port { get; }

    public bool IncludeIpInPreauth { get; private set; }

    public bool IsSimulated { get; private set; }

    public List<BaseClient> Clients { get; } = new List<BaseClient>();

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
    }

    public virtual bool OnClientConnecting(BaseClient client) => false;
    
    public virtual void OnClientConnected(BaseClient client) { }

    public virtual void OnClientReady(BaseClient client) { }
    public virtual void OnClientSpawnPlayer(BaseClient client) { }

    public virtual void OnUpdate() { }

    public Server(string name, string ip, int port, bool isSimulated)
    {
        Name = name;
        IpAddress = ip; 
        Port = port;
        IsSimulated = isSimulated;
    }
}
