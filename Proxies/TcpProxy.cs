using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using NetProxy.Clients;

namespace NetProxy.Proxies
{
    internal class TcpProxy : IProxy
    {
        public async Task Start(string remoteServerIp, ushort remoteServerPort, ushort localPort,string localIp)
        {
            //var clients = new ConcurrentDictionary<IPEndPoint, TcpClient>();

            IPAddress localIpAddress = string.IsNullOrEmpty(localIp) ? IPAddress.IPv6Any : IPAddress.Parse(localIp);
            var server = new TcpListener(new IPEndPoint(localIpAddress, localPort));
            server.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
            server.Start();

            Console.WriteLine($"TCP proxy started {localPort} -> {remoteServerIp}|{remoteServerPort}");
            while (true)
            {

                try
                {
                    var remoteClient = await server.AcceptTcpClientAsync();
                    remoteClient.NoDelay = true;
                    var ips = await Dns.GetHostAddressesAsync(remoteServerIp);

                    new TcpProxyClient(remoteClient, new IPEndPoint(ips.First(), remoteServerPort));


                }
                catch (Exception ex) {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex);
                    Console.ResetColor();
                }

            }
        }
    }

    
}
