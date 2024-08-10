using Mirror;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Linq;
using System.Text;
using XProxy.Attributes;
using XProxy.Models;
using XProxy.Services;
using static System.Net.Mime.MediaTypeNames;

namespace XProxy.Commands
{
    public class ProxyCommands
    {
        [ConsoleCommand("help")]
        public static void HelpCommand(CommandsService service, string[] args)
        {
            string text = $"Available commands:";
            foreach(var command in CommandsService.Commands)
            {
                if (command.Key == "help") continue;

                text += $"\n - {command.Key}";
            }
            Logger.Info(text, "help");
        }

        [ConsoleCommand("texttohash")]
        public static void TextToHashCommand(CommandsService service, string[] args)
        {
            if (args.Length == 0)
            {
                Logger.Info("Syntax: texttohash <fullName>", "send");
                return;
            }

            int hash = 23;
            foreach (char c in args[0])
            {
                hash = hash * 31 + c;
            }

            Logger.Info((ushort)hash, "texttohash");
        }

        [ConsoleCommand("servers")]
        public static void ServersCommand(CommandsService service, string[] args)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Servers:");
            foreach (var server in ProxyService.Singleton.Servers)
            {
                sb.AppendLine($" - (f=cyan){server.Value.ServerName}(f=white) [ (f=green){server.Value.PlayersOnline}/{server.Value.MaxPlayers}(f=white) ] ((f=darkcyan){server.Value}(f=white))");
            }
            Logger.Info(sb.ToString(), "servers");
        }

        [ConsoleCommand("players")]
        public static void PlayersCommand(CommandsService service, string[] args)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Players on servers:");
            foreach(var server in ProxyService.Singleton.Servers)
            {
                sb.AppendLine($" - Server (f=cyan){server.Value.ServerName}(f=white) [ (f=green){server.Value.PlayersOnline}/{server.Value.MaxPlayers}(f=white) ] ((f=darkcyan){server.Value}(f=white)){(server.Value.PlayersInQueue > 0 ? $" [ in queue (f=green){server.Value.PlayersInQueue}(f=white) ]" : string.Empty)}");
                foreach(var player in server.Value.Players.OrderBy(x => x.IsInQueue))
                {
                    sb.AppendLine($"  - {(player.IsInQueue ? $"[(f=red)Queue (f=white) (f=green){player.PositionInQueue}(f=white)/(f=yellow){player.ServerInfo.PlayersInQueue}(f=white)] " : string.Empty)}[(f=green){player.Id}(f=white)] (f=cyan){player.UserId}(f=white) connection time (f=darkcyan){player.Connectiontime.ToReadableString()}(f=white)");
                }
            }
            sb.AppendLine($" Total online players (f=green){ProxyService.Singleton.Players.Count}/{service.Config.Value.MaxPlayers}(f=white)!");
            Logger.Info(sb.ToString(), "players");
        }

        [ConsoleCommand("send")]
        public static void OnSendCommand(CommandsService service, string[] args)
        {
            if (args.Length < 2)
            {
                Logger.Info("Syntax: send <all/player> <server>", "send");
                return;
            }

            string serverName = string.Join(" ", args.Skip(1));

            ServerInfo server = ProxyService.Singleton.GetServerByName(serverName);

            if (server == null)
            {
                Logger.Info($"Server with name {args[1]} not exists!", "send");
                return;
            }

            switch (args[0].ToLower())
            {
                case "all":
                    int sent = 0;
                    foreach(var player in ProxyService.Singleton.Players.Values)
                    {
                        if (player.ServerInfo == server) continue;

                        if (player.RedirectTo(server.ServerName))
                        {
                            sent++;
                            player.SendHint($"Connecting to <color=green>{server.ServerName}</color>...", 10f);
                        }
                        else
                            player.SendHint($"<color=red>Server <color=green>{server.ServerName}</color> is full...</color>", 3f);
                    }

                    Logger.Info($"Sent (f=green){sent}(f=white) players to server (f=green){server.ServerName}(f=white)", "send");
                    break;
                default:
                    if (args[1].Contains("@"))
                    {
                        var targetPlayer = ProxyService.Singleton.Players.Values.Where(x => x.UserId == args[1]).FirstOrDefault();

                        if (targetPlayer == null)
                        {
                            Logger.Info($"Player with userid {args[1]} not exists!", "send");
                            return;
                        }

                        if (targetPlayer.RedirectTo(server.ServerName))
                            targetPlayer.SendHint($"Connecting to <color=green>{server.ServerName}</color>...", 10f);
                        else
                            targetPlayer.SendHint($"<color=red>Server <color=green>{server.ServerName}</color> is full...</color>", 3f);

                        Logger.Info($"Sent (f=green){targetPlayer.UserId} to server (f=green){server.ServerName}(f=white)", "send");
                    }
                    else
                    {
                        Logger.Info($"You need to use format ID@steam, ID@discord, ID@northwood!", "send");
                    }
                    return;
            }
        }

        [ConsoleCommand("maintenance")]
        public static void Maintenance(CommandsService service, string[] args)
        {
            if (args.Length == 0)
            {
                Logger.Info("Syntax: maintenance toggle/servername/kickmessage", "maintenance");
                return;
            }

            switch (args[0].ToLower())
            {
                default:
                    Logger.Info("Syntax: maintenance toggle/servername", "maintenance");
                    break;
                case "toggle":
                    service.Config.Value.MaintenanceMode = !service.Config.Value.MaintenanceMode;
                    service.Config.Save();

                    Logger.Info($"Maintenance is now {(service.Config.Value.MaintenanceMode ? "(f=green)enabled(f=white)" : "(f=red)disabled(f=white)")}!", "maintenance");
                    break;
                case "servername":
                    if (args.Length < 2)
                    {
                        Logger.Info("Syntax: maintenance servername (message)", "maintenance");
                        break;
                    }

                    string name = string.Join(" ", args.Skip(1));

                    service.Config.Value.MaintenanceServerName = name;
                    service.Config.Save();

                    Logger.Info($"Maintenance server name set to {name}", "maintenance");
                    break;
            }
        }

        [ConsoleCommand("reload")]
        public static void ReloadCommand(CommandsService service, string[] args)
        {
            service.Config.Load();
            Logger.Info($"Reloaded proxy.", "reload");
        }

        [ConsoleCommand("sendhint")]
        public static void SendHintCommand(CommandsService service, string[] args)
        {
            if (args.Length == 0)
            {
                Logger.Info("Syntax: sendhint <message>", "sendhint");
                return;
            }

            string message = string.Join(" ", args);

            foreach (var client in ProxyService.Singleton.Players.Values)
            {
                client.SendHint(message);
            }
            Logger.Info($"Send hint with message (f=green){message}(f=white) to {ProxyService.Singleton.Players.Count} players", "sendhint");
        }

        [ConsoleCommand("broadcast")]
        public static void BroadcastCommand(CommandsService service, string[] args)
        {
            if (args.Length == 0)
            {
                Logger.Info("Syntax: broadcast <message>", "broadcast");
                return;
            }

            string message = string.Join(" ", args);

            foreach (var client in ProxyService.Singleton.Players.Values)
            {
                client.SendBroadcast(message, 3, Broadcast.BroadcastFlags.Normal);
            }
            Logger.Info($"Send broadcast with message (f=green){message}(f=white) to {ProxyService.Singleton.Players.Count} players", "broadcast");
        }
    }
}
