using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using NetProxy.Clients;

namespace NetProxy.Proxies
{
    internal class UdpProxy : IProxy
    {
        public async Task Start(string remoteServerIp, ushort remoteServerPort, ushort localPort, string localIp = null)
        {
            var clients = new ConcurrentDictionary<IPEndPoint, UdpProxyClient>();

            var server = new UdpClient(AddressFamily.InterNetworkV6);
            server.Client.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
            IPAddress localIpAddress = string.IsNullOrEmpty(localIp) ? IPAddress.IPv6Any : IPAddress.Parse(localIp);
            server.Client.Bind(new IPEndPoint(localIpAddress, localPort));
            Console.WriteLine($"proxy started UDP:{localIpAddress}|{localPort} -> {remoteServerIp}|{remoteServerPort}");

            _ = Task.Run(async () =>
              {

                  while (true)
                  {
                      await Task.Delay(10000);
                      foreach (var client in clients.ToArray())
                      {
                          if (client.Value.lastActivity + TimeSpan.FromSeconds(60) < DateTime.UtcNow)
                          {
                              clients.TryRemove(client.Key, out UdpProxyClient c);
                              client.Value.Stop();
                          }
                      }
                  }

              });
            while (true)
            {

                try
                {
                    var message = await server.ReceiveAsync();
                    var endpoint = message.RemoteEndPoint;
                    var client = clients.GetOrAdd(endpoint, ep => new UdpProxyClient(server, endpoint, new IPEndPoint(IPAddress.Parse(remoteServerIp), remoteServerPort)));
                    await client.SendToServer(message.Buffer);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"an exception occurred on recieving a client datagram: {ex}");
                }

            }
        }
    }

    
}
