using System;
using System.Linq;
using System.Text;
using XProxy.Attributes;
using XProxy.Core;
using XProxy.Models;
using XProxy.Services;

namespace XProxy.Commands
{
    public class ProxyCommands
    {
        [ConsoleCommand("help")]
        public static void HelpCommand(CommandsService service, string[] args)
        {
            string text = $"Available commands:";
            foreach (var command in CommandsService.Commands)
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
            foreach (var server in Server.List)
            {
                sb.AppendLine($" - (f=cyan){server.Name}(f=white) [ (f=green){server.PlayersCount}/{server.Settings.MaxPlayers}(f=white) ] ((f=darkcyan){server}(f=white))");
            }
            Logger.Info(sb.ToString(), "servers");
        }

        [ConsoleCommand("players")]
        public static void PlayersCommand(CommandsService service, string[] args)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Players on servers:");
            foreach (var server in Server.List)
            {
                sb.AppendLine($" - Server (f=cyan){server.Name}(f=white) [ (f=green){server.PlayersCount}/{server.Settings.MaxPlayers}(f=white) ] ((f=darkcyan){server}(f=white))");
                sb.AppendLine($"  -> On Server  ");
                foreach (var player in server.Players.OrderBy(x => x.IsInQueue).OrderBy(x => x.PositionInQueue))
                {
                    sb.AppendLine($"  [(f=green){player.Id}(f=white)] (f=cyan){player.UserId}(f=white) connection time (f=darkcyan){player.Connectiontime.ToReadableString()}(f=white)");
                }

                if (server.PlayersInQueueCount > 0)
                {
                    sb.AppendLine($"  -> In Queue  ");
                    foreach (var queuePlayers in server.PlayersInQueue.OrderBy(x => x.Value.Position))
                    {
                        var plr = ProxyService.Singleton.GetPlayerByUserId(queuePlayers.Key);

                        sb.AppendLine($"  [(f=green){queuePlayers.Value.Position}(f=white)/(f=green){server.PlayersInQueueCount}(f=white)] (f=cyan){queuePlayers.Key}(f=white) {(plr == null ? $"(f=darkred)OFFLINE(f=white) ( slot expires in few seconds )" : $"connection time (f=darkcyan){plr.Connectiontime.ToReadableString()}(f=white)")}");
                    }
                }
            }
            sb.AppendLine($" Total online players (f=green){ProxyService.Singleton.Players.Count}/{ConfigService.Singleton.Value.MaxPlayers}(f=white)!");
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

            if (!Server.TryGetByName(serverName, out Server server))
            {
                Logger.Info($"Server with name {args[1]} not exists!", "send");
                return;
            }

            switch (args[0].ToLower())
            {
                case "all":
                    int sent = 0;
                    foreach (var player in ProxyService.Singleton.Players.Values)
                    {
                        if (player.ServerInfo == server) continue;

                        if (player.RedirectTo(server.Name))
                        {
                            sent++;
                            player.SendHint($"Connecting to <color=green>{server.Name}</color>...", 10f);
                        }
                        else
                            player.SendHint($"<color=red>Server <color=green>{server.Name}</color> is full...</color>", 3f);
                    }

                    Logger.Info($"Sent (f=green){sent}(f=white) players to server (f=green){server.Name}(f=white)", "send");
                    break;
                default:
                    if (args[0].Contains("@"))
                    {
                        var targetPlayer = ProxyService.Singleton.Players.Values.Where(x => x.UserId == args[0]).FirstOrDefault();

                        if (targetPlayer == null)
                        {
                            Logger.Info($"Player with userid {args[0]} not exists!", "send");
                            return;
                        }

                        if (targetPlayer.RedirectTo(server.Name))
                            targetPlayer.SendHint($"Connecting to <color=green>{server.Name}</color>...", 10f);
                        else
                            targetPlayer.SendHint($"<color=red>Server <color=green>{server.Name}</color> is full...</color>", 3f);

                        Logger.Info($"Sent (f=green){targetPlayer.UserId} to server (f=green){server.Name}(f=white)", "send");
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
                    ConfigService.Singleton.Value.MaintenanceMode = !ConfigService.Singleton.Value.MaintenanceMode;
                    ConfigService.Singleton.Save();

                    Logger.Info($"Maintenance is now {(ConfigService.Singleton.Value.MaintenanceMode ? "(f=green)enabled(f=white)" : "(f=red)disabled(f=white)")}!", "maintenance");
                    break;
                case "servername":
                    if (args.Length < 2)
                    {
                        Logger.Info("Syntax: maintenance servername (message)", "maintenance");
                        break;
                    }

                    string name = string.Join(" ", args.Skip(1));

                    ConfigService.Singleton.Value.MaintenanceServerName = name;
                    ConfigService.Singleton.Save();

                    Logger.Info($"Maintenance server name set to {name}", "maintenance");
                    break;
            }
        }

        [ConsoleCommand("reload")]
        public static void ReloadCommand(CommandsService service, string[] args)
        {
            ConfigService.Singleton.Load();
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
