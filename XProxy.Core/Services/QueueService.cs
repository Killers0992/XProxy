using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XProxy.Core.Models;
using XProxy.Models;
using XProxy.Services;

namespace XProxy.Core.Services
{
    public class QueueService : BackgroundService
    {
        public static ConcurrentDictionary<string, List<QueueTicket>> PlayersInQueue = new ConcurrentDictionary<string, List<QueueTicket>>();

        public static ConcurrentQueue<(string, string)> PlayersAddToQueue = new ConcurrentQueue<(string, string)>();

        public static ConcurrentQueue<string> PlayersRemoveFromQueue = new ConcurrentQueue<string>();

        public static Dictionary<string, string> PlayersInQueueDict = new Dictionary<string, string>();

        public static void AddPlayerToQueue(Player plr)
        {
            PlayersAddToQueue.Enqueue((plr.UserId, plr.ServerInfo.ServerName));
            Logger.Info($"Queue slot for {plr.UserId} assigned ( server {plr.ServerInfo.ServerName} )", "QueueService");
        }

        public static void RemoveFromQueue(Player plr, bool onConnect)
        {
            if (onConnect)
            {
                if (plr.PositionInQueue == 1)
                    Logger.Info($"Remove {plr.UserId} from queue ( player joined )", "QueueService");
            }
            else
            {
                Logger.Info($"Remove {plr.UserId} from queue ( player left )", "QueueService");
            }

            PlayersRemoveFromQueue.Enqueue(plr.UserId);
        }

        public static void UpdatePlayerInQueue(Player plr)
        {
            if (!PlayersInQueue.TryGetValue(plr.ServerInfo.ServerName, out List<QueueTicket> tickets))
                return;

            if (!PlayersInQueueDict.ContainsKey(plr.UserId))
                PlayersInQueueDict.Add(plr.UserId, plr.ServerInfo.ServerName);

            foreach (var ticket in tickets)
            {
                if (ticket.UserId != plr.UserId)
                    continue;

                ticket.TicketLifetime = DateTime.Now.AddSeconds(ConfigService.Instance.Value.QueueTicketLifetime);
                ticket.IsConnecting = true;
                Logger.Info($"Queue slot for {plr.UserId} updated for connecting", "QueueService");
            }
        }

        public static bool IsPlayerInQueue(Player plr)
        {
            if (PlayersInQueue.TryGetValue(plr.ServerInfo.ServerName, out List<QueueTicket> players))
                return players.Any(x => x.UserId == plr.UserId);

            return false;
        }

        public static int GetPositionInQueue(Player plr)
        {
            if (plr.ServerInfo == null)
                return -1;

            if (PlayersInQueue.TryGetValue(plr.ServerInfo.ServerName, out List<QueueTicket> players))
            {
                int position = 1;

                var que = players.ToArray();

                for (int x = 0; x < que.Length; x++)
                {
                    if (que[x] == null) continue;

                    if (que[x].UserId == plr.UserId)
                        return position;

                    position++;
                }

                return -1;
            }

            return -1;
        }

        public static int GetPlayersInQueueCount(ServerInfo serverInfo)
        {
            if (PlayersInQueue.TryGetValue(serverInfo.ServerName, out List<QueueTicket> plrs))
                return plrs.Count();

            return 0;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                try
                {
                    while (PlayersAddToQueue.Count > 0)
                    {
                        if (PlayersAddToQueue.TryDequeue(out (string, string) plr))
                        {
                            if (PlayersInQueue.ContainsKey(plr.Item2))
                                PlayersInQueue[plr.Item2].Add(new QueueTicket()
                                {
                                    ServerKey = plr.Item2,
                                    UserId = plr.Item1,
                                });
                            else
                                PlayersInQueue.TryAdd(plr.Item2, new List<QueueTicket>() { 
                                    new QueueTicket()
                                    {
                                        ServerKey = plr.Item2,
                                        UserId = plr.Item1,
                                    } 
                                });
                        }
                    }

                    while(PlayersRemoveFromQueue.Count > 0)
                    {
                        if (PlayersRemoveFromQueue.TryDequeue(out string userId))
                        {
                            foreach(var server in PlayersInQueue)
                            {
                                server.Value.RemoveAll(x => x.UserId == userId);
                            }
                        }
                    }

                    foreach (var server in PlayersInQueue)
                    {
                        var itemsToRemove = new List<QueueTicket>();
                        foreach(var plr in server.Value)
                        {
                            if (plr.IsConnecting && plr.TicketLifetime < DateTime.Now)
                            {
                                itemsToRemove.Add(plr);
                            }
                        }

                        foreach (var playerItem in itemsToRemove)
                        {
                            server.Value.Remove(playerItem);
                            Logger.Info($"Queue slot for {playerItem.UserId} expired ( server {playerItem.ServerKey} )", "QueueService");
                        }
                    }
                }
                catch (Exception ex) 
                {
                    Logger.Error(ex, "QueueService");
                }

                await Task.Delay(10);
            }
        }
    }
}
