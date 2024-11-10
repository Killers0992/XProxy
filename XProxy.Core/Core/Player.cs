﻿using Hints;
using LiteNetLib.Utils;
using LiteNetLib;
using Mirror;
using System;
using System.Net;
using System.Threading.Tasks;
using XProxy.Core.Connections;
using XProxy.Models;
using XProxy.Enums;
using RoundRestarting;
using PlayerRoles;
using RelativePositioning;
using static EncryptedChannelManager;
using System.Linq;
using XProxy.Shared.Enums;
using static PlayerStatsSystem.SyncedStatMessages;
using XProxy.Services;
using System.Threading;
using XProxy.Core.Monitors;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using Org.BouncyCastle.Utilities.Net;

namespace XProxy.Core
{
    public class Player : IDisposable
    {
        public const ushort RpcMessageId = 33978;
        public const ushort ReadyMessageId = 40252;
        public const ushort SceneMessageId = 30259;
        public const ushort SpawnMessageId = 16484;
        public const ushort AddPlayerMessageId = 13085;

        public const ushort ObjectSpawnStartedMessageId = 59786;
        public const ushort ObjectSpawnFinishedMessageId = 34417;

        public const ushort HintMessageId = 46055;

        public const ushort StatMessageId = 32410;
        public const ushort DisconnectErrorId = 55061;
        public const ushort RoleSyncInfoMessageId = 38952;
        public const ushort VcPrivacyMessageId = 3611;
        public const ushort EncryptedMessageOutsideId = 9934;

        public const ushort RoundRestartMessageId = 21154;
        public const ushort BroadcastAddId = 5862;
        public const ushort BroadcastClearId = 15261; 

        private bool _forceDisconnect;
        private string _forceDisconnectReason;

        private NetPeer _proxyPeer;
        private ConnectionRequest _connectionRequest;

        private NetManager _netManager;
        private EventBasedNetListener _listener;

        public IPEndPoint IpAddress;

        public Player(Listener proxy, ConnectionRequest request, PreAuthModel preAuth)
        {
            Proxy = proxy;
            IpAddress = request.RemoteEndPoint;

            Proxy.Connections.Add(IpAddress, this);

            _connectionRequest = request;

            PreAuth = preAuth;
        }

        public int Id { get; private set; } = -1;

        public bool IsDisposing { get; private set; }

        public DateTime ConnectedOn { get; } = DateTime.Now;
        public TimeSpan Connectiontime => DateTime.Now - ConnectedOn;

        public bool IsConnectionValid
        {
            get
            {
                if (_netManager == null)
                    return false;

                if (_connectionRequest != null)
                    return true;

                if (_proxyPeer.ConnectionState == ConnectionState.Connected)
                    return true;

                return false;
            }
        }

        public double RemoteTimestamp { get; private set; }
        public uint NetworkId { get; private set; } = Database.GetNextNetworkId();
        public string UserId => PreAuth.UserID;
        public bool IsPlayerSpawned { get; private set; }
        public bool IsRoundRestarting { get; private set; }
        public bool IsRedirecting { get; private set; }
        public bool IsChallenging => PreAuth.ChallengeID != 0;
        public bool IsConnectedToProxy => _proxyPeer != null;
        public bool IsConnectedToCurrentServer => _netManager != null && _netManager.FirstPeer != null;
        public bool ProcessMirrorMessagesFromProxy { get; set; }
        public bool ProcessMirrorMessagesFromCurrentServer { get; set; }
        public bool IsInQueue => CurrentServer == null ? false : CurrentServer.IsPlayerInQueue(this);
        public int PositionInQueue => CurrentServer == null ? -1 : CurrentServer.GetPlayerPositionInQueue(this);
        public Listener Proxy { get; private set; }
        public PreAuthModel PreAuth { get; private set; }
        public BaseConnection Connection { get; set; }
        public Server CurrentServer { get; set; }

        public CustomUnbatcher UnbatcherCurrentServer { get; private set; } = new CustomUnbatcher();
        public CustomUnbatcher UnbatcherProxy { get; private set; } = new CustomUnbatcher();

        public CustomBatcher Batcher { get; private set; } = new CustomBatcher(65535 * (NetConstants.MaxPacketSize - 6));

