namespace XProxy.Core;

/// <summary>
/// Represents a network connection for a client, handling communication and state.
/// </summary>
public class Connection : IDisposable
{
    private NetManager _netManager;
    private EventBasedNetListener _listener;

    /// <summary>
    /// Gets a value indicating whether this is the main connection.
    /// </summary>
    public bool IsMain { get; set; } = true;

    /// <summary>
    /// Gets a value indicating whether the connection is currently established.
    /// </summary>
    public bool IsConnected => IsValid && _netManager.FirstPeer != null || IsConnectedToSimulated;

    /// <summary>
    /// Gets a value indicating whether the NetManager instance is valid (not null).
    /// </summary>
    public bool IsValid => _netManager != null;

    public bool IsConnectedToSimulated { get; set; }

    public bool IsConnecting;

    /// <summary>
    /// Gets or sets the client associated with this connection.
    /// </summary>
    public BaseClient Client { get; private set; }

    /// <summary>
    /// Gets the server this connection is associated with.
    /// </summary>
    public Server Server { get; private set; }

    public ChallengeHandler Challenge { get; set; }

    /// <summary>
    /// Sets up the connection for the specified client.
    /// </summary>
    public void Setup(BaseClient client)
    {
        Challenge = new ChallengeHandler(this);
        Client = client;

        if (_listener == null)
        {
            _listener = new EventBasedNetListener();

            _listener.PeerConnectedEvent += OnConnected;
            _listener.NetworkReceiveEvent += OnReceiveData;
            _listener.PeerDisconnectedEvent += OnDisconnected;
        }

        if (_netManager == null)
        {
            _netManager = new NetManager(_listener)
            {
                UpdateTime = 5,
                ChannelsCount = (byte)6,
                DisconnectTimeout = 1000,
                ReconnectDelay = 300,
                MaxConnectAttempts = 3,
            };

            _netManager.Start();
        }
    }

