using Mirror;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using XProxy.Services;

namespace XProxy.Core.Connections
{
    public class LobbyConnection : SimulatedConnection
    {
        public const ushort NoclipToggleId = 48298;
        public const ushort VoiceMessageId = 41876;

        private const string _spacing = "           ";

        private string SelectedServer = string.Empty;

        public int CurrentIndex = 0;
        private bool connect;

        public LobbyConnection(Player plr) : base(plr)
        {
            var names = Server.GetServerNames(Player);
            string firstServer = names.FirstOrDefault();

            if (!string.IsNullOrEmpty(firstServer))
            {
                if (Server.TryGetByName(firstServer, out Server srv))
                    SelectedServer = srv.Name;
            }
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

        int _timer = 3;
        bool checkIfCanJoin = true;

        bool canJoin = false;

        public override void Update()
        {
            if (connect)
            {
                if (string.IsNullOrEmpty(SelectedServer))
                {
                    connect = false;
                    return;
                }

                if (checkIfCanJoin)
                {
                    if (!Server.TryGetByName(SelectedServer, out Server srv))
                    {
                        checkIfCanJoin = false;
                        connect = false;
                        return;
                    }

                    canJoin = srv.CanPlayerJoin(Player);

                    if (!canJoin)
                    {
                        if (Player.CanJoinQueue(srv))
                        {
                            Player.JoinQueue(srv);
                        }
                        return;
                    }

                    checkIfCanJoin = false;
                }

                if (canJoin)
                {
                    Player.SendHint(ConfigService.Singleton.Messages.LobbyConnectingToServerHint.Replace("%server%", SelectedServer), 1f);
                }

                if (_timer <= 0)
                {
                    if (canJoin)
                    {
                        Player.RedirectTo(SelectedServer);
                        connect = false;
                        _timer = 0;
                    }
                    else
                    {
                        _timer = 3;
                        connect = false;
                    }
                }
                else
                    _timer--;
                return;
            }

            SendInfoHint();
        }

        string Spaces(int num)
        {
            string str = string.Empty;
            for(int x =0; x < num; x++)
            {
                str += " ";
            }

            return str;
        }

        public void SendInfoHint(float dur = 1f)
        {
            string currentServer;

            var names = Server.GetServerNames(Player);

            if (Server.TryGetByName(SelectedServer, out Server selectedServer))
            {
                currentServer = selectedServer.Settings.Name;
            }
            else
            {
                string firstServer = names.FirstOrDefault();

                if (!string.IsNullOrEmpty(firstServer))
                {
                    if (Server.TryGetByName(firstServer, out Server srv))
                    {
                        SelectedServer = srv.Name;
                        currentServer = SelectedServer;
                    }
                    else
                        currentServer = "none";
                }
                else
                    currentServer = "none";
            }

            StringBuilder sb = new StringBuilder();

            string line1 = string.Empty;
            string line2 = string.Empty;

            for (int x = 0; x < names.Count; x++)
            {
                if (!Server.TryGetByName(names[x], out Server srv))
                    continue;

                bool last = x == names.Count - 1;

                string serverLine = ConfigService.Singleton.Messages.LobbyServerLine1.Replace("%selectedColor%", $"{(names[x] == SelectedServer ? ConfigService.Singleton.Messages.SelectedServerColor : ConfigService.Singleton.Messages.DefaultServerColor)}").Replace("%server%", srv.Settings.Name);
                int serverLineLength = Regex.Replace(serverLine, @"\<.*\>", "").Length;

                string serverLine2 = ConfigService.Singleton.Messages.LobbyServerLine2.Replace("%selectedColor%", $"{(names[x] == SelectedServer ? ConfigService.Singleton.Messages.SelectedServerColor : ConfigService.Singleton.Messages.DefaultServerColor)}").Replace("%onlinePlayers%", $"{srv.PlayersCount}").Replace("%maxPlayers%", $"{srv.Settings.MaxPlayers}");
                int serverLine2Length = Regex.Replace(serverLine2, @"\<.*\>", "").Length;

                int additionalSpacing = (serverLineLength - serverLine2Length);

                string spacing = Spaces(additionalSpacing);

                line1 += serverLine + (last ? string.Empty : _spacing);
                line2 += $"{spacing}{serverLine2}{spacing}{(last ? string.Empty : _spacing)}";
            }

            foreach (var line in ConfigService.Singleton.Messages.LobbyMainHint)
            {
                sb.AppendLine(line.Replace("%serversLine1%", line1).Replace("%serversLine2%", line2).Replace("%server%", currentServer));
            }

            Player.SendHint(sb.ToString(), dur);
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

                    if (!serv.CanPlayerJoin(Player))
                    {
                        Player.SendGameConsoleMessage($"Server is full!", "red");
                        return;
                    }

                    Player.RedirectTo(serv);
                    Player.SendGameConsoleMessage($"Connecting to <color=white>{args[0]}</color>!", "green");
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
                    List<string> names = Server.GetServerNames(Player);

                    CurrentIndex = names.Count <= CurrentIndex + 1 ? 0 : CurrentIndex + 1;
                    SelectedServer = names[CurrentIndex];
                    SendInfoHint(2f);
                    break;
                case VoiceMessageId:
                    checkIfCanJoin = true;
                    connect = true;
                    break;
            }
        }
    }
}
