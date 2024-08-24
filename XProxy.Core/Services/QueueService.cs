using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace XProxy.Core.Services
{
    public class QueueService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                try
                {
                    foreach (var server in Server.List)
                    {
                        foreach(var ticket in server.PlayersInQueue)
                        {
                            if (!ticket.Value.IsTicketExpired()) continue;

                            if (ticket.Value.IsConnecting)
                            {
                                Logger.Info($"Player {ticket.Key} joined server {server.Name} from queue successfully! ( freed slot )", "QueueService");

                            }
                            else
                            {
                                Logger.Info($"Queue slot for {ticket.Key} expired! ( server {server.Name} )", "QueueService");
                            }

                            server.PlayersInQueueByUserId.Remove(ticket.Value.UserId);
                            server.PlayersInQueue.TryRemove(ticket.Key, out _);
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
