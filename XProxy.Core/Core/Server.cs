using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using XProxy.Core.Core.Events.Args;
using XProxy.Core.Events;
using XProxy.Core.Models;
using XProxy.Services;
using XProxy.Shared.Enums;
using XProxy.Shared.Models;

namespace XProxy.Core
{
    public class Server
    {
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

        public static void Refresh()
        {
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
                    foreach (Player player in serv.Players)
                        player.InternalDisconnect();

                    foreach (var player in Listener.GetAllPlayers())
                    {
                        if (!serv.PlayersInQueueByUserId.Contains(player.UserId))
                            continue;

                        player.InternalDisconnect();
                    }

                    serv.Destroy();
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
        public List<string> PlayersInQueueByUserId = new List<string>();

        // New Priority Queue
        public Queue<Player> PriorityQueue { get; set; } = new Queue<Player>();
        public Queue<Player> RegularQueue { get; set; } = new Queue<Player>();

        /// <summary>
        /// Gets amount of players in queue.
        /// </summary>
        public int PlayersInQueueCount => PriorityQueue.Count + RegularQueue.Count;

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

        public bool CanPlayerJoin(Player player)
        {
            PlayerCanJoinEvent ev = new PlayerCanJoinEvent(player, this);
            EventManager.Player.InvokeCanJoin(ev);
            if (ev.ForceDeny)
                return false;

            if (ev.ForceAllow)
                return true;

            if (Settings.ConnectionType == ConnectionType.Simulated)
                return true;

            if (player.PreAuth.Flags.HasFlagFast(CentralAuthPreauthFlags.ReservedSlot))
                return true;

            if (player.PreAuth.Flags.HasFlagFast(CentralAuthPreauthFlags.NorthwoodStaff) && ConfigService.Singleton.Value.NorthwoodStaffIgnoresSlots)
                return true;

            if (player.IsInQueue)
            {
                if (player.PositionInQueue == 1)
                    return !IsServerFull;
                else
                    return false;
            }
            else if (player.CanJoinQueue(this))
                return true;

            if (PlayersInQueueCount > 0)
                return false;

            return !IsServerFull;
        }

        public bool IsPlayerInQueue(Player plr)
        {
            return PriorityQueue.Contains(plr) || RegularQueue.Contains(plr);
        }

        public int GetPlayerPositionInQueue(Player plr)
        {
            if (PriorityQueue.Contains(plr))
                return PriorityQueue.ToList().IndexOf(plr) + 1;
            if (RegularQueue.Contains(plr))
                return PriorityQueue.Count + RegularQueue.ToList().IndexOf(plr) + 1;

            return -1;
        }

        public void AddPlayerToQueue(Player player)
        {
            if (!IsPlayerInQueue(player))
            {
                RegularQueue.Enqueue(player);
                Logger.Info($"Added player {player.UserId} to regular queue for server {Name}", "QueueService");
            }
        }

        public void AddPlayerToPriorityQueue(Player player)
        {
            if (!IsPlayerInQueue(player))
            {
                PriorityQueue.Enqueue(player);
                Logger.Info($"Added player {player.UserId} to priority queue for server {Name}", "QueueService");
            }
        }

        public Player GetNextPlayerInQueue()
        {
            if (PriorityQueue.Count > 0)
            {
                return PriorityQueue.Dequeue();
            }
            else if (RegularQueue.Count > 0)
            {
                return RegularQueue.Dequeue();
            }

            return null;
        }

        public void MarkPlayerInQueueAsConnecting(Player plr)
        {
            if (PriorityQueue.Contains(plr))
            {
                Logger.Info($"{plr.UserId} is connecting from priority queue to {Name}!", "QueueService");
            }
            else if (RegularQueue.Contains(plr))
            {
                Logger.Info($"{plr.UserId} is connecting from regular queue to {Name}!", "QueueService");
            }
        }

        public void Destroy()
        {
            ServersByName.Remove(Name);
        }

        public override string ToString()
        {
            return $"{Settings.Ip}:{Settings.Port}";
        }
    }
}
