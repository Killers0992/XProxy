using System;
using LiteNetLib;
using LiteNetLib.Utils;
using XProxy.Core.Connections;
using XProxy.Core.Core.Connections.Responses;
using XProxy.Enums;
using XProxy.Services;

namespace XProxy.Core.Core.Connections
{
    public class ConnectionHandler : IDisposable
    {
        private ConnectionValidator _validator;
        private NetManager _netManager;
        private EventBasedNetListener _listener;

        public Player Owner;

        public bool IsValid => _netManager != null;

        public bool IsConnected => IsValid && _netManager.FirstPeer != null;
        public bool IsConnecting;

        public Server Server;

        public string ServerIpAddress => Server.Settings.Ip;

        public int ServerPort => Server.Settings.Port;

        public ConnectionValidator Validator
        {
            get => _validator;
            set
            {
                _validator = value;
            }
        }

        public void Setup(Player player)
        {
            Owner = player;

            if (_listener == null)
            {
                _listener = new EventBasedNetListener();

                _listener.PeerConnectedEvent += OnConnected;
                _listener.NetworkReceiveEvent += OnReceiveData;
                _listener.PeerDisconnectedEvent += OnDisconnected;
            }

            if (_netManager == null)
            {
                _netManager = new NetManager(_listener)
                {
                    UpdateTime = 5,
                    ChannelsCount = (byte)6,
                    DisconnectTimeout = 1000,
                    ReconnectDelay = 300,
                    MaxConnectAttempts = 3,
                };

                _netManager.Start();
            }
        }

        public void Update()
        {
            if (_netManager == null)
                return;

            if (_netManager.IsRunning)
                _netManager.PollEvents();
        }

        public void TryMakeConnection(Server server, NetDataWriter connectionData)
        {
            if (!IsValid)
                return;

            if (IsConnecting)
                return;

            Server = server;

            _netManager.Connect(ServerIpAddress, ServerPort, connectionData);
            IsConnecting = true;
        }

        public void Reconnect(NetDataWriter connectionData)
        {
            if (!IsValid)
                return;

            TryMakeConnection(Server, connectionData);
        }

        public void Send(byte[] bytes, int position, int length, DeliveryMethod method)
        {
            if (!IsConnected)
                return;

            _netManager.FirstPeer.Send(bytes, position, length, method);
        }

        private void OnDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            IsConnecting = false;

