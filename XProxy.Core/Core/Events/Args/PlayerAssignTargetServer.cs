namespace XProxy.Core.Events.Args
{
    public class PlayerAssignTargetServer : BaseEvent
    {
        public PlayerAssignTargetServer(Player player, Server server)
        {
            Player = player;
            Server = server;
        }

        public Player Player { get; }
        public Server Server { get; set; }
    }
}
