namespace XProxy.API.Networking;

public class Listener
{
    public const int PoolingDelayMs = 10;

    public static List<Listener> List => ListenersByName.Values.ToList();
    public static Dictionary<string, Listener> ListenersByName { get; } = new Dictionary<string, Listener>();

    public static bool TryGet(string name, out Listener listener) => ListenersByName.TryGetValue(name, out listener);

    public static void Register(Listener listener)
    {
        ListenersByName.Add(listener.Name, listener);
    }

    private string _version;
    private Version _parsedGameVersion;

    private HttpClient _httpClient;
    private NetManager _manager;
    private EventBasedNetListener _listener;
    private CancellationToken _token;
    private Queue<Client> _clientsToRemove = new Queue<Client>();

    public ListenerSettings Settings { get; }

    public string Name => Settings.Name;

    public string ListenAddress => Settings.ListenAddress;

    public int ListenPort => Settings.ListenPort;

    public string PublicIp { get; private set; }

    public string[] Priorities => Settings.Priorities;

    public Version GameVersion
    {
        get
        {
            if (_version != Settings.GameVersion)
            {
                _parsedGameVersion = Version.Parse(Settings.GameVersion);
                _version = Settings.GameVersion;
            }

            return _parsedGameVersion;
        }
    }

    public List<Client> NotConnectedClients = new List<Client>();
    public Dictionary<int, Client> ClientById = new Dictionary<int, Client>();

    public string Tag => $"[(f=cyan){Name}(f=white)]";

    public HttpClient Http
    {
        get
        {
            if (_httpClient == null)
            {
                _httpClient = new HttpClient();
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "SCP SL");
                _httpClient.DefaultRequestHeaders.Add("Game-Version", GameVersion.ToString(3));
            }

            return _httpClient;
        }
    }


    public bool ForceServerListUpdate;

    public bool ServerListUpdate;
    public int ServerListCycle;

    public Listener(ListenerSettings settings, CancellationToken cancellationToken)
    {
        Settings = settings;

        _token = cancellationToken;

        _listener = new EventBasedNetListener();
        _listener.ConnectionRequestEvent += OnConnectionRequest;
        _listener.NetworkReceiveEvent += OnNetworkReceive;
        _listener.PeerDisconnectedEvent += OnPeerDisconnected;

        _manager = new NetManager(_listener)
        {
            UpdateTime = 5,
            BroadcastReceiveEnabled = true,
            ChannelsCount = (byte)6,
            DisconnectTimeout = 6000,
            ReconnectDelay = 400,
            MaxConnectAttempts = 2,
        };

        if (!_manager.StartInManualMode(IPAddress.Parse(ListenAddress), IPAddress.IPv6Any, ListenPort))
        {
            ProxyLogger.Info($"{Tag} Failed to start listener!", "Listener");
            return;
        }

        Task.Run(() => RunEventPolling(_token), _token);

        ProxyLogger.Info($"{Tag} Listening on (f=green){ListenAddress}:{ListenPort}(f=white)", "Listener");
    }

    public async Task Initialize()
    {
        PublicIp = await GetPublicIp();
    }

    async Task<string> GetPublicIp()
    {
        try
        {
            using (var response = await Http.GetAsync("https://api.scpslgame.com/ip.php"))
            {
                string str = await response.Content.ReadAsStringAsync();

                str = (str.EndsWith(".") ? str.Remove(str.Length - 1) : str);

                return str;
            }
        }
        catch (Exception ex)
        {
            ProxyLogger.Error(ex, "ListService");
            return null;
        }
    }

    async Task RunEventPolling(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                _manager.PollEvents();
                _manager.ManualUpdate(PoolingDelayMs);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            foreach (Client client in NotConnectedClients)
            {
                if (client.Connection.IsConnected || client.IsDisposing)
                {
                    ProxyLogger.Info("Dispose not connected client " + client.PreAuth.UserId);
                    _clientsToRemove.Enqueue(client);
                    continue;
                }

                try
                {
                    client.PollEvents();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            while(_clientsToRemove.Count > 0)
            {
                NotConnectedClients.Remove(_clientsToRemove.Dequeue());
            }

            foreach (Client client in ClientById.Values)
            {
                try
                {
                    client.PollEvents();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            await Task.Delay(PoolingDelayMs, token);
        }

    }

    void OnConnectionRequest(ConnectionRequest request)
    {
        string connectionIpAddress = $"{request.RemoteEndPoint.Address}";

        DisconnectType response = DisconnectType.Valid;
        bool rejectForce = false;
        PreAuth preAuth = default;

        if (!PreAuth.TryRead(this, connectionIpAddress, request.Data, ref response, ref rejectForce, ref preAuth))
        {
            switch (response)
            {
                case DisconnectType.VersionNotCompatible:
                    NetDataWriter writer = new NetDataWriter();
                    writer.Put((byte)RejectionReason.VersionMismatch);
                    request.RejectForce(writer);
                    break;

                default:
                    request.RejectForce();
                    break;
            }
            return;
        }

        OnClientConnected(new Client(this, request, preAuth));
    }

    void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        if (!ClientById.TryGetValue(peer.Id, out Client client))
            return;

        byte[] bytes = reader.RawData;
        int pos = reader.Position;
        int length = reader.AvailableBytes;

        if (!client.ProcessMirrorDataFromListener(ref bytes, ref pos, ref length))
            return;

        client.Connection.Send(bytes, pos, length, deliveryMethod);
    }

    void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        if (!ClientById.TryGetValue(peer.Id, out Client client))
            return;

        OnClientDisconneted(client, disconnectInfo.Reason);

        client.World = null;

        switch (disconnectInfo.Reason)
        {
            case DisconnectReason.RemoteConnectionClose:
                ProxyLogger.Info($"{client.Tag} Client closed the connection!", "Listener");
                break;
            default:
                ProxyLogger.Info($"{client.Tag} Disconnected", "Listener");
                break;
        }

        ProxyLogger.Info("Dispose peer");

        client.Dispose();
    }

    public virtual void OnClientConnected(Client client)
    {
        ProxyLogger.Info($"{client.Tag} Connected", "Listener");

        client.Connect(Priorities);
    }

    public virtual void OnClientDisconneted(Client client, DisconnectReason reason)
    {

    }
}