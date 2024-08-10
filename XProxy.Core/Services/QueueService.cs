using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using XProxy.Models;

namespace XProxy.Core.Services
{
    public class QueueService : BackgroundService
    {
        public static ConcurrentDictionary<ServerInfo, List<Player>> PlayersInQueue = new ConcurrentDictionary<ServerInfo, List<Player>>();

        public static ConcurrentQueue<Player> PlayersAddToQueue = new ConcurrentQueue<Player>();

        public static bool IsPlayerInQueue(Player plr)
        {
            if (PlayersInQueue.TryGetValue(plr.ServerInfo, out List<Player> players))
                return players.Contains(plr);

            return false;
        }

        public static int GetPositionInQueue(Player plr)
        {
            if (PlayersInQueue.TryGetValue(plr.ServerInfo, out List<Player> players))
            {
                int position = 1;

                var que = players.ToArray();

                for (int x = 0; x < que.Length; x++)
                {
                    if (que[x] == plr)
                        return position;

                    position++;
                }

                return -1;
            }

            return -1;
        }

        public static int GetPlayersInQueueCount(ServerInfo serverInfo)
        {
            if (PlayersInQueue.TryGetValue(serverInfo, out List<Player> plrs))
                return plrs.Count;

            return 0;
        }

        public static void PlayerLeft(Player plr)
        {
            if (PlayersInQueue.TryGetValue(plr.ServerInfo, out List<Player> players))
            {
                players.Remove(plr);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                try
                {
                    while (PlayersAddToQueue.Count > 0)
                    {
                        if (PlayersAddToQueue.TryDequeue(out Player plr))
                        {
                            if (PlayersInQueue.ContainsKey(plr.ServerInfo))
                                PlayersInQueue[plr.ServerInfo].Add(plr);
                            else
                                PlayersInQueue.TryAdd(plr.ServerInfo, new List<Player>() { plr });
                        }
                    }

                }
                catch (Exception) { }

                await Task.Delay(10);
            }
        }
    }
}
