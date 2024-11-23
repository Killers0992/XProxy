using LiteNetLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LiteNetLib.Utils;
using XProxy.Core;
using XProxy.Core.Connections;
using XProxy.Core.Core.Events.Args;
using XProxy.Core.Events;
using XProxy.Core.Events.Args;
using XProxy.Core.Models;
using XProxy.Enums;
using XProxy.Models;
using XProxy.Services;

namespace XProxy
{
    public class Listener
    {
        /// <summary>
        /// UseAccurateOnlineStatus Toggle For Server Status.
        /// </summary>
        public bool UseAccurateOnlineStatus { get; set; } = false;
        /// <summary>
        /// Gets total amount of listeners.
        /// </summary>
        public static int Count => ListenerByName.Count;

        /// <summary>
        /// Gets all listeners.
        /// </summary>
        public static List<Listener> List => ListenerByName.Values.ToList();

        /// <summary>
        /// Gets listeners identified by name.
        /// </summary>
        public static Dictionary<string, Listener> ListenerByName { get; private set; } = new Dictionary<string, Listener>();

        /// <summary>
        /// Gets listener by name.
        /// </summary>
        /// <param name="name">The name of listener.</param>
        /// <param name="listener">The listener.</param>
        /// <returns>If successfull.</returns>
        public static bool TryGet(string name, out Listener listener)
        {
            if (string.IsNullOrEmpty(name))
            {
                listener = null;
                return false;
            }

            return ListenerByName.TryGetValue(name, out listener);
        }

        internal NetManager _manager;
        private EventBasedNetListener _listener;
       
        // Shared data between all listeners.

        public static ConcurrentDictionary<string, Player> ConnectionToUserId { get; private set; } = new ConcurrentDictionary<string, Player>();
        public static ConcurrentDictionary<string, LastServerInfo> ForceServerForUserID { get; set; } = new ConcurrentDictionary<string, LastServerInfo>();

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

        /// <summary>
        /// Gets all connections made to this listener.
        /// </summary>
        public Dictionary<IPEndPoint, Player> Connections { get; private set; } = new Dictionary<IPEndPoint, Player>();

        /// <summary>
        /// Gets all servers connected to this listener.
        /// </summary>
        public Dictionary<IPEndPoint, Server> ServerConnections { get; private set; } = new Dictionary<IPEndPoint, Server>();

        public CancellationToken CancellationToken { get; }

        public Listener(string listenerName, CancellationToken token)
        {
            ListenerName = listenerName;
            ListenerByName.Add(ListenerName, this);

            CancellationToken = token;

            Server.Refresh(true);

            NetDebug.Logger = new CustomNetLogger();

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
            _manager.StartInManualMode(IPAddress.Parse(Settings.ListenIp), IPAddress.IPv6Any, Settings.Port);

            EventManager.Proxy.InvokeStartedListening(new ProxyStartedListening(this, Settings.Port));

            Logger.Info($"(f=white)[(f=darkcyan){listenerName}(f=white)] {ConfigService.Singleton.Messages.ProxyStartedListeningMessage
                .Replace("%port%", $"{Settings.Port}")
                .Replace("%ip%", Settings.ListenIp)
                .Replace("%version%", Settings.Version)}", $"XProxy");

            Task.Run(() => RunEventPolling(CancellationToken), CancellationToken);
        }

        private async Task RunEventPolling(CancellationToken token)
        {
            const int msDelay = 10;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    _manager.PollEvents();
                    _manager.ManualUpdate(msDelay);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }

                foreach (Player connection in Connections.Values)
                {
                    try
                    {
                        connection.PollEvents();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                }

                await Task.Delay(msDelay, token);
            }
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
            var matchingServers = Server.List
                .Where(x => Settings.Priorities.Contains(x.Name))
                .OrderBy(pair => Settings.Priorities.IndexOf(pair.Name));

            Logger.Debug($"Matching servers by priority: {string.Join(", ", matchingServers.Select(s => s.Name))}");

            var filteredServers = matchingServers
                .Where(x => x.IsServerOnline) 
                .Where(x => plr != null ? x.CanPlayerJoin(plr) : !x.IsServerFull);

            Logger.Debug($"Joinable servers: {string.Join(", ", filteredServers.Select(s => s.Name))}");

            Server random = filteredServers.FirstOrDefault();

            if (random == null)
            {
                Logger.Warn("No servers matched the join conditions!");
            }

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

