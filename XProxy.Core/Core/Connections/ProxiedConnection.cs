using LiteNetLib;
using System;

namespace XProxy.Core.Connections
{
    public class ProxiedConnection : BaseConnection
    {
        public ProxiedConnection(Player plr) : base(plr)
        {
            plr.InternalAcceptConnection(this); 
            plr.ProcessMirrorMessagesFromProxy = true;
            plr.ProcessMirrorMessagesFromCurrentServer = true;
        }

        public override void OnConnected()
        {
            Logger.Info(Player.Proxy._config.Messages.PlayerConnectedMessage.Replace("%tag%", Player.Tag).Replace("%address%", $"{Player.ClientEndPoint}").Replace("%userid%", Player.UserId), $"Player");
        }

        public override void OnReceiveDataFromProxy(NetPacketReader reader, DeliveryMethod method)
        {
            try
            {
                Player.SendDataToCurrentServer(reader.RawData, reader.Position, reader.AvailableBytes, method);
            }
            catch (Exception ex)
            {
                Logger.Error(Player.Proxy._config.Messages.PlayerExceptionSendToServerMessage.Replace("%tag%", Player.ErrorTag).Replace("%address%", $"{Player.ClientEndPoint}").Replace("%userid%", Player.UserId).Replace("%message%", $"{ex}"), "Player");
            }
        }
    }
}
