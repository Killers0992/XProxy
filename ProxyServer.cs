using LiteNetLib;
using LiteNetLib.Utils;
using Mirror;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
        public XProxy.ServerList.ServerConsole serverlist;

        public int ServerPort { get; set; }

        public string TargetIP { get; set; }
        public int TargetPort { get; set; }

        public ProxyServer(int serverPort, string targetIp, int targetPort)
        {
            this.ServerPort = serverPort;
            this.TargetIP = targetIp;
            this.TargetPort = targetPort;
            serverlist = new XProxy.ServerList.ServerConsole(serverPort);
            Manager = new NetManager(this);
            Manager.IPv6Enabled = IPv6Mode.SeparateSocket;
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
            Manager.Start(serverPort);
            Console.WriteLine($"Proxy started on port {serverPort} redirecting to {TargetIP}:{targetPort}.");
            clients.Add(serverPort, new ConcurrentDictionary<byte, ProxyClient>());
        }

        public async Task Run()
        {
            while (true)
            {
                await Task.Delay(16);
                if (Manager != null)
                    if (Manager.IsRunning)
                        Manager.PollEvents();
            }
        }

        private NetManager Manager;

        public static Dictionary<int, ConcurrentDictionary<byte, ProxyClient>> clients = new Dictionary<int, ConcurrentDictionary<byte, ProxyClient>>();

        public Dictionary<byte, CancellationTokenSource> Tokens = new Dictionary<byte, CancellationTokenSource>();

        public void OnConnectionRequest(ConnectionRequest request)
        {
            string failed = string.Empty;
            var preAuth = PreAuthModel.ReadPreAuth(request.RemoteEndPoint.Address.ToString(), request.Data, ref failed);

            if (!preAuth.IsValid)
            {
                Console.WriteLine($"Preauth is invalid for connection " + request.RemoteEndPoint.Address.ToString() + $" {failed} ");
                request.Reject();
                return;
            }

            ProxyClient prox = new ProxyClient(this, request, preAuth);

            prox.ConnectTo(TargetIP, TargetPort);
        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            if (clients[ServerPort].TryGetValue(peer.ConnectionNum, out ProxyClient prox))
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
            clients[ServerPort].TryRemove(peer.ConnectionNum, out _);
            
            if (Tokens.TryGetValue(peer.ConnectionNum, out CancellationTokenSource token))
            {
                token.Cancel();
                Tokens.Remove(peer.ConnectionNum);
            }
            serverlist.PlayersOnline--;
        }
    }
}
