using Cryptography;
using LiteNetLib;
using LiteNetLib.Utils;
using Mirror;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
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


        //Server -> ( Proxy Client ) -> Client
        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            /*  try
              {
                  var segment = new ArraySegment<byte>(reader.RawData, reader.UserDataOffset, reader.UserDataSize);
                  using (PooledNetworkReader pooledReader = NetworkReaderPool.GetReader(segment))
                  {
                      while (pooledReader.Position < pooledReader.Length)
                      {
                          var cancelSend = false;
                          if (!this.UnpackAndInvoke(pooledReader, -1, ref cancelSend, false))
                          {
                              break;
                          }
                      }
                  }
              }
              catch (Exception ex)
              {
                  //Console.WriteLine($"Error " + ex.ToString());
              }
                                      */
            try
            {
                ServerPeer.Send(reader.RawData, reader.Position, reader.AvailableBytes, deliveryMethod);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error {ex}");
            }
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
            /*
               try
               {
                   var segment = new ArraySegment<byte>(reader.RawData, reader.UserDataOffset, reader.UserDataSize);

                   using (PooledNetworkReader pooledReader = NetworkReaderPool.GetReader(segment))
                   {
                       while (pooledReader.Position < pooledReader.Length)
                       {
                           var cancelSend = false;

                           if (!this.UnpackAndInvoke(pooledReader, -1, ref cancelSend, true))
                           {
                               break;
                           }
                       }
                   }
               }
               catch (Exception ex)
               {
                   //Console.WriteLine($"Error " + ex.ToString());
               }                    */

            try
            {
                
                Manager.FirstPeer.Send(reader.RawData, reader.Position, reader.AvailableBytes, deliveryMethod);
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Error {ex}");
            }
            reader.Recycle();
        }

        public bool UnpackAndInvoke(NetworkReader reader, int channelId, ref bool cancelPacket, bool isServer)
        {
            if (!MessagePacking.Unpack(reader, out ushort key))
                return false;

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
                        //TargetDiffieHellmanExchange
                        case -663499796:
                            break;
                    }
                    break;
                //PositionMessage
                case 46471:
                    if (isServer)
                    {
                        if (!Program.ShowDebugLogsFromServer)
                            Console.WriteLine($"[Client -> (Proxy Server) -> Server] Received PositionMessage.");
                    }
                    else
                    {
                        if (Program.ShowDebugLogsFromServer)
                            Console.WriteLine($"[Server -> (Proxy Client) -> Client] Received PositionMessage.");
                    }
                    break;
                //RotationMessage
                case 26002:
                    if (isServer)
                    {
                        if (!Program.ShowDebugLogsFromServer)
                            Console.WriteLine($"[Client -> (Proxy Server) -> Server] Received RotationMessage.");
                    }
                    else
                    {
                        if (Program.ShowDebugLogsFromServer)
                            Console.WriteLine($"[Server -> (Proxy Client) -> Client] Received RotationMessage.");
                    }
                    break;
                //PositionMessage2D
                case 30233:
                    if (isServer)
                    {
                        if (!Program.ShowDebugLogsFromServer)
                            Console.WriteLine($"[Client -> (Proxy Server) -> Server] Received PositionMessage2D.");
                    }
                    else
                    {
                        if (Program.ShowDebugLogsFromServer)
                            Console.WriteLine($"[Server -> (Proxy Client) -> Client] Received PositionMessage2D.");
                    }
                    break;
                //PositionMessage2DJump
                case 10727:
                    if (isServer)
                    {
                        if (!Program.ShowDebugLogsFromServer)
                            Console.WriteLine($"[Client -> (Proxy Server) -> Server] Received PositionMessage2DJump.");
                    }
                    else
                    {
                        if (Program.ShowDebugLogsFromServer)
                            Console.WriteLine($"[Server -> (Proxy Client) -> Client] Received PositionMessage2DJump.");
                    }
                    break;
                //PositionPPMMessage
                case 40794:
                    if (isServer)
                    {
                        if (!Program.ShowDebugLogsFromServer)
                            Console.WriteLine($"[Client -> (Proxy Server) -> Server] Received PositionPPMMessage.");
                    }
                    else
                    {
                        if (Program.ShowDebugLogsFromServer)
                            Console.WriteLine($"[Server -> (Proxy Client) -> Client] Received PositionPPMMessage.");
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

                                Console.WriteLine(encrypted);
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
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            if (disconnectInfo.AdditionalData.RawData == null && ConnectionRequest != null)
            {
                NetDataWriter writer = new NetDataWriter();
                writer.Put((byte)RejectionReason.Custom);
                writer.Put("Server is offline.");
                ConnectionRequest.Reject(writer);
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
            if (ServerPeer == null && !IsRedirecting)
                DisconnectFromProxy();
            IsConnected = false;
            IsPooling = false;
        }
    }
}