        public void OnConnectionRequest(ConnectionRequest request)
        {
            Logger.Debug($"[{request.RemoteEndPoint.Address}] New incoming connection from listener, {Settings.ListenIp}:{Settings.Port}");

            string failed = string.Empty;
            string ip = $"{request.RemoteEndPoint.Address}";
            
            var preAuth = PreAuthModel.ReadPreAuth(ip, request.Data, ref failed);

            if (preAuth.Server != null)
            {
                NetPeer peer = request.Accept();
                preAuth.Server.OnConnected(peer);
                ServerConnections.Add(peer.EndPoint, preAuth.Server);
                return;
            }

            if (!preAuth.IsValid)
            {
                Logger.Debug(ConfigService.Singleton.Messages.PreAuthIsInvalidMessage.Replace("%address%", $"{request.RemoteEndPoint.Address}").Replace("%failed%", failed), "XProxy");
                request.RejectForce();
                return;
            }

            if (preAuth.Major != Settings.GameVersionParsed.Major || preAuth.Minor != Settings.GameVersionParsed.Minor || preAuth.Revision != Settings.GameVersionParsed.Build)
            {
                Logger.Info(ConfigService.Singleton.Messages.WrongVersion.Replace("%address%", $"{request.RemoteEndPoint.Address}").Replace("%userid%", preAuth.UserID).Replace("%version%", preAuth.Version), "XProxy");
                request.DisconnectWrongVersion();
                return;
            }

            bool ignoreSlots = preAuth.Flags.HasFlagFast(CentralAuthPreauthFlags.ReservedSlot) || preAuth.Flags.HasFlagFast(CentralAuthPreauthFlags.NorthwoodStaff) && ConfigService.Singleton.Value.NorthwoodStaffIgnoresSlots;

            if (!ignoreSlots && _manager.ConnectedPeersCount >= Settings.MaxPlayers)
            {
                Logger.Info(ConfigService.Singleton.Messages.ProxyIsFull.Replace("%address%", $"{request.RemoteEndPoint.Address}").Replace("%userid%", preAuth.UserID), "XProxy");
                request.DisconnectServerFull();
                return;
            }

            ConfigService.Singleton.Value.Users.TryGetValue(preAuth.UserID, out UserModel model);

            if (!ignoreSlots && ConfigService.Singleton.Value.MaintenanceMode && (model == null || !model.IgnoreMaintenance))
            {
                Logger.Info(ConfigService.Singleton.Messages.MaintenanceDisconnectMessage.Replace("%address%", $"{request.RemoteEndPoint.Address}").Replace("%userid%", preAuth.UserID), $"XProxy");
                request.Disconnect(ConfigService.Singleton.Messages.MaintenanceKickMessage);
                return;
            }

            ProxyConnectionRequest ev = new ProxyConnectionRequest(this, request, preAuth.IpAddress, preAuth.UserID, preAuth.Flags);

            EventManager.Proxy.InvokeConnectionRequest(ev);
            
            if (ev.IsCancelled)
                return;

            Player player = new Player(this, request, preAuth);

            Server target;
            if (HasSavedLastServer(preAuth.UserID))
                target = GetSavedLastServerAndClear(preAuth.UserID);
            else
                target = GetRandomServerFromPriorities(player);

            if (target == null)
            {
                Logger.Debug($"UserID: {preAuth.UserID}, HasSavedLastServer: {HasSavedLastServer(preAuth.UserID)}");
                
                foreach (var listener in Listener.List)
                {
                    var servers = listener.ServerConnections.Values.Select(s => $"{s.Name} ({s.Settings.Port})");
                    Logger.Debug($"Listener: {listener.ListenerName}, Servers: {string.Join(", ", servers)}");
                }
                
                Logger.Debug($"Global Server List: {string.Join(", ", Server.List.Select(s => s.Name))}");

                player.DisconnectFromProxy("No server found!");
                return;
            }

            
            PlayerAssignTargetServer ev2 = new PlayerAssignTargetServer(player, target);

            EventManager.Player.InvokeAssignTargetServer(ev2);

            if (target.Settings.SendIpAddressInPreAuth)
                preAuth.RawPreAuth.Put(ip);

            player.InternalSetup(target);
            player.InternalConnect();
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            if (!Connections.TryGetValue(peer.EndPoint, out Player player))
            {
                if (ServerConnections.TryGetValue(peer.EndPoint, out Server server))
                    server.OnReceiveData(reader);

                return;
            }

            player.InternalReceiveDataFromProxy(reader, deliveryMethod);
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            if (!Connections.TryGetValue(peer.EndPoint, out Player player))
            {
                if (ServerConnections.TryGetValue(peer.EndPoint, out Server server))
                {
                    server.OnDisconnected();
                    ServerConnections.Remove(peer.EndPoint);
                }
                return;
            }

            bool showDisconnectMessage = disconnectInfo.Reason != DisconnectReason.DisconnectPeerCalled;

            if (showDisconnectMessage)
            {
                switch (disconnectInfo.Reason)
                {
                    case DisconnectReason.RemoteConnectionClose:
                        Logger.Info(ConfigService.Singleton.Messages.ProxyClientClosedConnectionMessage.Replace("%tag%", player.Tag).Replace("%address%", $"{peer.EndPoint.Address}").Replace("%userid%", player.UserId), $"XProxy");
                        break;
                    default:
                        Logger.Info(ConfigService.Singleton.Messages.ProxyClientClosedConnectionMessage.Replace("%tag%", player.Tag).Replace("%address%", $"{peer.EndPoint.Address}").Replace("%userid%", player.UserId).Replace("%reason%", $"{disconnectInfo.Reason}"), $"XProxy");
                        break;
                }
            }

            player.Dispose();
        }
    }
}
