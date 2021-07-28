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

        public void ConnectTo(string address, int port)
        {
            this.TargetAddress = address;
            this.TargetPort = port;
            Console.WriteLine($"Connecting client {this.ClientEndPoint} ({this.PreAuthData.UserID}) => {address}:{port}");
            if (Manager != null)
                Manager.Connect(address, port, PreAuthData.RawPreAuth);
            Console.WriteLine(PreAuthData.ToString());
            IsPooling = true;
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

        public void ReceiveData(NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            if (Manager.FirstPeer == null)
                return;
            ProcessData(ServerPeer.Id + 1, reader.GetRemainingBytesSegment(), -1);
            Manager.FirstPeer.Send(reader.RawData, reader.UserDataOffset, reader.UserDataSize, reader.RawData[3], deliveryMethod);
            reader.Recycle();
        }

        public void ProcessData(int connectionId, ArraySegment<byte> data, int channelId)
        {
            NetworkReader reader = NetworkReaderPool.GetReader(data);
            int num;
            if (MessagePacker.UnpackMessage(reader, out num))
            {
                NetworkMessage netMsg = new NetworkMessage
                {
                    reader = reader,
                    conn = null,
                    channelId = channelId
                };
                switch (num)
                {
                    case 33978:
                        RpcMessage rpc = netMsg.ReadMessage<RpcMessage>();
                        switch (rpc.functionHash)
                        {
                            case 1359606278:
                                NetworkReader reader2 = NetworkReaderPool.GetReader(rpc.payload);
                                Console.WriteLine("RPC FROM PlayerStats.RpcRoundRestart (server), Time " + reader2.ReadSingle() + ", Bool " + reader2.ReadBoolean());
                                break;
            
                            case 1377427148:
                                //Console.WriteLine("RPC FROM PlayerStats.TargetSyncHp (server)");
                                /*NetworkWriter writer2 = NetworkWriterPool.GetWriter();
                                writer2.WriteSingle(0f);
                                writer2.WriteBoolean(true);
                                RpcMessage msg = new RpcMessage
                                {
                                    netId = rpc.netId,
                                    componentIndex = rpc.componentIndex,
                                    functionHash = 1258067280,
                                    payload = writer2.ToArraySegment()
                                };
                                NetworkWriter writer = NetworkWriterPool.GetWriter();
                                MessagePacker.Pack<RpcMessage>(msg, writer);
                                ArraySegment<byte> segment = writer.ToArraySegment();
                                ServerPeer.Send(segment.Array, segment.Offset, segment.Count, (byte)((channelId < 5) ? channelId : 0), DeliveryMethod.ReliableOrdered);
                                NetworkWriterPool.Recycle(writer);*/
                                break;
                        }
                        break;
                    case 46228:
                        CommandMessage cmd = netMsg.ReadMessage<CommandMessage>();
                        switch (cmd.functionHash)
                        {
                            //Client console command
                            case -1962147612:
                                Console.WriteLine($"Client executed some command (client)");
                                break;
                        }
                        //Console.WriteLine($"Received Command Message from server");
                        break;
                }
            }
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

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {            
            ProcessData(peer.Id + 1, reader.GetRemainingBytesSegment(), -1);
            ServerPeer.Send(reader.RawData, reader.UserDataOffset, reader.UserDataSize, deliveryMethod);
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
            else
            {
                ServerPeer.Disconnect();
            }
            if (ServerPeer == null && !IsRedirecting)
                DisconnectFromProxy();
            IsConnected = false;
            IsPooling = false;
        }
    }
}
