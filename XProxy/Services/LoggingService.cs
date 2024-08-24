using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XProxy.Shared;

namespace XProxy.Services
{
    public class LoggingService : BackgroundService
    {
        static void WriteLogToFile(object message)
        {
            if (!Directory.Exists("logs"))
                Directory.CreateDirectory("logs");

            File.AppendAllLines($"logs/log_{Logger.SessionTime.ToString("dd_MM_yyyy_hh_mm_ss")}.txt", new string[1] { message.ToString() });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    while (Logger.NewLogEntry.Count != 0)
                    {
                        if (Logger.NewLogEntry.TryDequeue(out string entry))
                        {
                            WriteLogToFile(Logger.FormatAnsi(entry, true));
                            Console.WriteLine(Logger.FormatAnsi(entry));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "XProxy");
                }

                await Task.Delay(10);
            }
        }
    }
}
