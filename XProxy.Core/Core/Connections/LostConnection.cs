using XProxy.Enums;
using XProxy.Services;

namespace XProxy.Core.Connections
{
    public class LostConnection : SimulatedConnection
    {
        bool _searching;
        bool _disconnecting;
        int _timer = 1;
        bool _foundNextServer;

        public LostConnectionType LostConnectionType { get; private set; }
        public int LostConnectionTime { get; private set; }

        public LostConnection(Player plr, LostConnectionType type) : base(plr)
        {
            LostConnectionType = type;
        }

        public override void Update()
        {
            if (LostConnectionTime != 2)
            {
                LostConnectionTime++;
                Player.SendHint(ConfigService.Singleton.Messages.LostConnectionHint.Replace("%time%", $"{LostConnectionTime}"), 1);
                return;
            }

            if (_timer != 0 && !_searching)
            {
                Player.SendHint(ConfigService.Singleton.Messages.SearchingForFallbackServerHint, 1);
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
                Server fallback = Player.CurrentServer.GetFallbackServer(Player);

                if (fallback == null)
                {
                    Player.SendHint(ConfigService.Singleton.Messages.OnlineServerNotFoundHint, 4);
                }
                else
                {
                    _foundNextServer = true;
                    Player.SaveServerForNextSession(fallback.Name, 7f);
                    Player.SendHint(ConfigService.Singleton.Messages.ConnectingToServerHint.Replace("%server%", fallback.Name), 4);
                }

                _disconnecting = true;
                _timer = 2;
            }
        }
    }
}
