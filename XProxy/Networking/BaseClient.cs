using Hints;
using Mirror;
using PlayerRoles;
using VoiceChat.Networking;

namespace XProxy.Networking;

public class BaseClient : IDisposable
{
    private Connection _connection = new Connection();

    public uint NetworkIdentityId { get; private set; }// = Database.GetNextNetworkId();

    public uint NetworkId { get; set; }
    public bool IsReady { get; private set; }

    public Server Server { get; set; }

    public ConnectionRequest Request { get; set; }

    public BaseListener Listener { get; }
    public NetPeer Peer { get; set; } = null;
    public PreAuth PreAuth { get; }
    public double ListenerRemoteTimestamp { get; private set; }

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
        }
    }

    public Connection BackupConnection { get; private set; } = new Connection();

    public CustomUnbatcher UnbatcherCurrentServer { get; private set; } = new CustomUnbatcher();
    public CustomUnbatcher UnbatcherListener { get; private set; } = new CustomUnbatcher();

    public CustomBatcher Batcher { get; private set; } = new CustomBatcher(65535 * (NetConstants.MaxPacketSize - 6));

    public bool IsDisposing { get; set; }

    public DateTime ConnectedOn { get; } = DateTime.Now;
    public TimeSpan Connectiontime => DateTime.Now - ConnectedOn;

   // public Dictionary<ushort, Type> Types = ProxyUtils.FindNetworkMessageTypes();

    public string PlayerTag => $"[(f=cyan){Listener.ListenIpAddress}:{Listener.ListenPort}(f=white)] [(f=green){PreAuth.UserId}(f=white)]{(Server == null ? string.Empty : $" [(f=yellow){Server.IpAddress}:{Server.Port}(f=white)]")}";

    public BaseClient(BaseListener listener, ConnectionRequest request, PreAuth preAuth)
    {
        Listener = listener;
        Request = request;
        PreAuth = preAuth;

        Listener.NotConnectedClients.Add(this);

        Task.Run(() => RunBatcher());
    }

    async Task RunBatcher()
    {
        NetworkWriter writer = new NetworkWriter();

        while (!IsDisposing)
        {
            while (Batcher.GetBatch(writer))
            {
                var segment = writer.ToArraySegment();
                SendData(segment.Array, segment.Offset, segment.Count, DeliveryMethod.ReliableOrdered);
                writer.Position = 0;
            }

            await Task.Delay(10);
        }

        writer = null;
    }

    public virtual void OnConnectedToServer(Server Server)
    {

    }

    public virtual void OnDisconnectedFromServer(Server Server)
    {

    }

    public void OnConnectedToServerInternal(Server server)
    {
        OnConnectedToServer(server);
    }

    public void OnDisconnectedFromServerInternal(Server server)
    {
        OnDisconnectedFromServer(server);
    }

    public void AcceptConnection()
    {
        if (Request == null)
            return;

        Peer = Request.Accept();
        Listener.ClientById.Add(Peer.Id, this);

        Request = null;
    }



    public void PollEvents()
    {
        Connection.Update();
        BackupConnection.Update();
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
       /* if (!Types.ContainsKey(id))
            return true;

        string name = Types[id].FullName;
        switch (name)
        {
            // Ignore these messages.
            case "PlayerRoles.FirstPersonControl.NetworkMessages.FpcFromClientMessage":
            case "Mirror.TimeSnapshotMessage":
                break;
            default:
                Console.WriteLine($"FROM CLIENT -> " + name);
                break;
        }
       */
        return true;
    }

    public bool ProcessMirrorMessageFromServer(ushort id, NetworkReader reader)
    {
        switch (id)
        {
            // Spawn message
            case 16484:
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
                            NetworkId = netid;
                        break;
                }
                break;
        }

        var rolesyncid = NetworkMessageId<RoleSyncInfo>.Id;

        if (rolesyncid == id)
        {
            uint targetNetId = reader.ReadUInt();
            RoleTypeId targetRole = reader.ReadRoleType();

            Logger.Info($"{PlayerTag} Set role {targetRole} to target network id {targetNetId}", "Client");
            return true;
        }

        var vm = NetworkMessageId<VoiceMessage>.Id;

        if (vm == id)
        {
            SendBroadcast($"Talking {PreAuth.UserId}", 1, Broadcast.BroadcastFlags.Normal);
            return true;
        }


        /*

        if (!Types.ContainsKey(id))
            return true;

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
            default:
                Console.WriteLine($"FROM SERVER -> " + name);
                break;
        }*/

        return true;
    }
    public void SendHint(string message, float duration = 3)
    {
        NetworkWriter writerPooled = new NetworkWriter();

        writerPooled.WriteUShort(NetworkMessageId<HintMessage>.Id);

        var hint = new TextHint(message, new HintParameter[] {
                        new StringHintParameter(message) }, null, duration);

        //TextHint
        writerPooled.WriteByte(1);
        writerPooled.Serialize(hint);

        SendMirrorData(writerPooled);
    }

    public void SendBroadcast(string message, ushort time, Broadcast.BroadcastFlags flags)
    {
        NetworkWriter writerPooled = new NetworkWriter();

        // RPC MESSAGE
        writerPooled.WriteUShort(33978);
        writerPooled.WriteUInt(NetworkId);
        writerPooled.WriteByte(11);
        // BROADCAST
        writerPooled.WriteUShort(5862);

        NetworkWriter wri2 = new NetworkWriter();

        wri2.WriteString(message);
        wri2.WriteUShort(time);
        wri2.WriteByte((byte)flags);

        writerPooled.WriteArraySegment(wri2.ToArraySegment());

        SendMirrorData(writerPooled);
    }

    public void Connect(Server server)
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

    public void NotReady()
    {
        NetworkWriter wr = new NetworkWriter();

        wr.WriteUShort(NetworkMessageId<NotReadyMessage>.Id);

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
            Peer.Disconnect();
        else
        {
            Request.RejectWithMessage(message);
            Dispose();
        }
    }

    public void Dispose()
    {
        IsDisposing = true;
        Connection.Dispose();
        BackupConnection.Dispose();

        if (Peer != null)
        {
            Listener.ClientById.Remove(Peer.Id);
        }
        else
            Listener.NotConnectedClients.Remove(this);
    }
}
