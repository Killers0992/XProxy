using Cryptography;
using LiteNetLib;
using LiteNetLib.Utils;
using Mirror;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using RoundRestarting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XProxy.CustomMessages;
using XProxy.Enums;
using XProxy.Models;
using static TeslaGateController;

namespace XProxy
{
    public class ProxyClient : INetEventListener
    {
        public bool Redirecting { get; set; }
        private ProxyServer server;
        public ProxyClient(ProxyServer server, ConnectionRequest connectionRequest, PreAuthModel preAuth)
        {
            this.server = server;
            PreAuthData = preAuth;
            ConnectionRequest = connectionRequest;

            Manager = new NetManager(this);
            Manager.IPv6Enabled = IPv6Mode.SeparateSocket;
            Manager.UpdateTime = 5;
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

            Manager.Start();

            Task.Run(async () =>
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

        public NetPeer ConnectTo(string address, int port, bool reusePreAuth = false)
        {
            this.TargetAddress = address;
            this.TargetPort = port;
            Console.WriteLine($"Connecting client {this.ClientEndPoint} ({this.PreAuthData.UserID}) => {address}:{port}");
            NetPeer conPeer = null;
            if (Manager != null)
            {
                if (IsConnected)
                    Manager.DisconnectAll();
                conPeer = Manager.Connect(address, port, reusePreAuth ? PreAuthData.RegenPreAuth() : PreAuthData.RawPreAuth);
            }
            Console.WriteLine(PreAuthData.ToString());
            IsPooling = true;
            Redirecting = false;
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
            end = true;
        }


        //Server -> ( Proxy Client ) -> Client
        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            try
            {
                using (PooledNetworkReader reader2 = NetworkReaderPool.GetReader(new ArraySegment<byte>(reader.RawData, reader.Position, reader.AvailableBytes)))
                {
                    while (reader2.Position < reader2.Length)
                    {
                        bool cancel = false;
                        if (!UnpackAndInvoke(reader2, -1, ref cancel, false))
                        {
                            break;
                        }
                        if (cancel)
                            goto skip;
                    }
                }
            }
            catch (Exception) { }


            try
            {
                ServerPeer.Send(reader.RawData, reader.Position, reader.AvailableBytes, deliveryMethod);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error {ex}");
            }

        skip:

            reader.Recycle();
        }

        //Client -> ( Proxy Server ) -> Server
        public void ReceiveData(NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            if (Manager.FirstPeer == null)
            {
                reader.Recycle();
                return;
            }

            try
            {
                using (PooledNetworkReader reader2 = NetworkReaderPool.GetReader(new ArraySegment<byte>(reader.RawData, reader.Position, reader.AvailableBytes)))
                {
                    while (reader2.Position < reader2.Length)
                    {
                        bool cancel = false;
                        if (!UnpackAndInvoke(reader2, -1, ref cancel, false))
                        {
                            break;
                        }
                        if (cancel)
                            goto skip;
                    }
                }
            }
            catch (Exception) { }


            try
            {
                
                Manager.FirstPeer.Send(reader.RawData, reader.Position, reader.AvailableBytes, deliveryMethod);
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Error {ex}");
            }
            skip:
            reader.Recycle();
        }

        public bool UnpackAndInvoke(NetworkReader reader, int channelId, ref bool cancelPacket, bool isServer)
        {
            if (!MessagePacking.Unpack(reader, out ushort key))
                return false;

            var res = MessagePacking.GetId<RoundRestarting.RoundRestartMessage>();

            if (res == key)
            {
                ProxyServer.ServerOverride.Add(ClientEndPoint.Address.ToString(), new ProxyServerData()
                {
                    Address = TargetAddress,
                    Port = TargetPort,
                });
                Console.WriteLine($"Saving session {PreAuthData.UserID} ({ClientEndPoint.Address.ToString()}) to server {TargetAddress}:{TargetPort}, disconencting from proxy.");
            }

            switch (key)
            {
                //RPC Message
                case 33978:
                    var rpcMessage = new RpcMessage
                    {
                        netId = reader.ReadUInt32(),
                        componentIndex = reader.ReadInt32(),
                        functionHash = reader.ReadInt32(),
                        payload = reader.ReadBytesAndSizeSegment()
                    };
                    switch (rpcMessage.functionHash)
                    {
                        case -0169:
                            using (var poolReader = NetworkReaderPool.GetReader(rpcMessage.payload))
                            {
                                switch (poolReader.ReadByte())
                                {
                                    //Receive encryption keys for commands.
                                    case 1:
                                        Console.WriteLine("Receive encryption keys");
                                        ServerEncryptionKey = poolReader.ReadBytesAndSize();
                                        break;
                                }
                            }
                            cancelPacket = true;
                            break;
                    }
                    break;
                //Command Message
                case 46228:
                    var commandMessage = new CommandMessage
                    {
                        netId = reader.ReadUInt32(),
                        componentIndex = reader.ReadInt32(),
                        functionHash = reader.ReadInt32(),
                        payload = reader.ReadBytesAndSizeSegment()
                    };
                    switch (commandMessage.functionHash)
                    {
                        //Console command message.
                        case -1962147612:
                            using (PooledNetworkReader payloadReader = NetworkReaderPool.GetReader(commandMessage.payload))
                            {
                                var data = payloadReader.ReadBytesAndSize();
                                var encrypted = payloadReader.ReadBoolean();

                                if (encrypted)
                                {
                                    if (ServerEncryptionKey == null)
                                    {
                                        Console.WriteLine("Can't process encrypted message from server before completing ECDHE exchange.");
                                        break;
                                    }

                                    try
                                    {
                                        var query = Utf8.GetString(AES.AesGcmDecrypt(data, ServerEncryptionKey));
                                        Console.WriteLine($"Client {ClientEndPoint.Address} connected to {TargetAddress}:{TargetPort} sended console command, Query: {query}.");
                                        var sp = query.Split(' ');
                                        switch (sp[0].ToUpper())
                                        {
                                            case "SENDTO":
                                                Redirecting = true;
                                                ConnectTo(sp[1], int.Parse(sp[2]), true);
                                                cancelPacket = true;
                                                break;
                                            case "SERVERS":

                                                cancelPacket = true;
                                                break;
                                        }
                                    }
                                    catch
                                    {
                                        Console.WriteLine($"Decryption or verification of encrypted message failed.");
                                    }
                                }
                                
                            }
                            break;
                    }
                    break;
            }
            return true;
        }

        public byte[] ServerEncryptionKey;

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
            if (Program.config.servers.ContainsKey(TargetPort))
                Program.config.servers[TargetPort].Players++;
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            if (Program.config.servers.ContainsKey(TargetPort))
                Program.config.servers[TargetPort].Players--;

            if (disconnectInfo.AdditionalData.RawData == null && ConnectionRequest != null)
            {
                var targetServer = server.TakeFreeServer(ClientEndPoint.Address.ToString());
                if (targetServer == null)
                {
                    NetDataWriter writer = new NetDataWriter();
                    writer.Put((byte)RejectionReason.Custom);
                    writer.Put("Server is offline.");
                    ConnectionRequest.Reject(writer);
                    goto skipChecking;
                }
                goto skipChecking;
            }

            if (disconnectInfo.AdditionalData.RawData != null)
            {
                NetDataWriter writer2 = NetDataWriter.FromBytes(disconnectInfo.AdditionalData.RawData, disconnectInfo.AdditionalData.UserDataOffset, disconnectInfo.AdditionalData.UserDataSize);
                if (disconnectInfo.AdditionalData.TryGetByte(out byte lastRejectionReason))
                {
                    RejectionReason reason = (RejectionReason)lastRejectionReason;
                    Console.WriteLine($"Client {this.ClientEndPoint} ({this.PreAuthData.UserID}) disconnected from target server {TargetAddress}:{TargetPort} with reason {reason}");
                    if (ConnectionRequest != null)
                    {
                        if (reason == RejectionReason.Challenge)
                        {
                            if (!ProxyServer.ServerOverride.ContainsKey(ClientEndPoint.Address.ToString()))
                                ProxyServer.ServerOverride.Add(ClientEndPoint.Address.ToString(), new ProxyServerData()
                                {
                                    Address = TargetAddress,
                                    Port = TargetPort
                                });
                        }

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
            }
            else
            {
                ServerPeer.Disconnect();
            }
        skipChecking:
            DisconnectFromProxy();
            IsConnected = false;
            IsPooling = false;
        }
    }
}
