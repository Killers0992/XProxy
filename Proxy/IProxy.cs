using System.Threading.Tasks;

namespace XProxy.Proxy
{
    interface IProxy
    {
        Task Start(string remoteServerIp, ushort remoteServerPort, ushort localPort, string localIp = null);
    }
}
