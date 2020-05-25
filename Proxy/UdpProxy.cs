using Org.BouncyCastle.Asn1.Mozilla;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using XProxy.ServerList;

namespace XProxy.Proxy
{
    class UdpProxy : IProxy
    {
        static readonly TimeSpan time = TimeSpan.FromSeconds(3);

        public async Task Start(ushort port)
        {
            var clients = new ConcurrentDictionary<IPEndPoint, UdpClient>();
            var client_server = new ConcurrentDictionary<string, ushort>();
            var server = new System.Net.Sockets.UdpClient(AddressFamily.InterNetworkV6);
            server.Client.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
            IPAddress localIpAddress = IPAddress.IPv6Any;
            server.Client.Bind(new IPEndPoint(localIpAddress, port));
            Console.WriteLine($"proxy started UDP:{localIpAddress}|{port}");
            var _ = Task.Run(async () =>
            {

                while (true)
                {
                    await Task.Delay(3000);
                    DateTime timeNow = DateTime.UtcNow;
                    foreach (var client in clients.ToArray())
                    {
                        if (client.Value.lastActivity + time < timeNow)
                        {
                            UdpClient c;
                            clients.TryRemove(client.Key, out c);
                            client.Value.Stop();
                            var _2 = Task.Run(async () =>
                            {
                                await Task.Delay(3000);
                                if (client.Value.lastActivity + time <  timeNow)
                                {
                                    client_server.TryRemove(client.Key.Address.ToString(), out ushort port);
                                }
                            });
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
                    UdpClient client = null;
                    if (clients.ContainsKey(endpoint))
                    {
                        client = clients[endpoint];
                    }
                    else
                    {
                        ushort last_port = Program.main_server.port;
                        if (client_server.ContainsKey(endpoint.Address.ToString()))
                            last_port = client_server[endpoint.Address.ToString()];
                        else
                            client_server.TryAdd(endpoint.Address.ToString(), last_port);
                        client = new UdpClient(server, endpoint, new IPEndPoint(IPAddress.Parse("127.0.0.1"), last_port));
                        clients.TryAdd(endpoint, client);
                    }
                    await client.SendToServer(message.Buffer);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"an exception occurred on recieving a client datagram: {ex}");
                }

            }
        }
    }

    class UdpClient
    {
        private readonly System.Net.Sockets.UdpClient _server;
        public UdpClient(System.Net.Sockets.UdpClient server, IPEndPoint clientEndpoint, IPEndPoint remoteServer)
        {
            _server = server;

            _isRunning = true;
            _remoteServer = remoteServer;
            _clientEndpoint = clientEndpoint;
            ServerConsole.PlayersOnline++;
            Console.WriteLine($"Established {clientEndpoint} => {remoteServer}");
            Run();
        }

        public Server client_on_server;
        public readonly System.Net.Sockets.UdpClient client = new System.Net.Sockets.UdpClient();
        public DateTime lastActivity = DateTime.UtcNow;
        private readonly IPEndPoint _clientEndpoint;
        private readonly IPEndPoint _remoteServer;
        private bool _isRunning;
        private readonly TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();



        public async Task SendToServer(byte[] message)
        {
            lastActivity = DateTime.UtcNow;

            await _tcs.Task;
            var sent = await client.SendAsync(message, message.Length, _remoteServer);
            //Console.WriteLine($"{sent} bytes sent from a client message of {message.Length} bytes from {_clientEndpoint} to {_remoteServer}");
        }

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
                            //Console.WriteLine($"{sent} bytes sent from a return message of {result.Buffer.Length} bytes from {_remoteServer} to {_clientEndpoint}");

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
            ServerConsole.PlayersOnline--;
            _isRunning = false;
        }
    }
}