            switch (disconnectInfo.Reason)
            {
                case DisconnectReason.ConnectionFailed when disconnectInfo.AdditionalData.RawData == null:
                    if (Validator != null)
                    {
                        Owner.OnConnectionResponse(Server, new ServerIsOfflineResponse());
                        return;
                    }

                    Logger.Info(ConfigService.Singleton.Messages.PlayerServerIsOfflineMessage.Replace("%tag%", Owner.Tag).Replace("%address%", $"{Owner.ClientEndPoint}").Replace("%userid%", Owner.UserId), $"Player");
                    Owner.DisconnectFromProxy(ConfigService.Singleton.Messages.ServerIsOfflineKickMessage.Replace("%server%", Owner.CurrentServer.Name));
                    return;

                case DisconnectReason.ConnectionRejected when disconnectInfo.AdditionalData.RawData != null:
                    NetDataWriter rejectedData = NetDataWriter.FromBytes(disconnectInfo.AdditionalData.RawData, disconnectInfo.AdditionalData.UserDataOffset, disconnectInfo.AdditionalData.UserDataSize);

                    if (!disconnectInfo.AdditionalData.TryGetByte(out byte lastRejectionReason))
                        break;

                    RejectionReason reason = (RejectionReason)lastRejectionReason;

                    bool cancel = false;

                    switch (reason)
                    {
                        case RejectionReason.Delay:
                            if (disconnectInfo.AdditionalData.TryGetByte(out byte offset))
                            {
                                Owner.SaveCurrentServerForNextSession(offset + 10f);
                                Logger.Info(ConfigService.Singleton.Messages.PlayerDelayedConnectionMessage.Replace("%tag%", Owner.Tag).Replace("%address%", $"{Owner.ClientEndPoint}").Replace("%userid%", Owner.UserId).Replace("%time%", $"{offset}"), $"Player");
                            }
                            break;

                        case RejectionReason.ServerFull:
                            if (Validator != null)
                            {
                                if (Owner.TryJoinQueue(Server))
                                    return;

                                Owner.OnConnectionResponse(Server, new ServerIsFullResponse());
                                return;
                            }

                            Logger.Info(ConfigService.Singleton.Messages.PlayerServerIsFullMessage.Replace("%tag%", Owner.Tag).Replace("%address%", $"{Owner.ClientEndPoint}").Replace("%userid%", Owner.UserId), $"Player");
                            break;

                        case RejectionReason.Banned:
                            long expireTime = disconnectInfo.AdditionalData.GetLong();
                            string banReason = disconnectInfo.AdditionalData.GetString();

                            var date = new DateTime(expireTime, DateTimeKind.Utc).ToLocalTime();

                            if (Validator != null)
                            {
                                Owner.OnConnectionResponse(Server, new BannedResponse(banReason, date));
                                return;
                            }

                            Logger.Info(ConfigService.Singleton.Messages.PlayerBannedMessage.Replace("%tag%", Owner.Tag).Replace("%address%", $"{Owner.ClientEndPoint}").Replace("%userid%", Owner.UserId).Replace("%reason%", banReason).Replace("%date%", date.ToShortDateString()).Replace("%time%", date.ToLongTimeString()), $"Player");
                            break;

                        case RejectionReason.Challenge:
                            if (Validator != null)
                            {
                                Validator.ProcessChallenge(disconnectInfo.AdditionalData);
                                return;
                            }

                            //We need to save current server because after client tries to reconnect it will connect to random server from priority list.
                            Owner.SaveCurrentServerForNextSession();
                            Logger.Info(ConfigService.Singleton.Messages.PlayerReceivedChallengeMessage.Replace("%tag%", Owner.Tag).Replace("%address%", $"{Owner.ClientEndPoint}").Replace("%userid%", Owner.UserId), "Player");
                            break;

                        default:
                            Logger.Info(ConfigService.Singleton.Messages.PlayerDisconnectedMessage.Replace("%tag%", Owner.Tag).Replace("%address%", $"{Owner.ClientEndPoint}").Replace("%userid%", Owner.UserId).Replace("%reason%", $"{reason}"), $"Player");
                            break;
                    }

                    if (cancel)
                        return;

                    Owner.DisconnectFromProxy(writer: rejectedData);
                    return;

                case DisconnectReason.Timeout:
                case DisconnectReason.PeerNotFound:
                    Owner.Connection = new LostConnection(Owner, LostConnectionType.Timeout);
                    Logger.Info(ConfigService.Singleton.Messages.PlayerServerTimeoutMessage.Replace("%tag%", Owner.Tag).Replace("%address%", $"{Owner.ClientEndPoint}").Replace("%userid%", Owner.UserId), $"Player");
                    return;

                case DisconnectReason.RemoteConnectionClose when Owner._forceDisconnect:
                    Logger.Info(ConfigService.Singleton.Messages.PlayerDisconnectedWithReasonMessage.Replace("%tag%", Owner.Tag).Replace("%address%", $"{Owner.ClientEndPoint}").Replace("%userid%", Owner.UserId).Replace("%reason%", Owner._forceDisconnectReason), $"Player");
                    break;

                case DisconnectReason.RemoteConnectionClose when !Owner._forceDisconnect:
                    Owner.Connection = new LostConnection(Owner, LostConnectionType.Shutdown);
                    Logger.Info(ConfigService.Singleton.Messages.PlayerServerShutdownMessage.Replace("%tag%", Owner.Tag).Replace("%address%", $"{Owner.ClientEndPoint}").Replace("%userid%", Owner.UserId), $"Player");
                    return;
            }
        }

        private void OnReceiveData(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
        {
            bool cancelProcessor = false;

            Owner.ProcessMirrorData(reader.RawData, reader.Position, reader.AvailableBytes, ref cancelProcessor, false);

            if (!cancelProcessor)
            {
                try
                {
                    Owner.SendDataToProxy(reader.RawData, reader.Position, reader.AvailableBytes, deliveryMethod);
                }
                catch (Exception ex)
                {
                    Logger.Error(ConfigService.Singleton.Messages.PlayerExceptionSendToProxyMessage.Replace("%tag%", Owner.ErrorTag).Replace("%address%", $"{Owner.ClientEndPoint}").Replace("%userid%", Owner.UserId).Replace("%message%", $"{ex}"), "Player");
                }
            }

            reader.Recycle();
        }

        private void OnConnected(NetPeer peer)
        {
            IsConnecting = false;

            // If main connection is not done se connection as proxied.
            if (!Owner.MainConnectionHandler.IsConnected)
            {
                Owner.CurrentServer = Server;

                Owner.Connection = new ProxiedConnection(Owner);

                Owner.MainConnectionHandler = this;
                return;
            }

            if (Validator != null)
            {
                Owner.FastRoundrestart();
                Owner.NotReady();

                Owner.MainConnectionHandler = this;
                Owner.CurrentServer = Server;
                return;
            }
        }

        public void Dispose()
        {
            if (_listener != null)
            {
                _listener.PeerConnectedEvent -= OnConnected;
                _listener.NetworkReceiveEvent -= OnReceiveData;
                _listener.PeerDisconnectedEvent -= OnDisconnected;

                _listener = null;
            }

            if (_netManager != null)
            {
                if (IsConnected)
                    _netManager.FirstPeer.Disconnect();

                _netManager?.Stop();
                _netManager = null;
            }
        }
    }
}
