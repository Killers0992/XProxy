using LiteNetLib;
using System;

namespace XProxy.Core.Events.Args
{
    public class ProxyConnectionRequest : BaseCancellableEvent
    {
        private ConnectionRequest _request;

        public ProxyConnectionRequest(ProxyServer proxy, ConnectionRequest request, string ipAddress, string userId, CentralAuthPreauthFlags flags)
        {
            Proxy = proxy;

            _request = request;

            IpAddress = ipAddress;
            UserId = userId;
            Flags = flags;
        }

        public ProxyServer Proxy { get; }
        public string IpAddress { get; }
        public string UserId { get; }
        public CentralAuthPreauthFlags Flags { get; }

        public void Disconnect() => _request.RejectForce();
        public void Disconnect(string reason) => _request.Disconnect(reason);
        public void DisconnectBanned(string reason, long expiration) => _request.DisconnectBanned(reason, expiration);
        public void DisconnectBanned(string reason, DateTime date) => _request.DisconnectBanned(reason, date);
    }
}
