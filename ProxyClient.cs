using LiteNetLib;
using LiteNetLib.Utils;
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
        public ProxyClient(ConnectionRequest connectionRequest, PreAuthModel preAuth)
        {
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

            CancellationToken ct = token.Token;
            poolerTask = Task.Factory.StartNew(async () =>
            {
                ct.ThrowIfCancellationRequested();
                while (true)
                {
                    if (ct.IsCancellationRequested)
                        ct.ThrowIfCancellationRequested();
                    if (Manager != null && IsPooling)
                        if (Manager.IsRunning)
                            Manager.PollEvents();
                    await Task.Delay(15);
                }
            }, ct);
        }

        public void ConnectTo(string address, int port)
        {
            this.TargetAddress = address;
            this.TargetPort = port;
            Console.WriteLine($"Connecting client {this.ClientEndPoint} ({this.PreAuthData.UserID}) => {address}:{port}");
            if (Manager != null)
                Manager.Connect(address, port, PreAuthData.RawPreAuth);
            IsPooling = true;
        }

        public void DisconnectFromProxy()
        {
            Console.WriteLine($"Client {this.ClientEndPoint} ({this.PreAuthData.UserID}) disconnected from proxy, killing task.");
            token.Cancel();
        }

        public void ReceiveData(NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            Manager.FirstPeer.Send(reader.RawData, reader.UserDataOffset, reader.UserDataSize, reader.RawData[3], deliveryMethod);
            reader.Recycle();
        }

        private readonly NetManager Manager;
        public NetPeer ServerPeer;
        public ConnectionRequest ConnectionRequest;
        private IPEndPoint ClientEndPoint => ConnectionRequest != null ? ConnectionRequest.RemoteEndPoint : ServerPeer.EndPoint;
        public PreAuthModel PreAuthData { get; set; }

        private Task poolerTask;
        private CancellationTokenSource token = new CancellationTokenSource();

        public bool IsConnected { get; set; } = false;
        public bool IsPooling { get; set; } = false;

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

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            ServerPeer.Send(reader.RawData, reader.UserDataOffset, reader.UserDataSize, reader.RawData[3], deliveryMethod);
            reader.Recycle();
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
        }

        public void OnPeerConnected(NetPeer peer)
        {
            Console.WriteLine($"Client connected {this.ClientEndPoint} ({this.PreAuthData.UserID}) => {TargetAddress}:{TargetPort}");
            IsConnected = true;
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            if (disconnectInfo.AdditionalData.TryGetByte(out byte lastRejectionReason))
            {
                RejectionReason reason = (RejectionReason)lastRejectionReason; 
                Console.WriteLine($"Client {this.ClientEndPoint} ({this.PreAuthData.UserID}) disconnected from target server {TargetAddress}:{TargetPort} with reason {reason}");
                switch (reason)
                {
                    case RejectionReason.Challenge:
                        NetDataWriter writer = new NetDataWriter();
                        writer.Put((byte)RejectionReason.Challenge);
                        if (disconnectInfo.AdditionalData.TryGetByte(out byte type))
                        {
                            writer.Put(type);
                            if (disconnectInfo.AdditionalData.TryGetInt(out int random))
                            {
                                writer.Put(random);
                                if (disconnectInfo.AdditionalData.TryGetBytesWithLength(out byte[] rngBytes))
                                {
                                    writer.PutBytesWithLength(rngBytes);
                                    switch ((ChallengeType)type)
                                    {
                                        case ChallengeType.MD5:
                                        case ChallengeType.SHA1:
                                            if (disconnectInfo.AdditionalData.TryGetUShort(out ushort secretLen))
                                            {
                                                writer.Put(secretLen);
                                                if (disconnectInfo.AdditionalData.TryGetBytesWithLength(out byte[] rngBytes2))
                                                {
                                                    writer.PutBytesWithLength(rngBytes2);
                                                }
                                            }
                                            break;
                                    }
                                }

                            }
                        }
                        ConnectionRequest.Reject(writer);
                        break;
                }
            }

            if (ServerPeer == null)
                DisconnectFromProxy();
            IsConnected = false;
            IsPooling = false;
        }
    }
}
