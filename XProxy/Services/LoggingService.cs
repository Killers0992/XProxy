using XProxy.API.Misc;

namespace XProxy.Services;

public class LoggingService : BackgroundService
{
    static void WriteLogToFile(object message)
    {
        if (!Directory.Exists("Logs"))
            Directory.CreateDirectory("Logs");

        File.AppendAllLines($"Logs/log_{ProxyLogger.SessionTime.ToString("dd_MM_yyyy_hh_mm_ss")}.txt", [message.ToString()] );
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {

                while (ProxyLogger.NewLogEntry.Count != 0)
                {
                    if (ProxyLogger.NewLogEntry.TryDequeue(out string entry))
                    {
                        WriteLogToFile(entry.FormatAnsi(true));
                        Console.WriteLine(entry.FormatAnsi());
                    }
                }
            }
            catch (Exception ex)
            {
                ProxyLogger.Error(ex);
            }

            await Task.Delay(1000);
        }
    }
}
