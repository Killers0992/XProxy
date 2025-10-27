using System.Text;

namespace XProxy.API.Commands;

public class CentralCommand
{
    [ConsoleCommand("central")]
    public static void OnCentralCommand(string[] args)
    {
        if (args.Length < 2)
        {
            ProxyLogger.Info("Syntax: central <listenerName> <cmd>", "send");
            return;
        }

        if (!Listener.TryGet(args[0].ToLower(), out Listener listener))
        {
            ProxyLogger.Info($"Listener with name {args[0]} not exists! check \"listeners\" command", "central");
            return;
        }

        string[] rawCmd = args.Skip(1).ToArray();

        string cmd = rawCmd[0].ToLower();
        string[] cmdArgs = rawCmd.Skip(1).ToArray();

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Listeners:");

        Dictionary<string, string> data = new Dictionary<string, string>()
        {
            { "ip", listener.PublicIp },
            { "port", $"{listener.ListenPort}" },
            { "cmd", cmd.Base64Encode() },
            { "args", string.Join(" ", cmdArgs).Base64Encode() },
        };

        if (!string.IsNullOrEmpty(ListService.Password))
            data.Add("passcode", ListService.Password);

        var postResult = listener.Http.PostAsync($"https://api.scpslgame.com/centralcommands/{cmd}.php", new FormUrlEncodedContent(data)).Result;
        string responseText = postResult.Content.ReadAsStringAsync().Result;
        postResult.Dispose();

        ProxyLogger.Info($"[(f=green){cmd}(f=white)] {responseText}", $"central");
    }
}
