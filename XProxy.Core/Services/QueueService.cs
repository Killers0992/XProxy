using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using XProxy.Services;

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
                    foreach (var server in ProxyService.Singleton.Servers.Values)
                    {
                        foreach(var ticket in server.PlayersInQueue)
                        {
                            if (!ticket.Value.IsTicketExpired()) continue;

                            Logger.Info($"Queue slot for {ticket.Key} expired! ( server {server.ServerName} )", "QueueService");
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
