using System.Text;

namespace XProxy.API.Commands;

public class ServersCommand
{
    [ConsoleCommand("servers")]
    public static void OnServerCommand(string[] args)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Servers:");

        foreach (var server in Server.List)
        {
            sb.AppendLine($" - (f=cyan){server.Name}(f=white) [ (f=green){server.Clients.Count}(f=white) ] ((f=darkcyan){server}(f=white))");
        }

        ProxyLogger.Info(sb.ToString(), "servers");
    }
}
