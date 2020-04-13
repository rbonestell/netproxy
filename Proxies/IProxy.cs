using System.Threading.Tasks;

namespace NetProxy.Proxies
{
    internal interface IProxy
    {
        Task Start(string remoteServerIp, ushort remoteServerPort, ushort localPort, string localIp = null);
    }
}
