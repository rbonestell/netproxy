using System;
using System.Net;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace NetProxy.Clients
{
    internal class TcpProxyClient
    {

        public TcpClient client = new TcpClient();

        private TcpClient _remoteClient;
        private readonly IPEndPoint _clientEndpoint;
        private readonly IPEndPoint _remoteServer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="remoteClient"></param>
        /// <param name="remoteServer"></param>
        public TcpProxyClient(TcpClient remoteClient, IPEndPoint remoteServer)
        {
            _remoteClient = remoteClient;


            _remoteServer = remoteServer;
            client.NoDelay = true;
            _clientEndpoint = (IPEndPoint)_remoteClient.Client.RemoteEndPoint;
            Console.WriteLine($"Established {_clientEndpoint} => {remoteServer}");
            Run();
        }        

        /// <summary>
        /// 
        /// </summary>
        private void Run()
        {

            Task.Run(async () =>
            {
                try
                {
                    using (_remoteClient)
                    using (client)
                    {
                        await client.ConnectAsync(_remoteServer.Address, _remoteServer.Port);
                        var serverStream = client.GetStream();
                        var remoteStream = _remoteClient.GetStream();

                        await Task.WhenAny(remoteStream.CopyToAsync(serverStream), serverStream.CopyToAsync(remoteStream));



                    }
                }
                catch (Exception) { }
                finally
                {
                    Console.WriteLine($"Closed {_clientEndpoint} => {_remoteServer}");
                    _remoteClient = null;
                }
            });
        }


    }
}
