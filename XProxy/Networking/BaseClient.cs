using Hints;
using Mirror;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.FirstPersonControl.NetworkMessages;
using RelativePositioning;
using UnityEngine.SceneManagement;
using XProxy.Objects;
using static MapGeneration.SeedSynchronizer;
using static PlayerStatsSystem.SyncedStatMessages;

namespace XProxy.Networking;

public class BaseClient : IDisposable
{
    private Connection _connection = new Connection();
    private uint _nextId = 0;

    public uint NetworkIdentityId { get; private set; }

    public uint NextId => _nextId++;

    public bool IsReady { get; private set; }

    public Server Server => Connection.Server;

    public ConnectionRequest Request { get; set; }

    public BaseListener Listener { get; }
    public NetPeer Peer { get; set; } = null;
    public PreAuth PreAuth { get; }
    public double ListenerRemoteTimestamp { get; private set; }
    public bool IsDisposing { get; private set; }

    public RelativePosition Position;

    public Connection Connection
    {
        get => _connection;
        set
        {
            if (_connection != null)
            {
                _connection.Dispose();
                _connection.IsMain = false;
                BackupConnection = _connection;
            }

            if (value != null)
            {
                IsReady = false;
                value.IsMain = true;
            }

            _connection = value;
            OnConnectedToServerInternal(Server);
        }
    }

    public Connection BackupConnection { get; private set; } = new Connection();

    public CustomUnbatcher UnbatcherCurrentServer { get; private set; } = new CustomUnbatcher();
    public CustomUnbatcher UnbatcherListener { get; private set; } = new CustomUnbatcher();

    public CustomBatcher Batcher { get; private set; } = new CustomBatcher(65535 * (NetConstants.MaxPacketSize - 6));

    public DateTime ConnectedOn { get; } = DateTime.Now;
    public TimeSpan Connectiontime => DateTime.Now - ConnectedOn;

    public Dictionary<ushort, Type> Types = ProxyUtils.FindNetworkMessageTypes();

    public string PlayerTag => $"[(f=cyan){Listener.ListenIpAddress}:{Listener.ListenPort}(f=white)] [(f=green){PreAuth.UserId}(f=white)]{(Server == null ? string.Empty : $" [(f=yellow){Server.Name}(f=white)]")}";

    public BaseClient(BaseListener listener, ConnectionRequest request, PreAuth preAuth)
    {
        Listener = listener;
        Request = request;
        PreAuth = preAuth;

        Listener.NotConnectedClients.Add(this);
    }

    public virtual void OnConnectedToServer(Server Server)
    {

    }

    public virtual bool OnDisconnectedFromServer(Server Server, ConnectionFailedInfo info) => true;

    public void OnConnectedToServerInternal(Server server)
    {
        OnConnectedToServer(server);
    }

    public void OnDisconnectedFromServerInternal(Server server, ConnectionFailedInfo info)
    {
        bool canRun = OnDisconnectedFromServer(server, info);

        if (canRun)
            Disconnect(info.Message);
    }

    public void AcceptConnection()
    {
        if (Request == null)
            return;

        Peer = Request.Accept();
        Listener.ClientById.Add(Peer.Id, this);

        Request = null;
    }

    NetworkWriter batchWriter = new NetworkWriter();

    public void PollEvents()
    {
        Connection.Update();
        BackupConnection.Update();

        while (Batcher.GetBatch(batchWriter))
        {
            ArraySegment<byte> segment = batchWriter.ToArraySegment();
            SendData(segment.Array, segment.Offset, segment.Count, DeliveryMethod.ReliableOrdered);
            batchWriter.Position = 0;
        }
    }

