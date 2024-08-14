using LiteNetLib;
using Mirror;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using XProxy.Core;
using XProxy.Core.Connections;
using XProxy.Core.Events;
using XProxy.Core.Events.Args;
using XProxy.Models;
using XProxy.Services;
using XProxy.Shared.Models;

namespace XProxy
{
    public class ProxyServer
    {
        private NetManager _manager;
        private EventBasedNetListener _listener;

        internal ConfigService _config;

        public ushort Port => _config.Value.Port;

        public ConcurrentDictionary<int, Player> Players { get; private set; } = new ConcurrentDictionary<int, Player>();
        public Dictionary<string, ServerInfo> Servers { get; private set; } = new Dictionary<string, ServerInfo>();

        public static ConcurrentDictionary<string, LastServerInfo> ForceServerForUserID { get; set; } = new ConcurrentDictionary<string, LastServerInfo>();

        public static Dictionary<uint, string> MessageIdToName = new Dictionary<uint, string>();

        public static Dictionary<string, Type> Simulations { get; private set; } = new Dictionary<string, Type>()
        {
            { "lobby", typeof(LobbyConnection) }
        };

        public ProxyServer(ConfigService config)
        {
            foreach(var message in typeof(GameConsoleTransmission).Assembly.GetTypes().Where(x => x.GetInterface("NetworkMessage") != null))
            {
                ushort key = (ushort)message.FullName.GetStableHashCode();

                if (!MessageIdToName.ContainsKey(key))
                    MessageIdToName.Add(key, message.FullName);
            }

            foreach (var message in typeof(Mirror.AddPlayerMessage).Assembly.GetTypes().Where(x => x.GetInterface("NetworkMessage") != null))
            {
                ushort key = (ushort)message.FullName.GetStableHashCode();

                if (!MessageIdToName.ContainsKey(key))
                    MessageIdToName.Add(key, message.FullName);
            }

            _config = config;
            RefreshServers();

            _listener = new EventBasedNetListener();
            _listener.ConnectionRequestEvent += OnConnectionRequest;
            _listener.NetworkReceiveEvent += OnNetworkReceive;
            _listener.PeerDisconnectedEvent += OnPeerDisconnected;

            _manager = new NetManager(_listener);
            _manager.UpdateTime = 5;
            _manager.BroadcastReceiveEnabled = true;
            _manager.ChannelsCount = (byte)6;
            _manager.DisconnectTimeout = 6000;
            _manager.ReconnectDelay = 400;
            _manager.MaxConnectAttempts = 2;
            _manager.Start(_config.Value.Port);

            Logger.Info($"{_config.Messages.ProxyStartedListeningMessage.Replace("%port%", $"{Port}").Replace("%version%", ConfigModel.GameVersion)}", $"XProxy");
            Logger.Info("");
        }

        public ServerInfo GetServerByIp(string ip)
        {
            string[] ipParse = ip.Split(':');

            if (ipParse.Length != 2) return null;

            string ipPart = ipParse[0];

            if (!int.TryParse(ipParse[1], out int port))
                return null;

            return Servers.Values.Where(x => x.ServerIp == ipPart && x.ServerPort == port).FirstOrDefault();
        }

        public ServerInfo GetServerByPublicIp(string ip)
        {
            string[] ipParse = ip.Split(':');

            if (ipParse.Length != 2) return null;

            string ipPart = ipParse[0];

            if (!int.TryParse(ipParse[1], out int port))
                return null;

            return Servers.Values.Where(x => x.ServerPublicIp == ipPart && x.ServerPort == port).FirstOrDefault();
        }

        public ServerInfo GetServerByName(string name)
        {
            if (name == null) return null;

            if (Servers.TryGetValue(name, out ServerInfo info))
                return info;

            return null;
        }

