using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using XProxy.Core.Core.Events.Args;
using XProxy.Core.Events;
using XProxy.Core.Models;
using XProxy.Enums;
using XProxy.Models;
using XProxy.Services;

namespace XProxy.Core
{
    public class Server
    {
        public static Dictionary<string, Server> ServersByName = new Dictionary<string, Server>();
        public static List<Server> List => ServersByName.Values.ToList();

        public static List<string> GetServerNames(Player plr)
        {
            List<string> names = new List<string>();

            foreach(var server in List)
            {
                if (server.Name == plr.CurrentServer.Name)
                    continue;

                names.Add(server.Name);
            }

            return names;
        }

        public static bool TryGetByName(string name , out Server server)
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

                    foreach(var player in Listener.GetAllPlayers())
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

        public List<string> PlayersInQueueByUserId = new List<string>();

        public bool CanPlayerJoin(Player player)
        {
            //
            PlayerCanJoinEvent ev = new PlayerCanJoinEvent(player, this);
            EventManager.Player.InvokeCanJoin(ev);
            if (ev.ForceDeny)
                return false;

            if (ev.ForceAllow)
                return true;
            //

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