    public bool ProcessMirrorDataFromServer(ref byte[] bytes, ref int position, ref int length)
    {
        ArraySegment<byte> segment = new ArraySegment<byte>(bytes, position, length);

        NetworkReader reader = new NetworkReader(segment);

        double timeStamp = reader.ReadDouble();

        bool end = false;
        int totalReads = 0;

        List<(int, int)> rangesToRemove = new List<(int, int)>();

        while (reader.Remaining != 0 && !end)
        {
            int positionBeforeRead = reader.Position;
            int size = (int)Compression.DecompressVarUInt(reader);

            if (reader.Remaining < size)
            {
                end = true;
                continue;
            }

            ArraySegment<byte> message = reader.ReadBytesSegment(size);

            int positionAfterRead = reader.Position;

            NetworkReader reader2 = new NetworkReader(message);

            if (NetworkMessages.UnpackId(reader2, out ushort messageId))
            {
                if (!ProcessMirrorMessageFromServer(messageId, reader2))
                    rangesToRemove.Add((positionBeforeRead + 1, positionAfterRead));
            }
            totalReads++;
        }

        RemoveByteRanges(ref bytes, rangesToRemove);

        return true;
    }

    static void RemoveByteRanges(ref byte[] input, List<(int start, int end)> ranges)
    {
        List<byte> result = new List<byte>();
        int currentIndex = 0;

        ranges.Sort((a, b) => a.start.CompareTo(b.start));

        foreach (var (start, end) in ranges)
        {
            if (currentIndex < start)
            {
                result.AddRange(input.Skip(currentIndex).Take(start - currentIndex));
            }
            currentIndex = Math.Max(currentIndex, end + 1);
        }

        if (currentIndex < input.Length)
        {
            result.AddRange(input.Skip(currentIndex));
        }

        input = result.ToArray();
    }

    public bool ProcessMirrorDataFromListener(ref byte[] bytes, ref int position, ref int length)
    {
        ArraySegment<byte> segment = new ArraySegment<byte>(bytes, position, length);

        NetworkReader reader = new NetworkReader(segment);

        double timeStamp = reader.ReadDouble();

        bool end = false;
        int totalReads = 0;

        //Console.WriteLine($" Client -> Listener ");
        //Console.WriteLine("> READ START ");
        while (reader.Remaining != 0 && !end)
        {
            int positionBeforeRead = reader.Position;
            int size = (int)Compression.DecompressVarUInt(reader);

            if (reader.Remaining < size)
            {
                end = true;
                continue;
            }

            ArraySegment<byte> message = reader.ReadBytesSegment(size);
            int positionAfterRead = reader.Position;

            NetworkReader reader2 = new NetworkReader(message);

            if (NetworkMessages.UnpackId(reader2, out ushort messageId))
            {
                if (ProcessMirrorMessageFromListener(messageId, reader2))
                {
                }
            }
            totalReads++;
        }

        //Console.WriteLine($"> READ END, Total {totalReads} ");
        return true;
    }

    // Returning true will cancel that message.
    public bool ProcessMirrorMessageFromListener(ushort id, NetworkReader reader)
    {
        string name = Types[id].FullName;
        switch (name)
        {
            // Ignore these messages.
            case "PlayerRoles.FirstPersonControl.NetworkMessages.FpcFromClientMessage":
                byte code = reader.ReadByte();

                bool _bitMouseLook = false;
                bool _bitPosition = false;
                bool _bitCustom = false;

                ushort _rotH, _rotV;

                global::Misc.ByteToBools(code, out bool b1, out bool b2, out bool b3, out bool b4, out bool b5, out _bitMouseLook, out _bitPosition, out _bitCustom);

                PlayerMovementState _state = (PlayerMovementState)global::Misc.BoolsToByte(b1, b2, b3, b4, b5);

                if (_bitPosition)
                {
                    byte WaypointId = reader.ReadByte();
                    short PositionX, PositionY, PositionZ;
                    if (WaypointId > 0)
                    {
                        PositionX = reader.ReadShort();
                        PositionY = reader.ReadShort();
                        PositionZ = reader.ReadShort();

                        Logger.Info(PositionX + " " + PositionY + " " + PositionZ);
                    }
                    else
                    {
                        PositionX = 0;
                        PositionY = 0;
                        PositionZ = 0;
                    }


                }
                
                if (_bitMouseLook)
                {
                    _rotH = reader.ReadUShort();
                    _rotV = reader.ReadUShort();
                }
                else
                {
                    _rotH = 0;
                    _rotV = 0;
                }
                //Logger.Info($"Code {code}, State {_state}, BitPos {_bitPosition}, RotH {_rotH}, RotV {_rotV}");
                break;
            case "Mirror.NetworkPingMessage":
            case "Mirror.TimeSnapshotMessage":
                break;
            case "Mirror.ReadyMessage":
                Server?.OnClientReady(this);
                break;

            case "Mirror.AddPlayerMessage":
                Server?.OnClientSpawnPlayer(this);
                break;

            default:
                Console.WriteLine($"FROM CLIENT -> " + name);
                break;
        }

        return true;
    }

