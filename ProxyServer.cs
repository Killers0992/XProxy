using LiteNetLib;
using LiteNetLib.Utils;
using Mirror;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XProxy.Enums;
using XProxy.Models;
using XProxy.ServerList;
using static XProxy.Program;

namespace XProxy
{
    public class ProxyServer : INetEventListener
    {
        private ProxyConfig Config;
        private int targetID;
        public ServerConsole serverlist;

        public ProxyServer(ProxyConfig config, int Port, int targetServerID)
        {
            serverlist = new ServerConsole(Port);
            this.targetID = targetServerID;
            this.Config = config;
            manager = new NetManager(this);
            manager.IPv6Enabled = IPv6Mode.SeparateSocket;
            manager.UpdateTime = 15;
            manager.PingInterval = 1000;
            manager.DisconnectTimeout = 5000;
            manager.ReconnectDelay = 500;
            manager.MaxConnectAttempts = 10;
            manager.BroadcastReceiveEnabled = true;
            manager.ChannelsCount = 5;

            pollingTask = Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    await Task.Delay(15);
                    if (manager != null && IsPooling)
                        if (manager.IsRunning)
                            manager.PollEvents();
                }
            });
            manager.Start(Port);
            Console.WriteLine($"Proxy started on port {Port}.");
            IsPooling = true;
        }

        public void RedirectAllClients(string ip, int port)
        {
            Console.WriteLine($"Redirecting all clients ({manager.ConnectedPeersCount}) to server {ip}:{port}.");
            foreach (var client in clients)
            {
                client.Value.Redirect(ip, port);
            }
        }


        private Task pollingTask;
        private NetManager manager;

        private bool IsPooling { get; set; } = false;

        public ConcurrentDictionary<NetPeer, ProxyClient> clients = new ConcurrentDictionary<NetPeer, ProxyClient>();

        public void OnConnectionRequest(ConnectionRequest request)
        {
            ProxyClient prox = new ProxyClient(this, request, PreAuthModel.ReadPreAuth(request.RemoteEndPoint.Address.ToString(), request.Data));
            if (prox.PreAuthData == null)
            {
                request.Reject();
                return;
            }

            if (Config.servers.TryGetValue(targetID, out ProxyServerData proxServer))
            {
                prox.ConnectTo(proxServer.Address, proxServer.Port);
            }
            else
            {
                NetDataWriter writer = new NetDataWriter();
                writer.Put((byte)RejectionReason.NotSpecified);
                writer.Put("SERVER NOT FOUND");
                request.Reject(writer);
            }
        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            if (clients.TryGetValue(peer, out ProxyClient prox))
            {
                prox.ReceiveData(reader, deliveryMethod);
            }
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
        }

        public void OnPeerConnected(NetPeer peer)
        {
            serverlist.PlayersOnline++;
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            if (clients.TryRemove(peer, out ProxyClient prox))
                prox.DisconnectFromProxy();
            serverlist.PlayersOnline--;
        }
    }
}
