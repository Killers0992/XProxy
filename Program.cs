using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Threading;

namespace XProxy
{
    class Program
    {
        public static NetManager manager;
        private static EventBasedNetListener listener;

        static void Main(string[] args)
        {
            listener = new EventBasedNetListener();
            listener.ConnectionRequestEvent += Listener_ConnectionRequestEvent;
            listener.PeerDisconnectedEvent += Listener_PeerDisconnectedEvent;
            listener.NetworkReceiveEvent += Listener_NetworkReceiveEvent;
            manager = new NetManager(listener);
            manager.IPv6Enabled = IPv6Mode.SeparateSocket;
            manager.UpdateTime = 15;
            manager.PingInterval = 1000;
            manager.DisconnectTimeout = 5000;
            manager.ReconnectDelay = 500;
            manager.MaxConnectAttempts = 10;
            manager.BroadcastReceiveEnabled = false;
            manager.ChannelsCount = 5;

           

            manager.Start(7887);
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    await Task.Delay(15);
                    if (manager != null)
                        if (manager.IsRunning)
                            manager.PollEvents();
                }
            });

            Console.WriteLine($"Proxy is running on port 7887");
            Console.ReadKey();
           
        }

        private static void Listener_PeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            if (clients.TryGetValue(peer, out ProxyClient prox))
            {
                prox.token.Cancel();
                clients.Remove(peer);
            }
        }

        private static void Listener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            if (clients.TryGetValue(peer, out ProxyClient prox))
            {
                prox.manager.FirstPeer.Send(reader.RawData, reader.UserDataOffset, reader.UserDataSize, reader.RawData[3], deliveryMethod);
                reader.Recycle();
            }
        }


        public class ProxyClient
        {
            public ConnectionRequest connectionRequest { get; set; }
            public NetPeer linkedPeer { get; set; }
            public bool isReady { get; set; } = false;
            public bool isConnected { get; set; } = false;
            public bool IsPooling { get; set; } = false;
            public CancellationTokenSource token { get; set; }
            public Task managerTask { get; set; }
            public NetManager manager { get; set; }
            public EventBasedNetListener listener { get; set; }
            public NetPeer peer { get; set; }
            public PreAuthData PreAuth { get; set; }
            public string TargetIP { get; set; } = "127.0.0.1";
            public int TargetPORT { get; set; } = 9999;
        }

        public static Dictionary<NetPeer, ProxyClient> clients = new Dictionary<NetPeer, ProxyClient>();

        public class PreAuthData
        {
            private NetDataWriter _conData = new NetDataWriter();
            public NetDataWriter ConnectionData
            {
                get
                {
                    return _conData;
                }
                set
                {
                    _conData = value;
                }
            }
            public byte b { get; set; }
            public byte major { get; set; }
            public byte minor { get; set; }
            public byte revision { get; set; }
            public byte backwardRevision { get; set; }
            public bool flag { get; set; }
            public int challegneid { get; set; } = 0;
            public byte[] challenge { get; set; }
            public string userid { get; set; }
            public long expiration { get; set; }
            public byte flags { get; set; }
            public string region { get; set; }
            public byte[] signature { get; set; }
        }

        private static void Listener_ConnectionRequestEvent(ConnectionRequest request)
        {
            Console.WriteLine($"New connection request from " + request.RemoteEndPoint.Address);
            /* NetDataWriter writer = new NetDataWriter();
             writer.Put((byte)RejectionReason.Custom);
             writer.Put("Test\n<color=red>colors</color>");
            */

            ProxyClient prox = new ProxyClient()
            {
                PreAuth = new PreAuthData(),
                connectionRequest = request
            };

            if (request.Data.TryGetByte(out byte b));
                prox.PreAuth.ConnectionData.Put(b);

            byte cBackwardRevision = 0;
            byte cMajor;
            byte cMinor;
            byte cRevision;
            bool flag;

            if (!request.Data.TryGetByte(out cMajor) || !request.Data.TryGetByte(out cMinor) || !request.Data.TryGetByte(out cRevision) || !request.Data.TryGetBool(out flag) || (flag && !request.Data.TryGetByte(out cBackwardRevision)))
            {
                NetDataWriter writer = new NetDataWriter();
                writer.Put((byte)RejectionReason.VersionMismatch);
                request.RejectForce(writer);
                return;
            }
            prox.PreAuth.ConnectionData.Put(cMajor);
            prox.PreAuth.ConnectionData.Put(cMinor);
            prox.PreAuth.ConnectionData.Put(cRevision);
            prox.PreAuth.ConnectionData.Put(cBackwardRevision);

            Console.WriteLine($"Version {cMajor}.{cMinor}.{cRevision}. Backward version {cBackwardRevision}");

            if (request.Data.TryGetInt(out int challengeid))
            {
                prox.PreAuth.ConnectionData.Put(challengeid);
                Console.WriteLine($"Challenge ID " + challengeid);
                if (challengeid != 0)
                {
                    prox.linkedPeer = request.Accept();
                    prox.connectionRequest = null;
                    clients.Add(prox.linkedPeer, prox);
                }
            }


            if (request.Data.TryGetBytesWithLength(out byte[] array))
                prox.PreAuth.ConnectionData.PutBytesWithLength(array);

            if (request.Data.TryGetString(out string userid))
                prox.PreAuth.ConnectionData.Put(userid);
            Console.WriteLine($"UserID {userid}");

            if (request.Data.TryGetLong(out long num3))
                prox.PreAuth.ConnectionData.Put(num3);
            Console.WriteLine($"Expiration {num3}");

            if (request.Data.TryGetByte(out byte b3))
                prox.PreAuth.ConnectionData.Put(b3);
            Console.WriteLine($"Flags {b3}");

            if (request.Data.TryGetString(out string text2))
                prox.PreAuth.ConnectionData.Put(text2);
            Console.WriteLine($"Region {text2}");

            if (request.Data.TryGetBytesWithLength(out byte[] signature))
                prox.PreAuth.ConnectionData.PutBytesWithLength(signature);


            Console.WriteLine($"New peer connected to proxy {request.RemoteEndPoint.Address} redirecting data to server {prox.TargetIP}:{prox.TargetPORT}.");
            prox.listener = new EventBasedNetListener();
            prox.listener.NetworkReceiveEvent += (peer, data, delivery) =>
            {
                prox.linkedPeer.Send(data.RawData, data.UserDataOffset, data.UserDataSize, data.RawData[3], delivery);
                data.Recycle();
            };
            prox.listener.NetworkErrorEvent += (endPoint, error) =>
            {
                Console.WriteLine($"Socket error for {endPoint.Address}, reason: {error}");
            };
            prox.listener.PeerConnectedEvent += (peer) =>
            {
                Console.WriteLine($"Peer {peer.EndPoint.Address} connected to target server {prox.TargetIP}:{prox.TargetPORT}.");
                prox.isConnected = true;
            };
            prox.listener.ConnectionRequestEvent += (peer) =>
            {
                Console.WriteLine($"Peer {peer.RemoteEndPoint.Address} sended connection request to {prox.TargetIP}:{prox.TargetPORT}.");
            };
            prox.listener.PeerDisconnectedEvent += (peer, reason) =>
            {
                if (peer == null)
                    return;
                Console.WriteLine($"Peer {peer.EndPoint.Address} disconnected from target server {prox.TargetIP}:{prox.TargetPORT}, reason: {reason.Reason}.");
                if (reason.AdditionalData.TryGetByte(out byte lastRejectionReason))
                {
                    Console.WriteLine((RejectionReason)lastRejectionReason);
                    switch ((RejectionReason)lastRejectionReason)
                    {
                        case RejectionReason.Challenge:
                            NetDataWriter writer = new NetDataWriter();
                            writer.Put((byte)RejectionReason.Challenge);
                            if (reason.AdditionalData.TryGetByte(out byte type))
                            {
                                writer.Put(type);
                                if (reason.AdditionalData.TryGetInt(out int random))
                                {
                                    writer.Put(random);
                                    if (reason.AdditionalData.TryGetBytesWithLength(out byte[] rngBytes))
                                    {
                                        writer.PutBytesWithLength(rngBytes);
                                        switch ((ChallengeType)type)
                                        {
                                            case ChallengeType.MD5:
                                            case ChallengeType.SHA1:
                                                if (reason.AdditionalData.TryGetUShort(out ushort secretLen))
                                                {
                                                    writer.Put(secretLen);
                                                    if (reason.AdditionalData.TryGetBytesWithLength(out byte[] rngBytes2))
                                                    {
                                                        writer.PutBytesWithLength(rngBytes2);
                                                    }
                                                }
                                                break;
                                        }
                                    }

                                }
                            }
                            prox.connectionRequest.Reject(writer);
                            break;
                    }
                    return;
                }
                prox.isConnected = false;
                prox.IsPooling = false;
            };
            prox.manager = new NetManager(prox.listener);
            prox.manager.IPv6Enabled = IPv6Mode.SeparateSocket;
            prox.manager.UpdateTime = 15;
            prox.manager.PingInterval = 1000;
            prox.manager.DisconnectTimeout = 5000;
            prox.manager.ReconnectDelay = 500;
            prox.manager.MaxConnectAttempts = 10;
            prox.manager.BroadcastReceiveEnabled = false;
            prox.manager.ChannelsCount = 5;
            prox.manager.Start();

            prox.token = new CancellationTokenSource();
            CancellationToken ct = prox.token.Token;
            prox.managerTask = Task.Factory.StartNew(async () =>
            {
                ct.ThrowIfCancellationRequested();
                while (true)
                {
                    if (prox.manager != null && prox.IsPooling)
                        if (prox.manager.IsRunning)
                            prox.manager.PollEvents();
                    await Task.Delay(15);
                    if (ct.IsCancellationRequested)
                        ct.ThrowIfCancellationRequested();
                }
            }, ct);
            prox.peer = prox.manager.Connect(prox.TargetIP, prox.TargetPORT, prox.PreAuth.ConnectionData);
            prox.IsPooling = true;

            prox.isReady = true;
        }
    }



}