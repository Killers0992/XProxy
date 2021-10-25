using LiteNetLib;
using LiteNetLib.Utils;
using Mirror;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XProxy.Enums;
using XProxy.Models;

namespace XProxy
{
    public class ProxyClient : INetEventListener
    {
        private ProxyServer server;
        public ProxyClient(ProxyServer server, ConnectionRequest connectionRequest, PreAuthModel preAuth)
        {
            this.server = server;
            PreAuthData = preAuth;
            ConnectionRequest = connectionRequest;
            Manager = new NetManager(this);
            Manager.IPv6Enabled = IPv6Mode.SeparateSocket;
            Manager.UpdateTime = 15;
            Manager.PingInterval = 1000;
            Manager.DisconnectTimeout = 5000;
            Manager.ReconnectDelay = 500;
            Manager.MaxConnectAttempts = 10;
            Manager.BroadcastReceiveEnabled = false;
            Manager.ChannelsCount = 5;
            Manager.Start();

            Task.Factory.StartNew(async () =>
            {
                while (!end)
                {
                    try
                    {
                        if (Manager != null && IsPooling)
                            if (Manager.IsRunning)
                                Manager.PollEvents();
                    }catch(Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    await Task.Delay(15);
                }
            });
        }

        public NetPeer ConnectTo(string address, int port)
        {
            this.TargetAddress = address;
            this.TargetPort = port;
            Console.WriteLine($"Connecting client {this.ClientEndPoint} ({this.PreAuthData.UserID}) => {address}:{port}");
            NetPeer conPeer = null;
            if (Manager != null)
                conPeer = Manager.Connect(address, port, PreAuthData.RawPreAuth);
            Console.WriteLine(PreAuthData.ToString());
            IsPooling = true;
            return conPeer;
        }

        public void Redirect(string address, int port)
        {
            Console.WriteLine($"Redirecting client {this.ClientEndPoint} ({this.PreAuthData.UserID}) from {TargetAddress}:{TargetPort} => {address}:{port}");
            this.TargetAddress = address;
            this.TargetPort = port;
        }

        public void DisconnectFromProxy()
        {
            Console.WriteLine($"Client {this.ClientEndPoint} ({this.PreAuthData.UserID}) disconnected from proxy, killing task.");
            end = false;
        }


        //Receive data from server to proxy client.
        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            //Send data to client.
            switch (deliveryMethod)
            {
                /*case DeliveryMethod.ReliableOrdered:
                case DeliveryMethod.ReliableUnordered:
                    ServerPeer.SendWithDeliveryEvent(reader.RawData, reader.RawData[3], deliveryMethod, null);
                    break;      -*/
                default:
                    var bytes = reader.GetRemainingBytesSegment();
                    ServerPeer.Send(bytes.Array, bytes.Offset, bytes.Count, deliveryMethod);
                    //ServerPeer.Send(reader.RawData, reader.UserDataOffset, reader.UserDataSize, deliveryMethod);
                    break;
            }
            reader.Recycle();
        }

        //Receive data from client to proxy server.
        public void ReceiveData(NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            if (Manager.FirstPeer == null)
                return;

            //Send data to server.
            switch (deliveryMethod)
            {
                /*case DeliveryMethod.ReliableOrdered:
                case DeliveryMethod.ReliableUnordered:
                    Manager.FirstPeer.SendWithDeliveryEvent(reader.RawData, reader.RawData[3], deliveryMethod, null);
                    break;     */
                default:
                    var bytes = reader.GetRemainingBytesSegment();
                    Manager.FirstPeer.Send(bytes.Array, bytes.Offset, bytes.Count, deliveryMethod);
                    break;
            }
            reader.Recycle();
        }

        private readonly NetManager Manager;
        public NetPeer ServerPeer;
        public ConnectionRequest ConnectionRequest;
        private IPEndPoint ClientEndPoint => ConnectionRequest != null ? ConnectionRequest.RemoteEndPoint : ServerPeer.EndPoint;
        public PreAuthModel PreAuthData { get; set; }

        private bool end = false;

        public bool IsConnected { get; set; } = false;
        public bool IsPooling { get; set; } = false;
        public bool IsRedirecting { get; set; } = false;

        public string TargetAddress { get; set; } = "localhost";
        public int TargetPort { get; set; } = 7777;

        public void OnConnectionRequest(ConnectionRequest request)
        {
        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
        }

        public void OnPeerConnected(NetPeer peer)
        {
            ServerPeer = ConnectionRequest.Accept();
            ConnectionRequest = null;
            server.clients.TryAdd(ServerPeer, this);
            Console.WriteLine($"Client connected {this.ClientEndPoint} ({this.PreAuthData.UserID}) => {TargetAddress}:{TargetPort}");
            IsConnected = true;
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            if (disconnectInfo.AdditionalData.RawData == null)
            {
                NetDataWriter writer = new NetDataWriter();
                writer.Put((byte)RejectionReason.Custom);
                writer.Put("Server is offline.");
                ConnectionRequest.Reject(writer);
                goto skipChecking;
            }

            NetDataWriter writer2 = NetDataWriter.FromBytes(disconnectInfo.AdditionalData.RawData, disconnectInfo.AdditionalData.UserDataOffset, disconnectInfo.AdditionalData.UserDataSize);
            if (disconnectInfo.AdditionalData.TryGetByte(out byte lastRejectionReason))
            {
                RejectionReason reason = (RejectionReason)lastRejectionReason;
                Console.WriteLine($"Client {this.ClientEndPoint} ({this.PreAuthData.UserID}) disconnected from target server {TargetAddress}:{TargetPort} with reason {reason}");
                if (ConnectionRequest != null)
                {
                    Console.WriteLine($"Reject peer");
                    ConnectionRequest.Reject(writer2);
                }
                else
                {
                    Console.WriteLine($"Disconnect peer");
                    ServerPeer.Disconnect();
                }
            }
            else
            {
                ServerPeer.Disconnect();
            }
            skipChecking:
            if (ServerPeer == null && !IsRedirecting)
                DisconnectFromProxy();
            IsConnected = false;
            IsPooling = false;
        }
    }
}
