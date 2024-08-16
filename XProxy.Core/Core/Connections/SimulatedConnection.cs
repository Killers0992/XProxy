namespace XProxy.Core.Connections
{
    public class SimulatedConnection : BaseConnection
    {
        public virtual bool AddToOnlinePlayers { get; } = true;

        public SimulatedConnection(Player plr) : base(plr)
        {
            plr.InternalDestroyNetwork();
            plr.InternalAcceptConnection(this, AddToOnlinePlayers);
            plr.ProcessMirrorMessagesFromProxy = true;
        }
    }
}
