namespace XProxy.Networking;

public class BaseListener
{
    public const int PoolingDelayMs = 10;

    private NetManager _manager;
    private EventBasedNetListener _listener;
    private CancellationToken _token;

    public string ListenIpAddress { get; }
    public int ListenPort { get; }

    public Version GameVersion { get; } = new Version(14, 1, 0);

    public List<BaseClient> NotConnectedClients = new List<BaseClient>();
    public Dictionary<int, BaseClient> ClientById = new Dictionary<int, BaseClient>();

    public BaseListener(string listenIp, int listenPort, CancellationToken cancellationToken)
    {
        _token = cancellationToken;

        ListenIpAddress = listenIp;
        ListenPort = listenPort;

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

        _manager.StartInManualMode(IPAddress.Parse(listenIp), IPAddress.IPv6Any, listenPort);

        Task.Run(() => RunEventPolling(_token), _token);    
    }

    Queue<Client> _clientsToRemove = new Queue<Client>();

    private async Task RunEventPolling(CancellationToken token)
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
                if (client.Connection.IsConnected)
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

    private void OnConnectionRequest(ConnectionRequest request)
    {
        string connectionIpAddress = $"{request.RemoteEndPoint.Address}";

        PreAuthResponse response = PreAuthResponse.Valid;
        bool rejectForce = false;
        PreAuth preAuth = default;

        if (!PreAuth.TryRead(this, connectionIpAddress, request.Data, ref response, ref rejectForce, ref preAuth))
        {
            switch (response)
            {
                case PreAuthResponse.VersionNotCompatible:
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

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        if (!ClientById.TryGetValue(peer.Id, out BaseClient client))
            return;

        byte[] bytes = reader.RawData;
        int pos = reader.Position;
        int length = reader.AvailableBytes;

        client.ProcessMirrorDataFromListener(ref bytes, ref pos, ref length);

        client.Connection.Send(bytes, pos, length, deliveryMethod);
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        if (!ClientById.TryGetValue(peer.Id, out BaseClient client))
            return;

        OnClientDisconneted(client);
        client.Dispose();
    }

    public virtual void OnClientConnected(BaseClient client)
    {
    }

    public virtual void OnClientDisconneted(BaseClient client)
    {

    }
}
