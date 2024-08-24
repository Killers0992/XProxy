using XProxy.Core.Core.Events.Args;
using XProxy.Core.Events.Args;
using static XProxy.Core.Events.EventManager;

namespace XProxy.Core.Events
{
    public class EventManager
    {
        public delegate void CustomEventHandler<in TEvent>(TEvent ev)
            where TEvent : BaseEvent;

        public static ProxyEvents Proxy { get; } = new ProxyEvents();

        public static PlayerEvents Player { get; } = new PlayerEvents();
    }

    public class ProxyEvents
    {
        public event CustomEventHandler<ProxyConnectionRequest> ConnectionRequest;
        public void InvokeConnectionRequest(ProxyConnectionRequest ev) => ConnectionRequest?.InvokeWithExceptionHandler(ev);

        public event CustomEventHandler<ProxyStartedListening> StartedListening;
        public void InvokeStartedListening(ProxyStartedListening ev) => StartedListening?.InvokeWithExceptionHandler(ev);
    }

    public class PlayerEvents
    {
        public event CustomEventHandler<PlayerAssignTargetServer> AssignTargetServer;
        public void InvokeAssignTargetServer(PlayerAssignTargetServer ev) => AssignTargetServer?.InvokeWithExceptionHandler(ev);

        public event CustomEventHandler<PlayerCanJoinEvent> CanJoin;
        public void InvokeCanJoin(PlayerCanJoinEvent ev) => CanJoin?.InvokeWithExceptionHandler(ev);
    }
}
