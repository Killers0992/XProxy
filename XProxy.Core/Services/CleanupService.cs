using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XProxy.Models;

namespace XProxy.Core.Services
{
    public class CleanupService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    foreach(var itemsToRemove in ProxyServer.ForceServerForUserID.Where(x => x.Value.Time < DateTime.Now))
                    {
                        ProxyServer.ForceServerForUserID.Remove(itemsToRemove.Key, out LastServerInfo _);
                    }
                }
                catch(Exception ex)
                {
                    Logger.Error(ex);
                }
                await Task.Delay(1000);
            }
        }
    }
}
