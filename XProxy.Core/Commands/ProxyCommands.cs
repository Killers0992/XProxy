using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using XProxy.Attributes;
using XProxy.Core;
using XProxy.Core.Monitors;
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

        [ConsoleCommand("listeners")]
        public static void ListenersCommand(CommandsService service, string[] args)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Listeners:");
            foreach (Listener listener in Listener.List)
            {
                sb.AppendLine($" - (f=cyan){listener.Settings.ListenIp}:{listener.Settings.Port}(f=white) [ (f=green){listener.Connections.Count}/{listener.Settings.MaxPlayers}(f=white) ] ((f=darkcyan){listener.ListenerName}(f=white))");
            }
            Logger.Info(sb.ToString(), "listeners");
        }

        [ConsoleCommand("runcentralcmd")]
        public static void RunCentralCommand(CommandsService service, string[] args)
        {
            if (args.Length < 2)
            {
                Logger.Info("Syntax: runcentralcmd <listenerName> <cmd>", "send");
                return;
            }

            if (!Listener.TryGet(args[0], out Listener listener))
            {
                Logger.Info($"Listener with name {args[0]} not exists! check \"listeners\" command", "runcentralcmd");
                return;
            }

            string[] rawCmd = args.Skip(1).ToArray();

            string cmd = rawCmd[0].ToLower();
            string[] cmdArgs = rawCmd.Skip(1).ToArray();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Listeners:");

            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "ip", listener.Settings.PublicIp },
                { "port", $"{listener.Settings.Port}" },
                { "cmd", ListService.Base64Encode(cmd) },
                { "args", ListService.Base64Encode(string.Join(" ", cmdArgs)) },
            };

            if (!string.IsNullOrEmpty(ListService.Password))
                data.Add("passcode", ListService.Password);

            var postResult = listener.Settings.Http.PostAsync($"https://api.scpslgame.com/centralcommands/{cmd}.php", new FormUrlEncodedContent(data)).Result;
            string responseText = postResult.Content.ReadAsStringAsync().Result;
            postResult.Dispose();

            Logger.Info(ConfigService.Singleton.Messages.CentralCommandMessage.Replace("%command%", cmd).Replace("%message%", responseText), $"runcentralcmd");
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
                        Player plr = Player.Get(queuePlayers.Key);

                        sb.AppendLine($"  [(f=green){queuePlayers.Value.Position}(f=white)/(f=green){server.PlayersInQueueCount}(f=white)] (f=cyan){queuePlayers.Key}(f=white) {(plr == null ? $"(f=darkred)OFFLINE(f=white) ( slot expires in few seconds )" : $"connection time (f=darkcyan){plr.Connectiontime.ToReadableString()}(f=white)")}");
                    }
                }
            }
            sb.AppendLine($" Total online players (f=green){Player.Count}(f=white)!");
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
                    foreach (Player player in Player.List)
                    {
                        if (player.CurrentServer == server) 
                            continue;

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
                        if (!Player.TryGet(args[0], out Player targetPlayer))
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

            foreach (Player plr in Player.List)
            {
                plr.SendHint(message);
            }

            Logger.Info($"Send hint with message (f=green){message}(f=white) to {Player.Count} players", "sendhint");
        }

        [ConsoleCommand("broadcast")]
        public static void BroadcastCommand(CommandsService service, string[] args)
        {
            if (args.Length < 2)
            {
                Logger.Info("Syntax: broadcast <duration> <message>", "broadcast");
                return;
            }
            
            if (!int.TryParse(args[0], out int duration) || duration <= 0 || duration > ushort.MaxValue)
            {
                Logger.Info("Invalid duration. Please enter a positive number up to 65535.", "broadcast");
                return;
            }
            
            ushort broadcastDuration = (ushort)duration;
            
            string message = string.Join(" ", args.Skip(1));

            foreach (Player plr in Player.List)
            {
                plr.SendBroadcast(message, broadcastDuration, Broadcast.BroadcastFlags.Normal);
            }

            Logger.Info($"Sent broadcast with message (f=green){message}(f=white) for (f=green){broadcastDuration}(f=white) seconds to (f=green){Player.Count}(f=white) players", "broadcast");
        }

        [ConsoleCommand("stats")]
        public static void StatsCommand(CommandsService service, string[] args)
        {
            Logger.Info($"XProxy Stats");
            Logger.Info($" - CPU Usage {CpuProfiler.GetCpuUsage():0.00}%");
            Logger.Info($" - Running Tasks: {TaskMonitor.GetRunningTaskCount()}");
        }

        [ConsoleCommand("debuginfo")]
        public static void DebugInfoCommand(CommandsService service, string[] args)
        {
            foreach(Server server in Server.List)
            {
                Logger.Info("Server " + server);
                Logger.Info(" IsOnline " + server.IsServerOnline);
                Logger.Info(" IsFull " + server.IsServerFull);
                Logger.Info(" IsConnected to plugin " + server.IsConnectedToServer);
                Logger.Info(" Players " + server.PlayersCount);
                Logger.Info($" PlayersById ( count {server.PlayersById.Count} )");
                foreach (var player in server.PlayersById)
                {
                    Logger.Info($" [{player.Key}] {player.Value.UserId}");
                }
                Logger.Info($" PlayersInQueue ( count {server.PlayersInQueue.Count} )");
                foreach (var player in server.PlayersInQueue)
                {
                    Logger.Info($" [{player.Key}] {player.Value.UserId} ( is connecting {player.Value.IsConnecting} )");
                }
                Logger.Info($" PlayersInQueueByUserId ( count {server.PlayersInQueueByUserId.Count} )");
                int pos = 0;
                foreach (var player in server.PlayersInQueueByUserId)
                {
                    pos++;
                    Logger.Info($" {pos}/{server.PlayersInQueueCount} [{player}]");
                }
            }

            foreach(Listener listener in Listener.List)
            {
                Logger.Info("Listener " + listener.ListenerName);
                Logger.Info(" Peers connected " + listener._manager.ConnectedPeersCount);
                foreach(var peer in listener._manager.ConnectedPeerList)
                {
                    Logger.Info($" [{peer.Id}] {peer.EndPoint.Address} ( ping {peer.Ping}ms, state {peer.ConnectionState}, time since last packet {peer.TimeSinceLastPacket} )");
                }
            }
        }
    }
}