    public bool ProcessMirrorMessageFromServer(ushort id, NetworkReader reader)
    {
        string name = Types[id].FullName;
        switch (name)
        {
            // Ignore these messages.
            case "Mirror.RpcMessage":
            case "Mirror.EntityStateMessage":
            case "Mirror.TimeSnapshotMessage":

            case "PlayerRoles.Subroutines.SubroutineMessage":
            case "PlayerRoles.FirstPersonControl.NetworkMessages.FpcPositionMessage":

            case "InventorySystem.Items.Autosync.AutosyncMessage":
            case "InventorySystem.Items.Firearms.Ammo.ReserveAmmoSync+ReserveAmmoMessage":

            case "VoiceChat.Networking.VoiceMessage":

            case "PlayerStatsSystem.SyncedStatMessages+StatMessage":
                break;

            case "Mirror.SpawnMessage":
                uint netid = reader.ReadUInt();
                bool isLocalPlayer = reader.ReadBool();
                bool isOwner = reader.ReadBool();
                ulong sceneId = reader.ReadULong();
                uint assetId = reader.ReadUInt();

                switch (assetId)
                {
                    // Player
                    case 3816198336:
                        if (isLocalPlayer && isOwner)
                            NetworkIdentityId = netid;
                        break;
                }
                break;

            default:
                Console.WriteLine($"FROM SERVER -> " + name);
                break;
        }

        return true;
    }

    public void Connect<TServer>() where TServer : Server
    {
        Server server = Server.Get<TServer>();

        if (server == null)
        {
            Disconnect("Server not found.");
            return;
        }

        Connect(server);
    }

    public void Connect<TServer>(TServer server) where TServer : Server
    {
        if (Connection.IsConnected && server == Server)
            return;

        BackupConnection.Setup(this);
        BackupConnection.TryMakeConnection(server, PreAuth.Create(server.IncludeIpInPreauth));
    }

    public void SendData(byte[] bytes, int position, int length, DeliveryMethod method)
    {
        if (Peer == null)
            return;

        Peer.Send(bytes, position, length, method);
    }

    public void SendMirrorData(NetworkWriter writer)
    {
        if (Batcher == null)
            return;

        Batcher.AddMessage(writer.ToArraySegment(), Connectiontime.TotalSeconds);

        writer = null;
    }

    public void SendMirrorData<TMessage>() where TMessage : struct, NetworkMessage
    {
        NetworkWriter writer = new NetworkWriter();
        writer.WriteUShort(NetworkMessageId<TMessage>.Id);
        SendMirrorData(writer);
    }

    public void SendHint(string message, float duration = 3)
    {
        //message = PlaceHolders.ReplacePlaceholders(message);

        NetworkWriter wr = new NetworkWriter();

        wr.WriteUShort(NetworkMessageId<HintMessage>.Id);

        var hint = new TextHint(message, new HintParameter[] {
                        new StringHintParameter(message) }, null, duration);

        //TextHint
        wr.WriteByte(1);
        wr.Serialize(hint);

        SendMirrorData(wr);
    }

    public void SpawnObjects()
    {
        SendMirrorData<ObjectSpawnStartedMessage>();
        SendMirrorData<ObjectSpawnFinishedMessage>();
    }

