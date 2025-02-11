using LiteNetLib;
using Mirror;
using System;
using XProxy.Core.Core.Connections.Responses;

namespace XProxy.Core.Connections
{
    public class BaseConnection : IDisposable
    {
        public Player Player { get; private set; }

        public BaseConnection(Player plr)
        {
            Player = plr;
        }

        internal void InternalConnected()
        {
            try
            {
                OnConnected();
            }
            catch(Exception ex)
            {
                Logger.Error(ex, "Connection");
            }
        }

        public virtual void OnReceiveGameConsoleCommand(string command, string[] args)
        {

        }

        public virtual void OnReceiveDataFromProxy(NetPacketReader reader, DeliveryMethod method)
        {

        }

        public virtual void OnReceiveMirrorDataFromProxy(uint key, NetworkReader reader)
        {

        }

        public virtual void OnConnectionResponse(Server server, BaseResponse response)
        {

        }

        public virtual void OnConnected()
        {

        }

        public virtual void OnClientReady()
        {

        }

        public virtual void OnAddPlayer()
        {

        }

        public virtual void OnPlayerSpawned()
        {

        }

        public virtual void Update()
        {

        }

        public virtual void Dispose()
        {
        }
    }
}
