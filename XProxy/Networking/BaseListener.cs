namespace XProxy.Networking;

public class BaseListener
{
    public const int PoolingDelayMs = 10;

    private HttpClient _httpClient;
    private NetManager _manager;
    private EventBasedNetListener _listener;
    private CancellationToken _token;
    private Queue<Client> _clientsToRemove = new Queue<Client>();

    public string Name { get; }

    public string ListenIpAddress { get; }
    public int ListenPort { get; }

    public string PublicIp { get; private set; }

    public string[] Priorities { get; }

    public Version GameVersion { get; }

    public List<BaseClient> NotConnectedClients = new List<BaseClient>();
    public Dictionary<int, BaseClient> ClientById = new Dictionary<int, BaseClient>();

    public string Tag => $"[(f=cyan){ListenIpAddress}:{ListenPort}(f=white)]";

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
    public BaseListener(string name, string listenIp, int listenPort, string gameVersion, string[] priorities, string ip, CancellationToken cancellationToken)
    {
        _token = cancellationToken;

        Name = name;

        ListenIpAddress = listenIp;
        ListenPort = listenPort;

        PublicIp = ip;

        GameVersion = Version.Parse(gameVersion);

        Priorities = priorities;

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

        if (!_manager.StartInManualMode(IPAddress.Parse(listenIp), IPAddress.IPv6Any, listenPort))
        {
            Logger.Info($"{Tag} Failed to start listener!", "Listener");
            return;
        }

        Task.Run(() => RunEventPolling(_token), _token);
    }

    public async Task Initialize()
    {
        if (PublicIp == "auto")
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
            Logger.Error(ex, "ListService");
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
        if (!ClientById.TryGetValue(peer.Id, out BaseClient client))
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
        if (!ClientById.TryGetValue(peer.Id, out BaseClient client))
            return;

        OnClientDisconneted(client, disconnectInfo.Reason);

        client.World = null;

        switch (disconnectInfo.Reason)
        {
            case DisconnectReason.RemoteConnectionClose:
                Logger.Info($"{client.Tag} Client closed the connection!", "Listener");
                break;
            default:
                Logger.Info($"{client.Tag} Disconnected", "Listener");
                break;
        }

        client.Dispose();
    }

    public virtual void OnClientConnected(BaseClient client)
    {
    }

    public virtual void OnClientDisconneted(BaseClient client, DisconnectReason reason)
    {

    }
}