        public ServerInfo GetFirstServerFromPriorities()
        {
            ServerInfo first = Servers
                    .Where(x => _config.Value.Priorities.Contains(x.Key))
                    .OrderBy(pair => _config.Value.Priorities.IndexOf(pair.Key))
                    .Select(pair => pair.Value)
                    .ToList()
                    .FirstOrDefault();

            return first;
        }

        public ServerInfo GetRandomServerFromPriorities(Player plr = null)
        {
            ServerInfo random = Servers
                    .Where(x => _config.Value.Priorities.Contains(x.Key))
                    .OrderBy(pair => _config.Value.Priorities.IndexOf(pair.Key))
                    .Select(pair => pair.Value)
                    .Where(x => plr != null ? x.CanPlayerJoin(plr) : !x.IsServerFull)
                    .ToList()
                    .FirstOrDefault();

            return random;
        }

        public void SaveLastServerForUser(string userid, string serverIndex, float duration)
        {
            LastServerInfo newInfo = new LastServerInfo()
            {
                Index = serverIndex,
                Time = DateTime.Now.AddSeconds(duration),
            };

            if (ForceServerForUserID.ContainsKey(userid))
                ForceServerForUserID[userid] = newInfo;
            else
                ForceServerForUserID.TryAdd(userid, newInfo);
        }

        public void ClearSavedLastServer(string userid)
        {
            ForceServerForUserID.Remove(userid, out LastServerInfo _);
        }

        public bool HasSavedLastServer(string userId)
        {
            if (!ForceServerForUserID.TryGetValue(userId, out LastServerInfo info))
                return false;

            return info.Time > DateTime.Now;
        }

        public ServerInfo GetSavedLastServer(string userid)
        {
            if (!HasSavedLastServer(userid)) return null;

            return GetServerByName(ForceServerForUserID[userid].Index);
        }

        public ServerInfo GetSavedLastServerAndClear(string userid)
        {
            string name = ForceServerForUserID[userid].Index;

            ClearSavedLastServer(userid);

            return GetServerByName(name);
        }

        public void RefreshServers()
        {
            Servers = _config.Value.Servers.ToDictionary(
                x => 
                    x.Key, 
                a => 
                    new ServerInfo(
                        a.Key, 
                        a.Value.Name, 
                        a.Value.PublicIp, 
                        a.Value.Ip, 
                        a.Value.Port, 
                        a.Value.MaxPlayers, 
                        a.Value.SendIpAddressInPreAuth, 
                        a.Value.ConnectionType, 
                        a.Value.Simulation
                    ));
        }

        public async Task Run()
        {
            while (true)
            {
                try
                {
                    if (_manager != null)
                        if (_manager.IsRunning)
                            _manager.PollEvents();
                }
                catch(Exception ex)
                {
                    Logger.Error(ex);
                }

                await Task.Delay(1);
            }
        }

