using Mirror;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using XProxy.Models;
using XProxy.Services;

namespace XProxy.Core.Connections
{
    public class LobbyConnection : SimulatedConnection
    {
        public const ushort NoclipToggleId = 48298;
        public const ushort VoiceMessageId = 41876;

        private const string _spacing = "           ";

        private string SelectedServer = null;

        public int CurrentIndex = 0;
        private bool connect;

        private string[] Servers = new string[0];

        public LobbyConnection(Player plr) : base(plr)
        {
            Servers = ProxyService.Singleton.Servers.Keys.Where(x => x != plr.ServerInfo.ServerName).ToArray();
            SelectedServer = Servers[0];
        }

        public override void OnConnected()
        {
            Logger.Info(Player.Proxy._config.Messages.LobbyConnectedMessage.Replace("%tag%", Player.Tag).Replace("%address%", $"{Player.ClientEndPoint}").Replace("%userid%", Player.UserId), $"Player");
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
            if (string.IsNullOrEmpty(SelectedServer)) return;

            if (connect)
            {
                if (checkIfCanJoin)
                {
                    ServerInfo server = Player.Proxy.GetServerByName(SelectedServer);

                    canJoin = server.CanPlayerJoin(Player);

                    if (!canJoin)
                    {
                        Player.ServerInfo = server;
                        Player.Connection = new QueueConnection(Player);
                        return;
                    }

                    checkIfCanJoin = false;
                }

                if (canJoin)
                {
                    Player.SendHint(Player.Proxy._config.Messages.LobbyConnectingToServerHint.Replace("%server%", SelectedServer), 1f);
                }

                if (_timer == 0)
                {
                    if (canJoin)
                        Player.RedirectTo(SelectedServer);
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
            StringBuilder sb = new StringBuilder();

            string line1 = string.Empty;
            string line2 = string.Empty;

            for (int x = 0; x < Servers.Length; x++)
            {
                bool last = x == Servers.Length - 1;
                ServerInfo server = Player.Proxy.GetServerByName(Servers[x]);

                string serverLine = Player.Proxy._config.Messages.LobbyServerLine1.Replace("%selectedColor%", $"{(Servers[x] == SelectedServer ? Player.Proxy._config.Messages.SelectedServerColor : Player.Proxy._config.Messages.DefaultServerColor)}").Replace("%server%", server.ServerDisplayname);
                int serverLineLength = Regex.Replace(serverLine, @"\<.*\>", "").Length;

                string serverLine2 = Player.Proxy._config.Messages.LobbyServerLine2.Replace("%selectedColor%", $"{(Servers[x] == SelectedServer ? Player.Proxy._config.Messages.SelectedServerColor : Player.Proxy._config.Messages.DefaultServerColor)}").Replace("%onlinePlayers%", $"{server.PlayersOnline}").Replace("%maxPlayers%", $"{server.MaxPlayers}");
                int serverLine2Length = Regex.Replace(serverLine2, @"\<.*\>", "").Length;

                int additionalSpacing = (serverLineLength - serverLine2Length) / 2;

                string spacing = Spaces(additionalSpacing);

                line1 += serverLine + (last ? string.Empty : _spacing);
                line2 += $"{spacing}{serverLine2}{spacing}{(last ? string.Empty : _spacing)}";
            }

            string currentServer = "";

            ServerInfo selected = Player.Proxy.GetServerByName(SelectedServer);
            if (selected != null)
                currentServer = selected.ServerDisplayname;

            foreach (var line in Player.Proxy._config.Messages.LobbyMainHint)
            {
                sb.AppendLine(line.Replace("%proxyOnlinePlayers%", $"{Player.Proxy.Players.Count}").Replace("%proxyMaxPlayers%", $"{Player.Proxy._config.Value.MaxPlayers}").Replace("%serversLine1%", line1).Replace("%serversLine2%", line2).Replace("%server%", currentServer));
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
                    string text = $"Servers: " + string.Join(", ", Player.Proxy.Servers.Select(x => $"<color=orange>{x.Key}</color> (<color=white>{x.Value.PlayersOnline}/{x.Value.MaxPlayers}</color>)"));
                    Player.SendGameConsoleMessage(text, "green");
                    break;
                case "connect":
                    if (args.Length == 0 || string.IsNullOrEmpty(args[0]))
                    {
                        Player.SendGameConsoleMessage("Syntax: connect <serverName>", "red");
                        return;
                    }

                    ServerInfo serv;
                    if (args[0].Contains(":"))
                        serv = Player.Proxy.GetServerByPublicIp(args[0]);
                    else
                        serv = Player.Proxy.GetServerByName(args[0]);

                    if (serv == null)
                    {
                        Player.SendGameConsoleMessage($"Server with name <color=white>{args[0]}</color> not found!", "red");
                        return;
                    }

                    if (!serv.CanPlayerJoin(Player))
                    {
                        Player.ServerInfo = serv;
                        Player.Connection = new QueueConnection(Player);

                        Player.SendGameConsoleMessage($"Joined queue...", "red");
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
                case NoclipToggleId when Servers != null && Servers.Length > 0:
                    CurrentIndex = Servers.Length == CurrentIndex + 1 ? 0 : CurrentIndex + 1;
                    SelectedServer = Servers[CurrentIndex];
                    SendInfoHint(2f);
                    break;
                case VoiceMessageId:
                    connect = true;
                    break;
            }
        }
    }
}
