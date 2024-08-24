using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using XProxy.Core;
using XProxy.Core.Core.Events.Args;
using XProxy.Core.Events;
using XProxy.Core.Models;
using XProxy.Services;
using XProxy.Shared.Enums;

namespace XProxy.Models
{
    public class ServerInfo
    {
        public ServerInfo(string serverName, string serverDisplayname, string publicIp, string serverIp, int serverPort, int maxPlayers, bool sendIpAddressInPreAuth, ConnectionType connectionType, string simulation, int queueSlots)
        {
            ServerName = serverName;
            ServerDisplayname = serverDisplayname;
            ServerIp = serverIp;
            ServerPublicIp = publicIp;
            ServerPort = serverPort;
            MaxPlayers = maxPlayers;

            ConnectionType = connectionType;
            Simulation = simulation;
            SendIpAddressInPreAuth = sendIpAddressInPreAuth;
            QueueSlots = queueSlots;
        }

        public string ServerName { get;  }
        public string ServerDisplayname { get; }
        public string ServerIp { get; }
        public string ServerPublicIp { get; }
        public int QueueSlots { get; }
        public int ServerPort { get; }

        public int PlayersOnline => PlayersIds.Count;
        public int MaxPlayers { get; }
        public ConnectionType ConnectionType { get; }
        public string Simulation { get; }
        public bool SendIpAddressInPreAuth { get; }

        public List<int> PlayersIds = new List<int>();
        public List<Player> Players => ProxyService.Singleton.Players.Where(x => PlayersIds.Contains(x.Key)).Select(x => x.Value).ToList();

        public bool IsServerFull => PlayersOnline >= MaxPlayers;

        // QUEUE SYSTEM

        public List<string> PlayersInQueueByUserId = new List<string>();

        public ConcurrentDictionary<string, QueueTicket> PlayersInQueue { get; set; } = new ConcurrentDictionary<string, QueueTicket>();

        public int PlayersInQueueCount => PlayersInQueue.Count;

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
                Logger.Info($"Added player {plr.UserId} to queue because {ServerName} is full!, pos {GetPlayerPositionInQueue(plr)}/{PlayersInQueueCount}", "QueueService");
                return true;
            }

            return false;
        }

        public void MarkPlayerInQueueAsConnecting(Player plr)
        {
            if (!PlayersInQueue.TryGetValue(plr.UserId, out QueueTicket ticket))
                return;

            ticket.MarkAsConnecting();
            Logger.Info($"{plr.UserId} is connecting from queue to {ServerName}!", "QueueService");
        }

        // Methods

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

            if (ConnectionType == ConnectionType.Simulated)
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

        public override string ToString()
        {
            return $"{ServerIp}:{ServerPort}";
        }
    }
}