    /// <summary>
    /// Updates the connection state.
    /// </summary>
    public void Update()
    {
        if (_netManager == null)
            return;

        if (!_netManager.IsRunning)
            return;

        try
        {
            _netManager.PollEvents();
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    /// <summary>
    /// Attempts to establish a connection to the specified server using the provided connection data.
    /// Handles both simulated and real server connections, and manages connection state.
    /// </summary>
    /// <param name="server">The server to connect to.</param>
    /// <param name="connectionData">The data required for the connection.</param>
    /// <param name="reconnect">Indicates whether this is a reconnection attempt.</param>
    public bool TryMakeConnection(Server server, NetDataWriter connectionData, bool reconnect = false)
    {
        if (!IsValid)
            return false;

        if (IsConnecting)
            return false;

        IsConnectedToSimulated = false;

        Server = server;

        Logger.Info($"{Client.Tag} {(reconnect ? "Reconnecting" : "Connecting")} to (f=yellow){server.Name}(f=white)", "Client");

        if (server.IsSimulated)
        {
            bool canJoin = server.InternalClientConnecting(Client);

            if (canJoin)
            {
                AcceptConnection();
                IsConnectedToSimulated = true;
                server.InternalClientConnected(Client);
                return true;
            }

            return false;
        }

        _netManager.Connect(Server.IpAddress, Server.Port, connectionData);
        IsConnecting = true;
        return true;
    }


    /// <summary>
    /// Attempts to reconnect to the current server using the provided connection data.
    /// </summary>
    /// <param name="connectionData">The data required for the connection.</param>
    public void Reconnect(NetDataWriter connectionData)
    {
        if (!IsValid)
            return;

        TryMakeConnection(Server, connectionData, true);
    }

    /// <summary>
    /// Sends data over the connection.
    /// </summary>
    public void Send(byte[] bytes, int position, int length, DeliveryMethod method)
    {
        if (!IsConnected)
            return;

        if (IsConnectedToSimulated)
            return;

        _netManager.FirstPeer.Send(bytes, position, length, method);
    }

    void OnDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        IsConnecting = false;

        switch (disconnectInfo.Reason)
        {
            default:
                Logger.Info($"{Client.Tag} {disconnectInfo.Reason}", "Client");
                break;
            case DisconnectReason.ConnectionFailed when disconnectInfo.AdditionalData.RawData == null:
                if (!IsMain)
                {
                    Client.InvokeConnectionResponse(Server, new ServerIsOfflineResponse());
                    return;
                }

                //Logger.Info(ConfigService.Singleton.Messages.PlayerServerIsOfflineMessage.Replace("%tag%", Owner.Tag).Replace("%address%", $"{Owner.ClientEndPoint}").Replace("%userid%", Owner.UserId), $"Player");
                //Owner.DisconnectFromProxy(ConfigService.Singleton.Messages.ServerIsOfflineKickMessage.Replace("%server%", Owner.CurrentServer.Name));
                Logger.Info($"{Client.Tag} Server (f=yellow){Server.IpAddress}:{Server.Port}(f=white) is offline!", "Client");

                Client.TakeServerAndTryConnect();
                return;

            case DisconnectReason.ConnectionRejected when disconnectInfo.AdditionalData.RawData != null:
                NetDataWriter rejectedData = NetDataWriter.FromBytes(disconnectInfo.AdditionalData.RawData, disconnectInfo.AdditionalData.UserDataOffset, disconnectInfo.AdditionalData.UserDataSize);

                if (!disconnectInfo.AdditionalData.TryGetByte(out byte lastRejectionReason))
                    break;

                RejectionReason reason = (RejectionReason)lastRejectionReason;

                bool cancel = false;

                switch (reason)
                {
                    case RejectionReason.Delay:
                        if (!disconnectInfo.AdditionalData.TryGetByte(out byte offset))
                            break;

                        if (!IsMain)
                        {
                            Client.InvokeConnectionResponse(Server, new DelayConnectionResponse(offset));
                            return;
                        }

                        Logger.Info($"{Client.Tag} Delay connecting to (f=yellow){Server.IpAddress}:{Server.Port}(f=white) by {offset} seconds!", "Client");
                        break;

                    case RejectionReason.ServerFull:
                        if (!IsMain)
                        {
                            Client.InvokeConnectionResponse(Server, new ServerIsFullResponse());
                            return;
                        }

                        Logger.Info($"{Client.Tag} Server (f=yellow){Server.IpAddress}:{Server.Port}(f=white) is full!", "Client");
                        Client.OnDisconnectedFromServerInternal(Server, new ConnectionFailedInfo($"Server {Server.IpAddress}:{Server.Port} is full!", DisconnectType.ServerIsFull));
                        break;

                    case RejectionReason.Banned:
                        long expireTime = disconnectInfo.AdditionalData.GetLong();
                        string banReason = disconnectInfo.AdditionalData.GetString();

                        var date = new DateTime(expireTime, DateTimeKind.Utc).ToLocalTime();

                        if (!IsMain)
                        {
                            Client.InvokeConnectionResponse(Server, new BannedResponse(banReason, date));
                            return;
                        }

                        Logger.Info($"{Client.Tag} Banned from (f=yellow){Server.IpAddress}:{Server.Port}(f=white) with reason (f=yellow){banReason}(f=white)!", "Client");
                        break;

                    case RejectionReason.Challenge:
                        Logger.Info($"{Client.Tag} Processing challenge.", "Client");
                        Challenge.ProcessChallenge(disconnectInfo.AdditionalData);
                        break;

                    default:

                        break;
                }

                if (cancel)
                    return;

                //Client.Disconnect(writer: rejectedData);
                return;

            case DisconnectReason.Timeout:
            case DisconnectReason.PeerNotFound:
                Logger.Info($"{Client.Tag} Timeout!", "Client");
                return;

            case DisconnectReason.RemoteConnectionClose:
                switch (Client.LastResponse)
                {
                    case RoundRestartResponse roundRestart:
                        Client.Reconnect(Server.Name, roundRestart.TimeOffset, "is restarting");
                        return;
                }

                Logger.Info($"{Client.Tag} Remote server closed connection!", "Client");
                Client.Connect("lobby");
                break;
        }
    }

    void OnReceiveData(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
        byte[] bytes = reader.RawData;
        int pos = reader.Position;
        int length = reader.AvailableBytes;

        if (!Client.ProcessMirrorDataFromServer(ref bytes, ref pos, ref length))
        {
            reader.Recycle();
            return;
        }

        Client.SendData(bytes, pos, length, deliveryMethod);

        reader.Recycle();
    }

    void AcceptConnection()
    {
        IsConnecting = false;
        Client.AcceptConnection();

        Client.Connection = this;

        if (Client.Object != null)
        {
            Client.SetRole(PlayerRoles.RoleTypeId.Destroyed);

            Logger.Info("Send FastRoundRestart packet");
            Client.FastRoundrestart();
            Logger.Info("Send NotReady packet");
            Client.NotReady();
        }
    }

    void OnConnected(NetPeer peer)
    {
        AcceptConnection();
        Server.InternalClientConnected(Client);
    }

    /// <summary>
    /// Disconnects the connection.
    /// </summary>
    public void Disconnect()
    {
        if (_netManager == null)
            return;

        if (!IsConnected)
            return;

        Server?.InternalClientDisconnected(Client);

        if (!IsConnectedToSimulated)
            _netManager.FirstPeer.Disconnect();

        _netManager?.Stop();
        _netManager = null;

        switch (Client.LastResponse)
        {
            case RoundRestartResponse roundRestart:
                Client.Reconnect(Server.Name, roundRestart.TimeOffset, "is restarting");
                return;
        }
    }

    /// <summary>
    /// Disposes the connection and releases all resources.
    /// </summary>
    public void Dispose()
    {
        Disconnect();

        if (_listener != null)
        {
            _listener.PeerConnectedEvent -= OnConnected;
            _listener.NetworkReceiveEvent -= OnReceiveData;
            _listener.PeerDisconnectedEvent -= OnDisconnected;

            _listener = null;
        }
    }
}