        public IPEndPoint ClientEndPoint => _connectionRequest != null ? _connectionRequest.RemoteEndPoint : _proxyPeer.EndPoint;

        public string Tag => Proxy._config.Messages.PlayerTag.Replace("%serverIpPort%", CurrentServer.ToString()).Replace("%server%", CurrentServer.Name);
        public string ErrorTag => Proxy._config.Messages.PlayerErrorTag.Replace("%serverIpPort%", CurrentServer.ToString()).Replace("%server%", CurrentServer.Name);

        public bool CanJoinQueue(Server server = null)
        {
            Server targetServer = server ?? CurrentServer;

            if (targetServer == null)
                return false;

            if (!targetServer.IsServerFull)
                return false;

            return targetServer.PlayersInQueueCount < targetServer.Settings.QueueSlots;
        }

        public async Task JoinQueue(Server server = null)
        {
            Server targetServer = server ?? CurrentServer;

            if (targetServer == null)
                return;

            CurrentServer = targetServer;
            Connection = new QueueConnection(this);

            if (targetServer.IsPlayerInQueue(this))
                return;

            // Check if the player has priority
            bool isPriority = await IsPriorityPlayer(PreAuth.UserID);

            if (isPriority)
            {
                targetServer.AddPlayerToPriorityQueue(this);
            }
            else
            {
                targetServer.AddPlayerToQueue(this);
            }
        }

        internal async Task<bool> IsPriorityPlayer(string steamId)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = "https://api.kingsplayground.fun/web/patronlist";
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        List<string> priorityList = JsonConvert.DeserializeObject<List<string>>(jsonResponse);

                        return priorityList.Contains(steamId);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error fetching priority list: " + ex.Message);
            }

