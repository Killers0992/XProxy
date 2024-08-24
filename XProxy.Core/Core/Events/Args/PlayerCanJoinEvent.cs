using XProxy.Core.Events;
using XProxy.Models;

namespace XProxy.Core.Core.Events.Args
{
    public class PlayerCanJoinEvent : BaseEvent
    {
        public PlayerCanJoinEvent(Player player, Server server)
        {
            Player = player;
            Server = server;
        }

        public Player Player { get; }
        public Server Server { get; }

        public bool ForceAllow { get; set; }
        public bool ForceDeny { get; set; }
    }
}
