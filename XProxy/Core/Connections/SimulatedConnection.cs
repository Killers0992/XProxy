namespace XProxy.Core.Connections
{
    public class SimulatedConnection : BaseConnection
    {
        public SimulatedConnection(Player plr) : base(plr)
        {
            plr.InternalDestroyNetwork();
            plr.InternalAcceptConnection(this);
            plr.ProcessMirrorMessagesFromProxy = true;
        }
    }
}
