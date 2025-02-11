using Mirror;
using System.Collections.Generic;
using System.Linq;
using XProxy.Core.Core.Connections.Responses;
using XProxy.Services;

namespace XProxy.Core.Connections
{
    public class LobbyConnection : SimulatedConnection
    {
        public const ushort NoclipToggleId = 48298;
        public const ushort VoiceMessageId = 41876;

        public List<string> Servers;
        public int CurrentIndex;

        public bool Connect;
        public bool IsConnecting;

        public string InfoMessage;
        public int InfoMessageDuration;

        public LobbyConnection(Player plr) : base(plr)
        {
            Servers = Server.GetServerNames(Player);
        }

        public override void OnConnected()
        {
            Logger.Info(ConfigService.Singleton.Messages.LobbyConnectedMessage.Replace("%tag%", Player.Tag).Replace("%address%", $"{Player.ClientEndPoint}").Replace("%userid%", Player.UserId), $"Player");
            Player.SendToScene("Facility");
        }

        public override void OnClientReady()
        {
            Player.ObjectSpawnStarted();
            Player.ObjectSpawnFinished();
        }

        public override void OnAddPlayer()
        {
            Player.Spawn();
            Player.SetRole(PlayerRoles.RoleTypeId.Tutorial);
            Player.SetHealth(100f);
        }

        public void ShowInfo(string message, int duration)
        {
            InfoMessage = message;
            InfoMessageDuration = duration;
        }

        public override void Update()
        {
            if (InfoMessageDuration != 0)
                InfoMessageDuration--;

            if (Connect && !IsConnecting)
            {
                string serverName = Servers[CurrentIndex];

                if (!Server.TryGetByName(serverName, out Server server))
                {
                    ShowInfo($"Server <color=green>{serverName}</color> not found!", 3);
                    Connect = false;
                    return;
                }

                Player.ConnectTo(server);
                IsConnecting = true;
            }

            SendInfoHint();
        }

        public override void OnConnectionResponse(Server server, BaseResponse response)
        {
            switch (response)
            {
                case ServerIsFullResponse _:
                    ShowInfo($"Server <color=green>{server.Name}</color> is full!", 3);
                    break;
                case ServerIsOfflineResponse _:
                    ShowInfo($"Server <color=green>{server.Name}</color> is offline!", 3);
                    break;
            }

            IsConnecting = false;
            Connect = false;
        }

        public void SendInfoHint(float dur = 1.2f)
        {
            HintBuilder builder = new HintBuilder();

            int startingLine = 12;

            startingLine = startingLine - (Server.List.Count >= 12 ? 0 : Server.List.Count);

            builder.SetRightLine(startingLine, "<mark=#00000082>  Servers                                     </mark>‎‎");

            for(int x = 0; x < 16; x++)
            {
                Server serv = Server.List.ElementAtOrDefault(x);
                
                if (serv == null)
                    break;

                if (serv == Player.CurrentServer)
                    continue;

                int serverIndex = Servers.IndexOf(serv.Name);

                startingLine++;
                builder.SetRightLine(startingLine, $"<color=orange><b>{(serverIndex == CurrentIndex ? ">" : string.Empty)}</b></color>  {serv.Name} <color=orange>{serv.PlayersCount}</color>/<color=orange>{serv.Settings.MaxPlayers}</color> <mark=#00cf00>[</mark>");
            }

            startingLine++;
            builder.SetRightLine(startingLine, $"<mark=#00000082><size=29>     Total players connected <color=orange>{Player.Count}</color>   </size></mark>‎‎");
            startingLine += 2;
            builder.SetRightLine(startingLine, $"<size=27><mark=#00000082>Switch server by pressing <color=green>Noclip Keybind</color></mark></size>");

            if (InfoMessageDuration > 0)
                builder.SetCenterLine(15, InfoMessage);

            builder.SetCenterLine(23, $"You will be connecting to server <color=orange>{Servers[CurrentIndex]}</color>");
            builder.SetCenterLine(24, $"<size=26>Press <color=green>Voicechat Keybind</color> to connect</size>");

            Player.SendHint(builder.ToString(), dur);
        }

        public override void OnReceiveGameConsoleCommand(string command, string[] args)
        {
            switch (command.ToLower())
            {
                case "gsh":
                    Player.SendGameConsoleMessage("Syntax: .connect IP:PORT", "green");
                    break;
                case "servers":
                    string text = $"Servers: " + string.Join(", ", Server.List.Select(x => $"<color=orange>{x.Name}</color> (<color=white>{x.PlayersCount}/{x.Settings.MaxPlayers}</color>)"));
                    Player.SendGameConsoleMessage(text, "green");
                    break;
                case "connect":
                    if (args.Length == 0 || string.IsNullOrEmpty(args[0]))
                    {
                        Player.SendGameConsoleMessage("Syntax: connect <serverName>", "red");
                        return;
                    }

                    Server serv;
                    if (args[0].Contains(":"))
                    {
                        if (!Server.TryGetByPublicIp(args[0], out serv))
                        {
                            Player.SendGameConsoleMessage($"Server with ip <color=white>{args[0]}</color> not found!", "red");
                            return;
                        }
                    }
                    else
                    {
                        if (!Server.TryGetByName(args[0], out serv))
                        {
                            Player.SendGameConsoleMessage($"Server with name <color=white>{args[0]}</color> not found!", "red");
                            return;
                        }
                    }

                    Player.ConnectTo(serv);
                    Player.SendGameConsoleMessage($"Connecting to <color=white>{args[0]}</color>...", "green");
                    break;
            }
        }

        public override void OnReceiveMirrorDataFromProxy(uint key, NetworkReader reader)
        {
            if (key == 3034 || key == 53182)
                return;
                
            switch (key)
            {
                case NoclipToggleId:
                    CurrentIndex = Servers.Count <= CurrentIndex + 1 ? 0 : CurrentIndex + 1;
                    break;
                case VoiceMessageId:
                    Connect = true;
                    break;
            }
        }
    }
}
