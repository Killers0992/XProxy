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
        public XProxy.ServerList.ServerConsole serverlist;

        public ProxyServer(ProxyConfig config, int Port, int targetServerID)
        {
            serverlist = new XProxy.ServerList.ServerConsole(Port);
            this.targetID = targetServerID;
            this.Config = config;
            Manager = new NetManager(this);
            Manager.IPv6Enabled = IPv6Mode.SeparateSocket;
            //Manager.UpdateTime = 5;
            Manager.AutoRecycle = false;
            Manager.BroadcastReceiveEnabled = false;
            Manager.ChannelsCount = (byte)6;
            Manager.DisconnectTimeout = 5000;
            Manager.ReconnectDelay = 1200;
            Manager.UnsyncedDeliveryEvent = false;
            Manager.EnableStatistics = false;
            Manager.MaxConnectAttempts = 22;
            Manager.MtuOverride = 0;
            Manager.NatPunchEnabled = false;
            Manager.PingInterval = 1000;
            Manager.ReuseAddress = false;
            Manager.UnconnectedMessagesEnabled = false;
            Manager.UnsyncedEvents = false;
            Manager.UnsyncedReceiveEvent = false;
            Manager.UseSafeMtu = false;

            pollingTask = Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    await Task.Delay(15);
                    if (Manager != null && IsPooling)
                        if (Manager.IsRunning)
                            Manager.PollEvents();
                }
            });
            Manager.Start(Port);
            Console.WriteLine($"Proxy started on port {Port}.");
            IsPooling = true;
        }


        public void RedirectAllClients(string ip, int port)
        {
            Console.WriteLine($"Redirecting all clients ({Manager.ConnectedPeersCount}) to server {ip}:{port}.");
            foreach (var client in clients)
            {
                client.Value.Redirect(ip, port);
            }
        }


        private Task pollingTask;
        private NetManager Manager;

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
