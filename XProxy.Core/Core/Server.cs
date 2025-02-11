﻿using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using XProxy.Core.Core.Events.Args;
using XProxy.Core.Enums;
using XProxy.Core.Events;
using XProxy.Core.Models;
using XProxy.Enums;
using XProxy.Models;
using XProxy.Services;

namespace XProxy.Core
{
    public class Server : IDisposable
    {
        /// <summary>
        /// Xproxy.Plugin Push Updates
        /// </summary>
        private DateTime _lastStatusUpdate = DateTime.MinValue;

        public bool HasRecentStatusUpdate()
        {
            const int StatusUpdateTimeoutSeconds = 30; // Timeout threshold in seconds
            return (DateTime.Now - _lastStatusUpdate).TotalSeconds <= StatusUpdateTimeoutSeconds;
        }

        static bool intialRefresh;

        public static bool UpdateServers;

        public static Dictionary<string, Server> ServersByName = new Dictionary<string, Server>();
        public static List<Server> List => ServersByName.Values.ToList();

        public static List<string> GetServerNames(Player plr)
        {
            List<string> names = new List<string>();

            foreach (var server in List)
            {
                if (server.Name == plr.CurrentServer.Name)
                    continue;

                names.Add(server.Name);
            }

            return names;
        }

        public static bool TryGetByName(string name, out Server server)
        {
            if (string.IsNullOrEmpty(name))
            {
                server = null;
                return false;
            }

            if (ServersByName.TryGetValue(name, out server))
                return true;

            server = null;
            return false;
        }

        public static bool TryGetByIp(string ip, ushort port, out Server server)
        {
            server = List.FirstOrDefault(x => (x.Settings.Ip == ip || x.Settings.PublicIp == ip) && x.Settings.Port == port);
            return server != null;
        }

        public static bool TryGetByPublicIp(string ip, out Server server)
        {
            if (string.IsNullOrEmpty(ip))
            {
                server = null;
                return false;
            }

            string[] ipParse = ip.Split(':');

            if (ipParse.Length != 2)
            {
                server = null;
                return false;
            }

            string ipPart = ipParse[0];

            if (!int.TryParse(ipParse[1], out int port))
            {
                server = null;
                return false;
            }

            server = List.FirstOrDefault(x => x.Settings.PublicIp == ipPart && x.Settings.Port == port);
            return server != null;
        }

        public static void Refresh(bool intial)
        {
            if (intial)
            {
                if (intialRefresh)
                    return;

                intialRefresh = true;
            }

            List<string> newServers = ConfigService.Singleton.Value.Servers.Keys.ToList();

            List<string> currentServers = ServersByName.Keys.ToList();

            List<string> serversToDestroy = currentServers.Except(newServers).ToList();
            List<string> serversToAdd = newServers.Except(currentServers).ToList();

            if (serversToDestroy.Count == 0 && serversToAdd.Count == 0)
                return;

            Logger.Info($"Initializing (f=green){serversToAdd.Count}(f=white) servers", "XProxy");

            foreach (string server in serversToDestroy)
            {
                if (TryGetByName(server, out Server serv))
                {
                    serv.Dispose();
                }

                Logger.Info($" - (f=red)Remove(f=white) - (f=magenta){server}(f=white)", "XProxy");
            }

            foreach (string server in serversToAdd)
            {
                new Server(server);
                Logger.Info($" - (f=green)Add(f=white) - (f=magenta){server}(f=white)", "XProxy");
            }
        }

        public Server(string serverName)
        {
            Name = serverName;

            if (!ServersByName.ContainsKey(serverName))
                ServersByName.Add(serverName, this);
            else
                ServersByName[serverName] = this;
        }

        /// <summary>
        /// Gets name of server.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets dictionary of players defined by player id.
        /// </summary>
        public ConcurrentDictionary<int, Player> PlayersById { get; private set; } = new ConcurrentDictionary<int, Player>();

        /// <summary>
        /// Gets online players.
        /// </summary>
        public List<Player> Players => PlayersById.Values
            .Where(x => x.IsConnectedToCurrentServer)
            .ToList();

        /// <summary>
        /// Gets amount of online players.
        /// </summary>
        public int PlayersCount => PlayersById.Values
            .Where(x => x.IsConnectedToCurrentServer)
            .Count();

        public ConcurrentDictionary<string, QueueTicket> PlayersInQueue { get; set; } = new ConcurrentDictionary<string, QueueTicket>();

        /// <summary>
        /// Gets amount of players in queue.
        /// </summary>
        public int PlayersInQueueCount => PlayersInQueue.Count;

