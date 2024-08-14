using XProxy.Core.Events;

namespace XProxy.Core.Core.Events.Args
{
    public class ProxyStartedListening : BaseEvent
    {
        public ProxyStartedListening(ProxyServer server, int port) 
        { 
            Server = server;
            Port = port;
        }

        public ProxyServer Server { get; private set; }
        public int Port { get; private set; }
    }
}
