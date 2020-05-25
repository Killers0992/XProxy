using System.Threading.Tasks;

namespace XProxy.Proxy
{
    interface IProxy
    {
        Task Start(ushort port);
    }
}
