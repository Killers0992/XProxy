using System;
using System.Collections.Generic;
using System.Linq;
using XProxy.Core;
using XProxy.Enums;
using XProxy.Services;

namespace XProxy.Models
{
    public class ServerInfo
    {
        public ServerInfo(string serverName, string serverDisplayname, string serverIp, int serverPort, int maxPlayers, bool sendIpAddressInPreAuth, ConnectionType connectionType, string simulation)
        {
            ServerName = serverName;
            ServerDisplayname = serverDisplayname;
            ServerIp = serverIp;
            ServerPort = serverPort;
            MaxPlayers = maxPlayers;

            ConnectionType = connectionType;
            Simulation = simulation;
            SendIpAddressInPreAuth = sendIpAddressInPreAuth;
        }

        public string ServerName { get;  }
        public string ServerDisplayname { get; }
        public string ServerIp { get; }
        public int ServerPort { get; }
        public int PlayersOnline => PlayersIds.Count;
        public int MaxPlayers { get; }
        public ConnectionType ConnectionType { get; }
        public string Simulation { get; }
        public bool SendIpAddressInPreAuth { get; }
        public bool IsOnline => (DateTime.Now - LastRespondTime).TotalSeconds < 20;

        public List<int> PlayersIds = new List<int>();
        public List<Player> Players => ProxyService.Singleton.Players.Where(x => PlayersIds.Contains(x.Key)).Select(x => x.Value).ToList();
        public DateTime LastRespondTime { get; private set; } = DateTime.MinValue;
        public List<string> PlayersInQueue = new List<string>();

        public bool IsServerFull => PlayersOnline >= MaxPlayers;

        public void RemoveFromQueue(Player player)
        {
            PlayersInQueue.Remove(player.UserId);
        }

        public void AddToQueue(Player player)
        {
            if (!PlayersInQueue.Contains(player.UserId))
                PlayersInQueue.Add(player.UserId);
        }

        public int GetPositionInQueue(Player player)
        {
            int position = 1;

            var que = PlayersInQueue.ToArray();

            for (int x = 0; x < que.Length; x++)
            {
                if (que[x] == player.UserId)
                    return position;
                position++;
            }

            return -1;
        }

        public bool CanPlayerJoin(Player player)
        {
            if (ConnectionType == ConnectionType.Simulated)
                return true;

            if (player.PreAuth.Flags.HasFlagFast(CentralAuthPreauthFlags.ReservedSlot))
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