        public void OnConnectionRequest(ConnectionRequest request)
        {
            string failed = string.Empty;
            string ip = $"{request.RemoteEndPoint.Address}";

            Logger.Debug($"[{request.RemoteEndPoint.Address}] Connection request, read preAuth...");

            var preAuth = PreAuthModel.ReadPreAuth(ip, request.Data, ref failed);

            Logger.Debug($"[{request.RemoteEndPoint.Address}] Connection request, preAuth\n{preAuth}");

            if (!preAuth.IsValid)
            {
                Logger.Warn(_config.Messages.PreAuthIsInvalidMessage.Replace("%address%", $"{request.RemoteEndPoint.Address}").Replace("%failed%", failed), "XProxy");
                request.RejectForce();
                return;
            }

            if (preAuth.Major != _config.Value.GameVersionParsed.Major || preAuth.Minor != _config.Value.GameVersionParsed.Minor || preAuth.Revision != _config.Value.GameVersionParsed.Build)
            {
                Logger.Info(_config.Messages.WrongVersion.Replace("%address%", $"{request.RemoteEndPoint.Address}").Replace("%userid%", preAuth.UserID).Replace("%version%", preAuth.Version), "XProxy");
                request.DisconnectWrongVersion();
                return;
            }

            bool ignoreSlots = preAuth.Flags.HasFlagFast(CentralAuthPreauthFlags.ReservedSlot) || preAuth.Flags.HasFlagFast(CentralAuthPreauthFlags.NorthwoodStaff) && _config.Value.NorthwoodStaffIgnoresSlots;

            if (!ignoreSlots && _manager.ConnectedPeersCount >= _config.Value.MaxPlayers)
            {
                Logger.Info(_config.Messages.ProxyIsFull.Replace("%address%", $"{request.RemoteEndPoint.Address}").Replace("%userid%", preAuth.UserID), "XProxy");
                request.DisconnectServerFull();
                return;
            }

            _config.Value.Users.TryGetValue(preAuth.UserID, out UserModel model);

            if (!ignoreSlots && _config.Value.MaintenanceMode && (model == null || !model.IgnoreMaintenance))
            {
                Logger.Info(_config.Messages.MaintenanceDisconnectMessage.Replace("%address%", $"{request.RemoteEndPoint.Address}").Replace("%userid%", preAuth.UserID), $"XProxy");
                request.Disconnect(_config.Messages.MaintenanceKickMessage);
                return;
            }

            ProxyConnectionRequest ev = new ProxyConnectionRequest(this, request, preAuth.IpAddress, preAuth.UserID, preAuth.Flags);

            EventManager.Proxy.InvokeConnectionRequest(ev);
            
            if (ev.IsCancelled)
                return;

            Logger.Debug($"[{request.RemoteEndPoint.Address}] Create player");

            Player player = new Player(this, preAuth);

            Logger.Debug($"[{request.RemoteEndPoint.Address}] Get last server");

            ServerInfo target;
            if (HasSavedLastServer(preAuth.UserID))
                target = GetSavedLastServerAndClear(preAuth.UserID);
            else
                target = GetRandomServerFromPriorities(player);

            if (target == null)
            {
                Logger.Info(_config.Messages.ProxyIsFull.Replace("%address%", $"{request.RemoteEndPoint.Address}").Replace("%userid%", preAuth.UserID), "XProxy");
                request.DisconnectServerFull();
                return;
            }

            PlayerAssignTargetServer ev2 = new PlayerAssignTargetServer(player, target);

            EventManager.Player.InvokeAssignTargetServer(ev2);

            if (target.SendIpAddressInPreAuth)
                preAuth.RawPreAuth.Put(ip);

            Logger.Debug($"[{request.RemoteEndPoint.Address}] Internal setup -> connect");

            player.InternalSetup(request, target);
            player.InternalConnect();
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            if (!Players.TryGetValue(peer.Id, out Player player)) return;

            player.InternalReceiveDataFromProxy(reader, deliveryMethod);
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            if (!Players.TryGetValue(peer.Id, out Player player)) return;

            bool showDisconnectMessage = !player.IsRoundRestarting && !player.IsRedirecting && disconnectInfo.Reason != DisconnectReason.DisconnectPeerCalled;

            if (showDisconnectMessage)
            {
                switch (disconnectInfo.Reason)
                {
                    case DisconnectReason.RemoteConnectionClose:
                        Logger.Info(_config.Messages.ProxyClientClosedConnectionMessage.Replace("%tag%", player.Tag).Replace("%address%", $"{peer.EndPoint.Address}").Replace("%userid%", player.UserId), $"XProxy");
                        break;
                    default:
                        Logger.Info(_config.Messages.ProxyClientClosedConnectionMessage.Replace("%tag%", player.Tag).Replace("%address%", $"{peer.EndPoint.Address}").Replace("%userid%", player.UserId).Replace("%reason%", $"{disconnectInfo.Reason}"), $"XProxy");
                        break;
                }
            }

            player.InternalDestroy();
        }
    }
}