            return false;
        }

        // METHODS

        public void Roundrestart(float time = 0.1f)
        {
            NetworkWriter writerPooled = new NetworkWriter();

            writerPooled.WriteUShort(RoundRestartMessageId);
            //Restart Type ( Full Restart )
            writerPooled.WriteByte(0);
            //Reconnect
            writerPooled.WriteBool(true);
            //Extended reconnection period
            writerPooled.WriteBool(false);
            //Time offset
            writerPooled.WriteFloat(time);

            SendMirrorMessage(writerPooled);
        }

        public void SendBroadcast(string message, ushort time, Broadcast.BroadcastFlags flags) 
        {
            NetworkWriter writerPooled = new NetworkWriter();

            writerPooled.WriteUShort(RpcMessageId);
            writerPooled.WriteUInt(NetworkId);
            writerPooled.WriteByte(11);
            writerPooled.WriteUShort(BroadcastAddId);

            NetworkWriter wri2 = new NetworkWriter();

            wri2.WriteString(message);
            wri2.WriteUShort(time);
            wri2.WriteByte((byte)flags);

            writerPooled.WriteArraySegment(wri2.ToArraySegment());

            SendMirrorMessage(writerPooled);
        }

        public void ClearBroadcasts()
        {
            NetworkWriter writerPooled = new NetworkWriter();

            writerPooled.WriteUShort(RpcMessageId);
            writerPooled.WriteUInt(NetworkId);
            writerPooled.WriteByte(11);
            writerPooled.WriteUShort(BroadcastClearId);
            writerPooled.WriteArraySegment(null);

            SendMirrorMessage(writerPooled);
        }

        public void SendHint(string message, float duration = 3)
        {
            message = PlaceHolders.ReplacePlaceholders(message);

            NetworkWriter writerPooled = new NetworkWriter();

            writerPooled.WriteUShort(HintMessageId);

            var hint = new TextHint(message, new HintParameter[] {
                        new StringHintParameter(message) }, null, duration);

            //TextHint
            writerPooled.WriteByte(1);
            writerPooled.Serialize(hint);

            SendMirrorMessage(writerPooled);
        }

        public void ObjectSpawnStarted()
        {
            NetworkWriter wri = new NetworkWriter();

            wri.WriteUShort(ObjectSpawnStartedMessageId);

            SendMirrorMessage(wri);
        }

        public void ObjectSpawnFinished()
        {
            NetworkWriter wri = new NetworkWriter();

            wri.WriteUShort(ObjectSpawnFinishedMessageId);

            SendMirrorMessage(wri);
        }

        public void SendToScene(string sceneName)
        {
            NetworkWriter wri = new NetworkWriter();

            wri.WriteUShort(SceneMessageId);

            //Scene name
            wri.WriteString(sceneName);
            //Scene operation ( Normal, LoadAdditive, UnloadAdditive )
            wri.WriteByte(0);
            //Custom handling
            wri.WriteBool(false);

            SendMirrorMessage(wri);
        }

        public void SendSpawnMessage(uint netId, bool isLocalPlayer, bool isOwner, ulong sceneId, uint assetId, UnityEngine.Vector3 pos, UnityEngine.Quaternion rot, UnityEngine.Vector3 scale, ArraySegment<byte> payload)
        {
            NetworkWriter wr = new NetworkWriter();

            wr.WriteUShort(SpawnMessageId);

            wr.WriteUInt(netId);
            wr.WriteBool(isLocalPlayer);
            wr.WriteBool(isOwner);

            wr.WriteULong(sceneId);
            wr.WriteUInt(assetId);

            wr.WriteVector3(pos);
            wr.WriteQuaternion(rot);
            wr.WriteVector3(scale);

            wr.WriteArraySegmentAndSize(payload);

            SendMirrorMessage(wr);
        }

        public void SetRole(RoleTypeId role)
        {
            NetworkWriter wr = new NetworkWriter();
            wr.WriteUShort(RoleSyncInfoMessageId);

            wr.WriteUInt(NetworkId);
            wr.WriteSByte((sbyte)role);
            wr.WriteRelativePosition(new RelativePosition(new UnityEngine.Vector3(0f, 0f, 0f)));
            wr.WriteUShort(0);

            SendMirrorMessage(wr);
        }

        public void SetHealth(float value)
        {
            NetworkWriter wr = new NetworkWriter();
            wr.WriteUShort(StatMessageId);

            wr.WriteUInt(NetworkId);

            // 0 HealthStat
            // 1 AhpStat
            // 2 StaminaStat
            // 3 AdminFlagsStat
            // 4 HumeShieldStat
            // 5 Vigor Stat
            wr.WriteByte(0);

            int clampedValue = UnityEngine.Mathf.Clamp(UnityEngine.Mathf.CeilToInt(value), 0, 65535);
            wr.WriteUShort((ushort)clampedValue);

            SendMirrorMessage(wr);
        }

        public void Spawn()
        {
            var playerIdentity = Database.GetNetworkIdentity("player");

            var gameManagerIdentity = Database.GetNetworkIdentity("gamemanager");

            SendSpawnMessage(
                Database.GetNextNetworkId(),
                false,
                false,
                gameManagerIdentity.SceneID,
                gameManagerIdentity.AssetID,
                UnityEngine.Vector3.zero,
                UnityEngine.Quaternion.identity,
                UnityEngine.Vector3.one,
                default(ArraySegment<byte>));

            SendSpawnMessage(
                NetworkId,
                true,
                true,
                0,
                playerIdentity.AssetID,
                new UnityEngine.Vector3(0f, 1000f, -42f),
                UnityEngine.Quaternion.identity,
                UnityEngine.Vector3.one,
                default(ArraySegment<byte>));
        }

        public void SendGameConsoleMessage(string message, string color = "green")
        {
            message = PlaceHolders.ReplacePlaceholders(message);

            NetworkWriter wri = new NetworkWriter();

            wri.WriteUShort(EncryptedMessageOutsideId);

            wri.WriteByte((byte)EncryptedChannelManager.SecurityLevel.Unsecured);

            string content = $"{color}#{message}";

            int length = Misc.Utf8Encoding.GetByteCount(content) + 5;

            byte[] data = new byte[length];

            data[0] = (byte)EncryptedChannel.GameConsole;
            BitConverter.GetBytes(0).CopyTo(data, 1);
            Misc.Utf8Encoding.GetBytes(content, 0, content.Length, data, 5);

            wri.WriteByteArray(data);

            SendMirrorMessage(wri);
        }

        public void SendMirrorMessage(NetworkWriter writer)
        {
            if (Batcher == null) return;

            Batcher.AddMessage(writer.ToArraySegment(), Connectiontime.TotalSeconds);

            writer = null;
        }

        public bool RedirectToLobby()
        {
            Server lobby = Proxy.GetRandomServerFromPriorities();

            return RedirectTo(lobby);
        }

        public bool RedirectTo(string serverName)
        {
            if (!Server.TryGetByName(serverName, out Server server))
                return false;

            return RedirectTo(server);
        }

        public bool RedirectTo(Server server, bool queue = false)
        {
            if (!server.CanPlayerJoin(this))
                return false;

            if (queue)
                CurrentServer.MarkPlayerInQueueAsConnecting(this);

            IsRedirecting = true;
            Logger.Info(Proxy._config.Messages.PlayerRedirectToMessage.Replace("%tag%", Tag).Replace("%address%", $"{ClientEndPoint}").Replace("%userid%", UserId).Replace("%server%", server.Name), $"Player");
            SaveServerForNextSession(server.Name, 7f);
            Roundrestart();
            return true;
        }

        public void SaveCurrentServerForNextSession(float duration = 4f) => Proxy.SaveLastServerForUser(UserId, CurrentServer.Name, duration);
        public void SaveServerForNextSession(string name, float duration = 4f) => Proxy.SaveLastServerForUser(UserId, name, duration);

        public void DisconnectFromProxy(string reason = null, NetDataWriter writer = null, RejectionReason? rejectionReason = null)
        {
            if (_connectionRequest != null)
            {
                if (!string.IsNullOrEmpty(reason))
                {
                    NetDataWriter customWriter = new NetDataWriter();
                    customWriter.Put((byte)RejectionReason.Custom);
                    customWriter.Put(reason);
                    _connectionRequest.Reject(customWriter);
                }
                else if (writer != null)
                    _connectionRequest.Reject(writer);
                else if (rejectionReason.HasValue)
                {
                    NetDataWriter customWriter = new NetDataWriter();
                    customWriter.Put((byte)rejectionReason.Value);
                    _connectionRequest.Reject(customWriter);
                }
                else
                    _connectionRequest.RejectForce();

                Dispose();
            }
            else
                _proxyPeer.Disconnect();
        }

        public void SendDataToCurrentServer(byte[] bytes, int position, int length, DeliveryMethod method)
        {
            if (!IsConnectedToCurrentServer) return;

            _netManager.FirstPeer.Send(bytes, position, length, method);
        }

        public void SendDataToProxy(byte[] bytes, int position, int length, DeliveryMethod method)
        {
            if (!IsConnectedToProxy) return;

            _proxyPeer.Send(bytes, position, length, method);
        }

        public void SendDataToProxy(NetDataWriter writer, DeliveryMethod method)
        {
            if (!IsConnectedToProxy) return;

            _proxyPeer.Send(writer, method);
        }

        public virtual void Update() { }

        void SetupNetManager()
        {
            if (_netManager != null) return;

            _listener = new EventBasedNetListener();

            _listener.PeerConnectedEvent += OnConnected;
            _listener.NetworkReceiveEvent += OnReceiveData;
            _listener.PeerDisconnectedEvent += OnDisconnected;

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

        public virtual void OnReceiveMirrorDataFromProxy(uint id, NetworkReader reader, ref bool cancelProcessor) 
        {
            switch (id)
            {
                case ReadyMessageId:
                    Connection.OnClientReady();
                    break;
                case AddPlayerMessageId:
                    Connection.OnAddPlayer();
                    break;
                case VcPrivacyMessageId:
                    if (!IsPlayerSpawned)
                    {
                        IsPlayerSpawned = true;
                        Connection.OnPlayerSpawned();
                    }
                    break;
                case EncryptedMessageOutsideId:
                    SecurityLevel securityLevel = (SecurityLevel)reader.ReadByte();

                    switch (securityLevel)
                    {
                        case SecurityLevel.Unsecured:
                            byte[] incomingData = reader.ReadByteArray();

                            EncryptedChannel channel = (EncryptedChannel)incomingData[0];
                            string content = Misc.Utf8Encoding.GetString(incomingData, 5, incomingData.Length - 5);

                            switch (channel)
                            {
                                case EncryptedChannel.GameConsole:
                                    string[] args = content.Split(' ');

                                    Connection.OnReceiveGameConsoleCommand(args[0], args.Skip(1).ToArray());
                                    break;
                            }
                            break;
                    }
                    break;
                default:
                    Connection.OnReceiveMirrorDataFromProxy(id, reader);
                    break;
            }
        }

        public virtual void OnReceiveMirrorDataFromCurrentServer(uint id, NetworkReader reader, ref bool cancelProcessor) 
        {
            switch (id)
            {
                case RoundRestartMessageId:
                    IsRoundRestarting = true;
                    RoundRestartMessage rrm = RoundRestartMessageReaderWriter.ReadRoundRestartMessage(reader);
                    SaveCurrentServerForNextSession(rrm.TimeOffset + 10f);
                    Logger.Info(Proxy._config.Messages.PlayerRoundRestartMessage.Replace("%tag%", Tag).Replace("%address%", $"{ClientEndPoint}").Replace("%userid%", UserId).Replace("%time%", $"{rrm.TimeOffset}"), $"Player");
                    break;
                case SpawnMessageId:
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
                case RpcMessageId:
                    RpcMessage msg = new RpcMessage
                    {
                        netId = reader.ReadUInt(),
                        componentIndex = reader.ReadByte(),
                        functionHash = reader.ReadUShort(),
                        payload = reader.ReadArraySegmentAndSize()
                    };

                    switch (msg.functionHash)
                    {
                        // We need to check if server is trying to disconnect player with error message.
                        case DisconnectErrorId:
                            using (NetworkReaderPooled networkReaderPooled = NetworkReaderPool.Get(msg.payload))
                            {
                                _forceDisconnectReason = networkReaderPooled.ReadString();
                                _forceDisconnect = true;
                            }
                            break;
                    }
                    break;
            }
        }

        internal void InternalReceiveDataFromProxy(NetPacketReader reader, DeliveryMethod method)
        {
            bool cancelProcessor = false;

            if (ProcessMirrorMessagesFromProxy)
                ProcessMirrorData(reader.RawData, reader.Position, reader.AvailableBytes, ref cancelProcessor, true);

            if (!cancelProcessor)
                Connection.OnReceiveDataFromProxy(reader, method);
        }

        internal void InternalSetup(Server info)
        {
            CurrentServer = info;

            TaskMonitor.RegisterTask(Task.Run(() => RunBatcher()));

            SetupNetManager();
        }

        internal void InternalConnect()
        {
            switch (CurrentServer.Settings.ConnectionType)
            {
                case ConnectionType.Proxied:
                    if (IsChallenging)
                        Logger.Info(Proxy._config.Messages.PlayerSentChallengeMessage.Replace("%tag%", Tag).Replace("%address%", $"{ClientEndPoint}").Replace("%userid%", UserId), $"Player");
                    else
                        Logger.Info(Proxy._config.Messages.PlayerIsConnectingMessage.Replace("%tag%", Tag).Replace("%address%", $"{ClientEndPoint}").Replace("%userid%", UserId), $"Player");

                    _netManager.Connect(CurrentServer.Settings.Ip, CurrentServer.Settings.Port, PreAuth.RawPreAuth);
                    break;
                case ConnectionType.Simulated:
                    if (CurrentServer.Settings.Simulation == "lobby")
                    {
                        if (ConfigService.Singleton.Value.AutoJoinQueueInLobby)
                        {
                            var targetServer = Proxy.GetFirstServerFromPriorities();

                            if (targetServer != null)
                            {
                                if (CanJoinQueue(targetServer))
                                {
                                    JoinQueue(targetServer);
                                }
                                break;
                            }
                        }
                    }

                    if (Listener.Simulations.TryGetValue(CurrentServer.Settings.Simulation, out Type simType))
                    {
                        Connection = (SimulatedConnection)Activator.CreateInstance(simType, args: this);
                    }
                    break;
            }
        }

        internal void InternalUpdate()
        {
            if (!IsPlayerSpawned)
                return;

            try
            {
                Update();
                Connection?.Update();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        internal void InternalAcceptConnection(BaseConnection connection = null)
        {
            // This should never happen.
            if (_connectionRequest == null)
                return;

            _proxyPeer = _connectionRequest.Accept();

            Id = _proxyPeer.Id;

            if (!CurrentServer.PlayersById.ContainsKey(Id))
                CurrentServer.PlayersById.TryAdd(Id, this);

            if (!Listener.ConnectionToUserId.ContainsKey(UserId))
                Listener.ConnectionToUserId.TryAdd(UserId, this);

            _connectionRequest = null;
            (connection == null ? Connection : connection).InternalConnected();
        }

        internal void InternalDisconnect()
        {
            _netManager.FirstPeer.Disconnect();
            Logger.Info(Proxy._config.Messages.PlayerServerShutdownMessage.Replace("%tag%", Tag).Replace("%address%", $"{ClientEndPoint}").Replace("%userid%", UserId), $"Player");
            DisconnectFromProxy();
        }

        internal void InternalDestroyNetwork()
        {
            if (_netManager != null)
            {
                _netManager.Stop();
                _netManager = null;
            }

            if (_listener != null)
            {
                _listener.PeerConnectedEvent -= OnConnected;
                _listener.NetworkReceiveEvent -= OnReceiveData;
                _listener.PeerDisconnectedEvent -= OnDisconnected;

                _listener = null;
            }
        }

        public void PollEvents()
        {
            if (_netManager == null)
                return;

            if (_netManager.IsRunning)
                _netManager.PollEvents();
        }

        async Task RunBatcher()
        {
            NetworkWriter writer = new NetworkWriter();

            while (!IsDisposing)
            {
                while (Batcher.GetBatch(writer))
                {
                    var segment = writer.ToArraySegment();
                    SendDataToProxy(segment.Array, segment.Offset, segment.Count, DeliveryMethod.ReliableOrdered);
                    writer.Position = 0;
                }

                await Task.Delay(10);
            }

            writer = null;
        }

        void ProcessMirrorData(byte[] bytes, int position, int length, ref bool cancelProcessor, bool fromProxy = true)
        {
            try
            {
                double timeStamp;
                ushort id;

                ArraySegment<byte> segment = new ArraySegment<byte>(bytes, position, length);

                if (!(fromProxy ? UnbatcherProxy : UnbatcherCurrentServer).AddBatch(segment))
                    return;

                while ((fromProxy ? UnbatcherProxy : UnbatcherCurrentServer).GetNextMessage(out ArraySegment<byte> message, out timeStamp))
                {
                    NetworkReader reader2 = new NetworkReader(message);

                    if (reader2.Remaining >= NetworkMessages.IdSize)
                    {
                        RemoteTimestamp = timeStamp;

                        if (NetworkMessages.UnpackId(reader2, out id))
                        {
                            if (fromProxy)
                                OnReceiveMirrorDataFromProxy(id, reader2, ref cancelProcessor);
                            else
                                OnReceiveMirrorDataFromCurrentServer(id, reader2, ref cancelProcessor); 
                        }
                        else
                            return;
                    }
                    else
                        return;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(Proxy._config.Messages.PlayerUnbatchingExceptionMessage.Replace("%tag%", ErrorTag).Replace("%address%", $"{ClientEndPoint}").Replace("%userid%", UserId).Replace("%message%", $"{ex}").Replace("%conditon%", fromProxy ? Proxy._config.Messages.Proxy : Proxy._config.Messages.CurrentServer), "Player");
            }
        }

        void OnConnected(NetPeer peer)
        {
            Connection = new ProxiedConnection(this);
        }

        void OnReceiveData(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
        {
            bool cancelProcessor = false;

            ProcessMirrorData(reader.RawData, reader.Position, reader.AvailableBytes, ref cancelProcessor, false);

            if (!cancelProcessor)
            {
                try
                {
                    SendDataToProxy(reader.RawData, reader.Position, reader.AvailableBytes, deliveryMethod);
                }
                catch (Exception ex)
                {
                    Logger.Error(Proxy._config.Messages.PlayerExceptionSendToProxyMessage.Replace("%tag%", ErrorTag).Replace("%address%", $"{ClientEndPoint}").Replace("%userid%", UserId).Replace("%message%", $"{ex}"), "Player");
                }
            }

            reader.Recycle();
        }

        void OnDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            switch (disconnectInfo.Reason)
            {
                case DisconnectReason.ConnectionFailed when disconnectInfo.AdditionalData.RawData == null:
                    DisconnectFromProxy(Proxy._config.Messages.ServerIsOfflineKickMessage.Replace("%server%", CurrentServer.Name));
                    Logger.Info(Proxy._config.Messages.PlayerServerIsOfflineMessage.Replace("%tag%", Tag).Replace("%address%", $"{ClientEndPoint}").Replace("%userid%", UserId), $"Player");
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
                            if (disconnectInfo.AdditionalData.TryGetByte(out byte offset))
                            {
                                SaveCurrentServerForNextSession(offset + 10f);
                                Logger.Info(Proxy._config.Messages.PlayerDelayedConnectionMessage.Replace("%tag%", Tag).Replace("%address%", $"{ClientEndPoint}").Replace("%userid%", UserId).Replace("%time%", $"{offset}"), $"Player");
                            }
                            break;

                        case RejectionReason.ServerFull:
                            Logger.Info(Proxy._config.Messages.PlayerServerIsFullMessage.Replace("%tag%", Tag).Replace("%address%", $"{ClientEndPoint}").Replace("%userid%", UserId), $"Player");

                            if (CanJoinQueue())
                            {
                                JoinQueue();
                                cancel = true;
                            }
                            break;

                        case RejectionReason.Banned:
                            long expireTime = disconnectInfo.AdditionalData.GetLong();
                            string banReason = disconnectInfo.AdditionalData.GetString();

                            var date = new DateTime(expireTime, DateTimeKind.Utc).ToLocalTime();

                            Logger.Info(Proxy._config.Messages.PlayerBannedMessage.Replace("%tag%", Tag).Replace("%address%", $"{ClientEndPoint}").Replace("%userid%", UserId).Replace("%reason%", banReason).Replace("%date%", date.ToShortDateString()).Replace("%time%", date.ToLongTimeString()), $"Player");
                            break;

                        case RejectionReason.Challenge:
                            //We need to save current server because after client tries to reconnect it will connect to random server from priority list.
                            SaveCurrentServerForNextSession();
                            Logger.Info(Proxy._config.Messages.PlayerReceivedChallengeMessage.Replace("%tag%", Tag).Replace("%address%", $"{ClientEndPoint}").Replace("%userid%", UserId), "Player");
                            break;

                        default:
                            Logger.Info(Proxy._config.Messages.PlayerDisconnectedMessage.Replace("%tag%", Tag).Replace("%address%", $"{ClientEndPoint}").Replace("%userid%", UserId).Replace("%reason%", $"{reason}"), $"Player");
                            break;
                    }

                    if (cancel)
                        return;

                    DisconnectFromProxy(writer: rejectedData);
                    return;

                case DisconnectReason.Timeout:
                case DisconnectReason.PeerNotFound:
                    Connection = new LostConnection(this, LostConnectionType.Timeout);
                    Logger.Info(Proxy._config.Messages.PlayerServerTimeoutMessage.Replace("%tag%", Tag).Replace("%address%", $"{ClientEndPoint}").Replace("%userid%", UserId), $"Player");
                    return;

                case DisconnectReason.RemoteConnectionClose when _forceDisconnect:
                    Logger.Info(Proxy._config.Messages.PlayerDisconnectedWithReasonMessage.Replace("%tag%", Tag).Replace("%address%", $"{ClientEndPoint}").Replace("%userid%", UserId).Replace("%reason%", _forceDisconnectReason), $"Player");
                    break;

                case DisconnectReason.RemoteConnectionClose when !_forceDisconnect:
                    Connection = new LostConnection(this, LostConnectionType.Shutdown);
                    Logger.Info(Proxy._config.Messages.PlayerServerShutdownMessage.Replace("%tag%", Tag).Replace("%address%", $"{ClientEndPoint}").Replace("%userid%", UserId), $"Player");
                    return;
            }

            DisconnectFromProxy();
        }

        public void Dispose()
        {
            Proxy.Connections.Remove(IpAddress);

            // If player is connected to any server remove their id.
            if (CurrentServer != null)
                CurrentServer.PlayersById.TryRemove(Id, out _);

            Listener.ConnectionToUserId.Remove(UserId, out _);

            InternalDestroyNetwork();

            IsDisposing = true;
            PreAuth = null;
            Batcher = null;
            UnbatcherCurrentServer = null;
            UnbatcherProxy = null;
        }
    }
}
