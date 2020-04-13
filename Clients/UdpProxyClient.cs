using System;
using System.Net;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace NetProxy.Clients
{
    internal class UdpProxyClient
    {

        public readonly UdpClient client = new UdpClient();
        public DateTime lastActivity = DateTime.UtcNow;

        private readonly IPEndPoint _clientEndpoint;
        private readonly IPEndPoint _remoteServer;
        private bool _isRunning;
        private readonly TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();
        private readonly UdpClient _server;

        /// <summary>
        /// UDP Proxy Client
        /// </summary>
        /// <param name="server"></param>
        /// <param name="clientEndpoint"></param>
        /// <param name="remoteServer"></param>
        public UdpProxyClient(UdpClient server, IPEndPoint clientEndpoint, IPEndPoint remoteServer)
        {
            _server = server;

            _isRunning = true;
            _remoteServer = remoteServer;
            _clientEndpoint = clientEndpoint;
            Console.WriteLine($"Established {clientEndpoint} => {remoteServer}");
            Run();
        }

        /// <summary>
        /// Send UDP message to configured server
        /// </summary>
        /// <param name="message">Array of bytes which to send to configured server</param>
        public async Task SendToServer(byte[] message)
        {
            lastActivity = DateTime.UtcNow;

            await _tcs.Task;
            var sent = await client.SendAsync(message, message.Length, _remoteServer);
            Console.WriteLine($"{sent} bytes sent from a client message of {message.Length} bytes from {_clientEndpoint} to {_remoteServer}");
        }

        /// <summary>
        /// Run UDP Proxy Client
        /// </summary>
        private void Run()
        {

            Task.Run(async () =>
            {
                client.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
                _tcs.SetResult(true);
                using (client)
                {
                    while (_isRunning)
                    {
                        try
                        {
                            var result = await client.ReceiveAsync();
                            lastActivity = DateTime.UtcNow;
                            var sent = await _server.SendAsync(result.Buffer, result.Buffer.Length, _clientEndpoint);
                            Console.WriteLine($"{sent} bytes sent from a return message of {result.Buffer.Length} bytes from {_remoteServer} to {_clientEndpoint}");

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"An exception occurred while recieving a server datagram : {ex}");
                        }
                    }
                }

            });
        }

        public void Stop()
        {
            Console.WriteLine($"Closed {_clientEndpoint} => {_remoteServer}");
            _isRunning = false;
        }
    }
}
