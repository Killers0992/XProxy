using System.Linq;
using XProxy.Enums;

namespace XProxy.Core.Connections
{
    public class LostConnection : SimulatedConnection
    {
        bool _searching;
        bool _disconnecting;
        int _timer = 5;
        bool _foundNextServer;

        public LostConnectionType LostConnectionType { get; private set; }
        public int LostConnectionTime { get; private set; }

        public LostConnection(Player plr, LostConnectionType type) : base(plr)
        {
            LostConnectionType = type;
        }

        public override void Update()
        {
            if (LostConnectionTime != 10)
            {
                LostConnectionTime++;
                Player.SendHint(Player.Proxy._config.Messages.LostConnectionHint.Replace("%time%", $"{LostConnectionTime}"), 1);
                return;
            }


            if (_timer != 0 && !_searching)
            {
                Player.SendHint(Player.Proxy._config.Messages.SearchingForFallbackServerHint, 1);
                _timer--;
                return;
            }

            _searching = true;
            if (_disconnecting)
            {
                if (_timer == 0)
                {
                    if (_foundNextServer)
                        Player.Roundrestart(1);
                    else
                        Player.DisconnectFromProxy();
                }
                else
                {
                    _timer--;
                }
            }
            else
            {
                string targetServer = Player.Proxy.GetRandomServerFromPriorities().ServerName;

                if (string.IsNullOrEmpty(targetServer))
                {
                    Player.SendHint(Player.Proxy._config.Messages.OnlineServerNotFoundHint, 4);
                }
                else
                {
                    _foundNextServer = true;
                    Player.SaveServerForNextSession(targetServer, 7f);
                    Player.SendHint(Player.Proxy._config.Messages.ConnectingToServerHint.Replace("%server%", targetServer), 4);
                }

                _disconnecting = true;
                _timer = 2;
            }
        }
    }
}
