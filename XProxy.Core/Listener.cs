using LiteNetLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using XProxy.Core;
using XProxy.Core.Connections;
using XProxy.Core.Core.Events.Args;
using XProxy.Core.Events;
using XProxy.Core.Events.Args;
using XProxy.Core.Models;
using XProxy.Models;
using XProxy.Services;
using XProxy.Shared.Models;

namespace XProxy
{
    public class Listener
    {
        public static Dictionary<string, Listener> NamesByListener = new Dictionary<string, Listener>();

        private NetManager _manager;
        private EventBasedNetListener _listener;

        public static bool UpdateServers = false;

        internal ConfigService _config => ConfigService.Singleton;
       
        // Shared data between all listeners.

        public ConcurrentDictionary<int, Player> Players { get; private set; } = new ConcurrentDictionary<int, Player>();

        public static ConcurrentDictionary<string, int> PlayersByUserId { get; private set; } = new ConcurrentDictionary<string, int>();
        public static ConcurrentDictionary<string, LastServerInfo> ForceServerForUserID { get; set; } = new ConcurrentDictionary<string, LastServerInfo>();

        public static int GetTotalPlayersOnline()
        {
            int count = 0;

            foreach (var listener in NamesByListener.Values)
                count += listener.Players.Count;

            return count;
        }

        public static List<Player> GetAllPlayers()
        {
            var list = new List<Player>();

            foreach (var listener in NamesByListener.Values)
                list.AddRange(listener.Players.Values);

            return list;
        }

        public static Player GetPlayerByUserId(string userId)
        {
            if (!PlayersByUserId.TryGetValue(userId, out int playerId))
                return null;

            Player plr = null;

            foreach (var listener in NamesByListener.Values)
            {
                if (!listener.Players.TryGetValue(playerId, out plr))
                    continue;
            }

            return plr;
        }

        // --

        public static Dictionary<string, Type> Simulations { get; private set; } = new Dictionary<string, Type>()
        {
            { "lobby", typeof(LobbyConnection) }
        };

        /// <summary>
        /// Gets listener name.
        /// </summary>
        public string ListenerName { get; private set; }

        /// <summary>
        /// Gets listener settings for this proxy server.
        /// </summary>
        public ListenerServer Settings
        {
            get
            {
                if (ConfigService.Singleton.Value.Listeners.TryGetValue(ListenerName, out var listener))
                    return listener;

                return null;
            }
        }

        public Listener(string listenerName)
        {
            ListenerName = listenerName;
            NamesByListener.Add(ListenerName, this);

            Server.Refresh();

            NetDebug.Logger = new CustomNetLogger();

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
            _manager.Start(IPAddress.Parse(Settings.ListenIp), IPAddress.IPv6Any, Settings.Port);

            EventManager.Proxy.InvokeStartedListening(new ProxyStartedListening(this, Settings.Port));

            Logger.Info($"{_config.Messages.ProxyStartedListeningMessage.Replace("%port%", $"{Settings.Port}").Replace("%version%", ConfigModel.GameVersion)}", $"XProxy");
        }

        public Server GetFirstServerFromPriorities()
        {
            Server first = Server.List
                    .Where(x => Settings.Priorities.Contains(x.Name))
                    .OrderBy(pair => Settings.Priorities.IndexOf(pair.Name))
                    .ToList()
                    .FirstOrDefault();

            return first;
        }

        public Server GetRandomServerFromPriorities(Player plr = null)
        {
            Server random = Server.List
                    .Where(x => Settings.Priorities.Contains(x.Name))
                    .OrderBy(pair => Settings.Priorities.IndexOf(pair.Name))
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

        public Server GetSavedLastServerAndClear(string userid)
        {
            if (!ForceServerForUserID.TryGetValue(userid, out LastServerInfo info))
                return null;

            string name = info.Index;

            ClearSavedLastServer(userid);

            if (!Server.TryGetByName(name, out Server server))
                return null;

            return server;
        }

        public async Task Run(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (_manager != null)
                        if (_manager.IsRunning)
                            _manager.PollEvents();

                    if (UpdateServers)
                    {
                        Server.Refresh();
                        UpdateServers = false;
                    }
                }
                catch(Exception ex)
                {
                    Logger.Error(ex);
                }

                await Task.Delay(10);
            }
        }

        public void OnConnectionRequest(ConnectionRequest request)
        {
            Logger.Debug($"[{request.RemoteEndPoint.Address}] New incoming connection from listener, {Settings.ListenIp}:{Settings.Port}");

            string failed = string.Empty;
            string ip = $"{request.RemoteEndPoint.Address}";

            var preAuth = PreAuthModel.ReadPreAuth(ip, request.Data, ref failed);

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

            if (!ignoreSlots && _manager.ConnectedPeersCount >= Settings.MaxPlayers)
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

            Player player = new Player(this, preAuth);

            Server target;
            if (HasSavedLastServer(preAuth.UserID))
                target = GetSavedLastServerAndClear(preAuth.UserID);
            else
                target = GetRandomServerFromPriorities(player);

            if (target == null)
            {
                Logger.Info(_config.Messages.ProxyIsFull.Replace("%address%", $"{request.RemoteEndPoint.Address}").Replace("%userid%", preAuth.UserID), "XProxy");
                request.DisconnectServerFull();
                player?.Dispose();
                return;
            }

            PlayerAssignTargetServer ev2 = new PlayerAssignTargetServer(player, target);

            EventManager.Player.InvokeAssignTargetServer(ev2);

            if (target.Settings.SendIpAddressInPreAuth)
                preAuth.RawPreAuth.Put(ip);

            Logger.Debug($"[{request.RemoteEndPoint.Address}] Internal setup -> connect");

            player.InternalSetup(request, target);
            player.InternalConnect();
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            if (!Players.TryGetValue(peer.Id, out Player player)) 
                return;

            player.InternalReceiveDataFromProxy(reader, deliveryMethod);
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            if (!Players.TryGetValue(peer.Id, out Player player)) 
                return;

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
