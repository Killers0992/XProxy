using XProxy.Core.Events;

namespace XProxy.Core.Core.Events.Args
{
    public class ProxyStartedListening : BaseEvent
    {
        public ProxyStartedListening(Listener server, int port) 
        { 
            Server = server;
            Port = port;
        }

        public Listener Server { get; private set; }
        public int Port { get; private set; }
    }
}
