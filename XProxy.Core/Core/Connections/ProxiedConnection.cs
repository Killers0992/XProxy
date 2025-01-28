using LiteNetLib;
using System;
using XProxy.Services;

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
            Logger.Info(ConfigService.Singleton.Messages.PlayerConnectedMessage.Replace("%tag%", Player.Tag).Replace("%address%", $"{Player.ClientEndPoint}").Replace("%userid%", Player.UserId), $"Player");
        }

        public override void OnReceiveDataFromProxy(NetPacketReader reader, DeliveryMethod method)
        {
            try
            {
                Player.MainConnectionHandler.Send(reader.RawData, reader.Position, reader.AvailableBytes, method);
            }
            catch (Exception ex)
            {
                Logger.Error(ConfigService.Singleton.Messages.PlayerExceptionSendToServerMessage.Replace("%tag%", Player.ErrorTag).Replace("%address%", $"{Player.ClientEndPoint}").Replace("%userid%", Player.UserId).Replace("%message%", $"{ex}"), "Player");
            }
        }
    }
}