        /// <summary>
        /// Gets if server is full.
        /// </summary>
        public bool IsServerFull => PlayersCount >= Settings.MaxPlayers;

        /// <summary>
        /// Gets settings of this server.
        /// </summary>
        public ServerModel Settings
        {
            get
            {
                if (ConfigService.Singleton.Value.TryGetServer(Name, out ServerModel model))
                    return model;

                // This should be never null.
                return null;
            }
        }

        /// <summary>
        /// Gets connection made by XProxy.Plugin to server.
        /// </summary>
        public NetPeer ConnectionToServer;

        /// <summary>
        /// Gets status of server.
        /// </summary>
        public ServerStatus Status { get; private set; }

        /// <summary>
        /// Gets if server is connected to XProxy.Plugin.
        /// </summary>
        public bool IsConnectedToServer => ConnectionToServer != null;

        public string Tag => ConfigService.Singleton.Messages.PlayerTag.Replace("%serverIpPort%", ToString()).Replace("%server%", Name);


        public List<string> PlayersInQueueByUserId = new List<string>();

        public Server GetFallbackServer(Player plr = null)
        {
            Server random = List
                    .Where(x => Settings.FallbackServers.Contains(x.Name))
                    .OrderBy(pair => Settings.FallbackServers.IndexOf(pair.Name))
                    .ToList()
                    .FirstOrDefault();

            return random;
        }

        public bool IsPlayerInQueue(Player plr)
        {
            return PlayersInQueue.ContainsKey(plr.UserId);
        }

        public int GetPlayerPositionInQueue(Player plr)
        {
            if (PlayersInQueue.TryGetValue(plr.UserId, out QueueTicket ticket))
                return ticket.Position;

            return -1;
        }

        public bool AddPlayerToQueue(Player plr)
        {
            if (PlayersInQueue.TryAdd(plr.UserId, new QueueTicket(plr.UserId, this)))
            {
                PlayersInQueueByUserId.Add(plr.UserId);
                Logger.Info($"Added player {plr.UserId} to queue because {Name} is full!, pos {plr.PositionInQueue}/{PlayersInQueueCount}", "QueueService");
                return true;
            }

            return false;
        }

        public void MarkPlayerInQueueAsConnecting(Player plr)
        {
            if (!PlayersInQueue.TryGetValue(plr.UserId, out QueueTicket ticket))
                return;

            ticket.MarkAsConnecting();
            Logger.Info($"{plr.UserId} is connecting from queue to {Name}!", "QueueService");
        }

        /// <summary>
        /// Sends data to XProxy.Plugin.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void SendData(NetDataWriter writer)
        {
            if (!IsConnectedToServer)
                return;

            ConnectionToServer.Send(writer, DeliveryMethod.ReliableOrdered);
        }

        /// <summary>
        /// Invoked when XProxy.Plugin makes connection with proxy.
        /// </summary>
        public void OnConnected(NetPeer peer)
        {
            if (IsConnectedToServer)
                ConnectionToServer.Disconnect();

            ConnectionToServer = peer;
            Logger.Info($"{Tag} Connection to (f=cyan)XProxy.Plugin(f=white) has been (f=green)established(f=white).", "XProxy");
        }

        /// <summary>
        /// Invoked when XProxy.Plugin sends data to proxy.
        /// </summary>
        /// <param name="reader">The reader.</param>
        public void OnReceiveData(NetDataReader reader)
        {
            byte b = reader.GetByte();

            switch (b)
            {
                // Status
                case 0:
                    ServerStatus status = (ServerStatus)reader.GetByte();
                    Status = status;
                    _lastStatusUpdate = DateTime.Now;
                    break;
            }
        }

        /// <summary>
        /// Invoked when XProxy.Plugin loses connection with proxy.
        /// </summary>
        public void OnDisconnected()
        {
            ConnectionToServer = null;
            Logger.Info($"{Tag} Connection to (f=cyan)XProxy.Plugin(f=white) has been (f=red)broken(f=white).", "XProxy");
        }

        public override string ToString() => $"{Settings.Ip}:{Settings.Port}";

        public void Dispose()
        {
            foreach (Player player in Players)
                player.InternalDisconnect();

            foreach (string userId in PlayersInQueueByUserId)
            {
                if (Player.TryGet(userId, out Player plr))
                    plr.InternalDisconnect();
            }

            foreach (Player player in Player.List)
            {
                if (!PlayersInQueueByUserId.Contains(player.UserId))
                    continue;

                player.InternalDisconnect();
            }

            ServersByName.Remove(Name);
        }

    }
}
