using Org.BouncyCastle.Asn1.Ocsp;
using XProxy.Responses;

namespace XProxy.Core;

public class Connection : IDisposable
{
    private ChallengeHandler _validator;
    private NetManager _netManager;
    private EventBasedNetListener _listener;

    public Client Client { get; private set; }

    public bool IsValid => _netManager != null;

    public bool IsMain = true;

    public bool IsConnected => IsValid && _netManager.FirstPeer != null;

    public bool IsConnecting;

    public Server Server;

    public ChallengeHandler Challenge;

    public void Setup(Client client)
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

    public void TryMakeConnection(Server server, NetDataWriter connectionData, bool reconnect = false)
    {
        if (!IsValid)
            return;

        if (IsConnecting)
            return;

        Server = server;

        Console.WriteLine($"{Client.PlayerTag} {(reconnect ? "Reconnect" : "Connect")} to {server.IpAddress}:{server.Port}");
        _netManager.Connect(Server.IpAddress, Server.Port, connectionData);
        IsConnecting = true;
    }

    public void Reconnect(NetDataWriter connectionData)
    {
        if (!IsValid)
            return;

        TryMakeConnection(Server, connectionData, true);
    }

    public void Send(byte[] bytes, int position, int length, DeliveryMethod method)
    {
        if (!IsConnected)
            return;

        _netManager.FirstPeer.Send(bytes, position, length, method);
    }

    private void OnDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        IsConnecting = false;

        switch (disconnectInfo.Reason)
        {
            case DisconnectReason.ConnectionFailed when disconnectInfo.AdditionalData.RawData == null:
                if (!IsMain)
                {
                    Client.OnConnectionResponse(Server, new ServerIsOfflineResponse());
                    return;
                }

                //Logger.Info(ConfigService.Singleton.Messages.PlayerServerIsOfflineMessage.Replace("%tag%", Owner.Tag).Replace("%address%", $"{Owner.ClientEndPoint}").Replace("%userid%", Owner.UserId), $"Player");
                //Owner.DisconnectFromProxy(ConfigService.Singleton.Messages.ServerIsOfflineKickMessage.Replace("%server%", Owner.CurrentServer.Name));
                Client.Disconnect();
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
                        {
                            break;
                            //Owner.SaveCurrentServerForNextSession(offset + 10f);
                            //Logger.Info(ConfigService.Singleton.Messages.PlayerDelayedConnectionMessage.Replace("%tag%", Owner.Tag).Replace("%address%", $"{Owner.ClientEndPoint}").Replace("%userid%", Owner.UserId).Replace("%time%", $"{offset}"), $"Player");
                        }

                        Console.WriteLine($"{Client.PlayerTag} Delay connecting by {offset} seconds!");
                        break;

                    case RejectionReason.ServerFull:
                        if (!IsMain)
                        {
                            Client.OnConnectionResponse(Server, new ServerIsFullResponse());
                            return;
                        }

                        Console.WriteLine($"{Client.PlayerTag} Server is full!");
                        Client.Disconnect($"Server {Server.IpAddress}:{Server.Port} is full!");
                        break;

                    case RejectionReason.Banned:
                        long expireTime = disconnectInfo.AdditionalData.GetLong();
                        string banReason = disconnectInfo.AdditionalData.GetString();

                        var date = new DateTime(expireTime, DateTimeKind.Utc).ToLocalTime();

                        if (!IsMain)
                        {
                            Client.OnConnectionResponse(Server, new BannedResponse(banReason, date));
                            return;
                        }

                        Console.WriteLine($"{Client.PlayerTag} Banned for reason {banReason}!");
                        break;

                    case RejectionReason.Challenge:
                        Console.WriteLine($"{Client.PlayerTag} Process challenge.");
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
                Console.WriteLine($"[{Client.PreAuth.UserId}] [{Server.IpAddress}:{Server.Port}] Timeout!");
                return;

            case DisconnectReason.RemoteConnectionClose:
                Console.WriteLine($"[{Client.PreAuth.UserId}] [{Server.IpAddress}:{Server.Port}] Connection closed!");
                break;
        }
    }

    private void OnReceiveData(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
        byte[] bytes = reader.RawData;
        int pos = reader.Position;
        int length = reader.AvailableBytes;

        Client.ProcessMirrorDataFromServer(ref bytes, ref pos, ref length);

        Client.SendData(bytes, pos, length, deliveryMethod);

        reader.Recycle();
    }

    private void OnConnected(NetPeer peer)
    {
        try
        {
            IsConnecting = false;

            if (Client.Request != null)
            {
                Client.Peer = Client.Request.Accept();
                Client.Listener.ClientById.Add(Client.Peer.Id, Client);

                Client.Request = null;
            }

            if (!Client.Connection.IsConnected)
            {
                Client.Server = Server;
                Client.Connection = this;
                Console.WriteLine($"{Client.PlayerTag} connected");
                return;
            }

            if (!IsMain)
            {
                Client.FastRoundrestart();
                Client.NotReady();

                Client.Connection = this;
                Client.Server = Server;
                return;
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex);
        }

    }

    public void Dispose()
    {
        if (_listener != null)
        {
            _listener.PeerConnectedEvent -= OnConnected;
            _listener.NetworkReceiveEvent -= OnReceiveData;
            _listener.PeerDisconnectedEvent -= OnDisconnected;

            _listener = null;
        }

        if (_netManager != null)
        {
            if (IsConnected)
                _netManager.FirstPeer.Disconnect();

            _netManager?.Stop();
            _netManager = null;
        }
    }
}