    public void SetRole(RoleTypeId role)
    {
        NetworkWriter wr = new NetworkWriter();
        wr.WriteUShort(NetworkMessageId<RoleSyncInfo>.Id);

        wr.WriteUInt(NetworkIdentityId);
        wr.WriteSByte((sbyte)role);
        wr.WriteRelativePosition(new RelativePosition(new UnityEngine.Vector3(0f, 0f, 0f)));
        wr.WriteUShort(0);

        SendMirrorData(wr);
    }

    public void Spawn()
    {
        // Spawns game manager.
        /*Spawn(
            30,
            false,
            false,

            3656837586448471562,
            180257209,

            UnityEngine.Vector3.zero,
            UnityEngine.Quaternion.identity,
            UnityEngine.Vector3.one);*/

        SpawnPlayer();
    }

    public PlayerObject Object;

    public void SpawnPlayer()
    {
        Object = new PlayerObject(true, true, NextId);
        Object.Spawn(this, default);
        Object.SendUpdate(this);

        this.NetworkIdentityId = Object.NetworkId;
    }

    public void DestroyObject(uint networkIdentityId)
    {
        NetworkWriter wr = new NetworkWriter();

        wr.WriteUShort(NetworkMessageId<ObjectDestroyMessage>.Id);
        wr.WriteUInt(networkIdentityId);

        SendMirrorData(wr);
    }

    public void FastRoundrestart()
    {
        NetworkWriter wr = new NetworkWriter();

        wr.WriteUShort(NetworkMessageId<RoundRestartMessage>.Id);

        //Restart Type ( Fast Restart )
        wr.WriteByte(1);

        SendMirrorData(wr);

        DestroyObject(NetworkIdentityId);
    }

    public void SendToScene(string sceneName)
    {
        NetworkWriter wr = new NetworkWriter();

        wr.WriteUShort(NetworkMessageId<SceneMessage>.Id);

        //Scene name
        wr.WriteString(sceneName);
        //Scene operation ( Normal, LoadAdditive, UnloadAdditive )
        wr.WriteByte(0);
        //Custom handling
        wr.WriteBool(false);

        SendMirrorData(wr);
    }

    public void NotReady()
    {
        SendMirrorData<NotReadyMessage>();
    }

    public void SetSeed(int seed)
    {
        NetworkWriter wr = new NetworkWriter();

        wr.WriteUShort(NetworkMessageId<SeedMessage>.Id);

        wr.WriteInt(seed);

        SendMirrorData(wr);
    }

    public void SetHealth(float value)
    {
        NetworkWriter wr = new NetworkWriter();
        wr.WriteUShort(NetworkMessageId<StatMessage>.Id);

        wr.WriteUInt(NetworkIdentityId);

        // 0 HealthStat
        // 1 AhpStat
        // 2 StaminaStat
        // 3 AdminFlagsStat
        // 4 HumeShieldStat
        // 5 Vigor Stat
        wr.WriteByte(0);

        wr.WriteByte((byte)StatMessageType.CurrentValue);

        int clampedValue = UnityEngine.Mathf.Clamp(UnityEngine.Mathf.CeilToInt(value), 0, 65535);
        wr.WriteUShort((ushort)clampedValue);

        SendMirrorData(wr);
    }

    public void OnConnectionResponse(Server server, BaseResponse response)
    {
        switch (response)
        {
            case ServerIsFullResponse _:
                break;
            case ServerIsOfflineResponse _:
                break;
            default:
                break;
        }

        //Connection?.OnConnectionResponse(server, response);
    }

    public void Disconnect(string message = null)
    {
        if (Request == null)
        {
            Peer.Disconnect();
        }
        else
        {
            Request.RejectWithMessage(message);
            Listener?.OnClientDisconneted(this, DisconnectReason.DisconnectPeerCalled);
            Dispose();
        }
    }

    public void Dispose()
    {
        Connection.Dispose();
        BackupConnection.Dispose();

        if (Peer != null)
            Listener.ClientById.Remove(Peer.Id);

        IsDisposing = true;
    }
}
