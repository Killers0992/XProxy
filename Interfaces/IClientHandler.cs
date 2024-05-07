using LiteNetLib;
using XProxy.Models;

namespace XProxy.Interfaces
{
    public interface IClientHandler
    {
        void CreateConnection();
        void OnReceiveDataFromProxy(NetPacketReader reader, DeliveryMethod method);
    }
}
