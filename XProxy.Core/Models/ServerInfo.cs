using System;
using System.Collections.Generic;
using System.Linq;
using XProxy.Core;
using XProxy.Core.Services;
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
        public int PlayersOnline => Math.Clamp(PlayersIds.Count - PlayersInQueue, 0, int.MaxValue);
        public int PlayersInQueue => QueueService.GetPlayersInQueueCount(this);
        public int MaxPlayers { get; }
        public ConnectionType ConnectionType { get; }
        public string Simulation { get; }
        public bool SendIpAddressInPreAuth { get; }
        public bool IsOnline => (DateTime.Now - LastRespondTime).TotalSeconds < 20;

        public List<int> PlayersIds = new List<int>();
        public List<Player> Players => ProxyService.Singleton.Players.Where(x => PlayersIds.Contains(x.Key)).Select(x => x.Value).ToList();

        public DateTime LastRespondTime { get; private set; } = DateTime.MinValue;

        public bool IsServerFull => PlayersOnline >= MaxPlayers;

        public bool CanPlayerJoin(Player player)
        {
            if (ConnectionType == ConnectionType.Simulated)
                return true;

            if (player.PreAuth.Flags.HasFlagFast(CentralAuthPreauthFlags.ReservedSlot))
                return true;

            if (player.PreAuth.Flags.HasFlagFast(CentralAuthPreauthFlags.NorthwoodStaff) && ConfigService.Instance.Value.NorthwoodStaffIgnoresSlots)
                return true;

            if (player.PositionInQueue == 1)
                return !IsServerFull;

            if (player.PositionInQueue > 0)
                return false;

            if (QueueSlots > PlayersInQueue)
                return true;

            return !IsServerFull;
        }

        public void SetOffline() => LastRespondTime = DateTime.MinValue;

        public void SetOnline() => LastRespondTime = DateTime.Now;

        public override string ToString()
        {
            return $"{ServerIp}:{ServerPort}";
        }
    }
}
