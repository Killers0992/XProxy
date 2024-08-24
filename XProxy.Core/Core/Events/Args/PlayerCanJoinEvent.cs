using XProxy.Core.Events;
using XProxy.Models;

namespace XProxy.Core.Core.Events.Args
{
    public class PlayerCanJoinEvent : BaseEvent
    {
        public PlayerCanJoinEvent(Player player, ServerInfo server)
        {
            Player = player;
            Server = server;
        }

        public Player Player { get; }
        public ServerInfo Server { get; }

        public bool ForceAllow { get; set; }
        public bool ForceDeny { get; set; }
    }
}
