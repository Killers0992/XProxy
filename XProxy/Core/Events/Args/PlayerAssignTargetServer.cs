using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XProxy.Core.Events;
using XProxy.Models;

namespace XProxy.Core.Events.Args
{
    public class PlayerAssignTargetServer : BaseEvent
    {
        public PlayerAssignTargetServer(Player player, ServerInfo server)
        {
            Player = player;
            Server = server;
        }

        public Player Player { get; }
        public ServerInfo Server { get; set; }
    }
}
