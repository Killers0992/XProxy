using System;
using XProxy.Models;
using XProxy.Services;

namespace XProxy.Core.Models
{
    public class QueueTicket
    {
        bool? _lastOfflineStatus = false;
        DateTime _offlineFor = DateTime.Now;
        Server _info;

        public QueueTicket(string userId, Server server)
        {
            UserId = userId;
            _info = server;
        }

        public string UserId { get; private set; }
        public int Position => _info.PlayersInQueueByUserId.IndexOf(UserId) + 1;
        public bool IsConnecting { get; private set; }
        public DateTime TicketLifetime { get; private set; }

        public bool IsPlayerOffline => !ProxyService.Singleton.PlayersByUserId.ContainsKey(UserId);
        public TimeSpan OfflineTime => DateTime.Now - _offlineFor;

        public bool IsPlayerConnected
        {
            get
            {
                var plr = ProxyService.Singleton.GetPlayerByUserId(UserId);

                if (plr == null) return false;

                return plr.IsConnectedToCurrentServer;
            }
        }

        public bool IsTicketExpired()
        {
            if (IsConnecting)
                return TicketLifetime < DateTime.Now;

            if (IsPlayerOffline)
            {
                if (_lastOfflineStatus.HasValue)
                {
                    return OfflineTime.TotalSeconds > 5;
                }
                else
                {
                    _offlineFor = DateTime.Now;
                    _lastOfflineStatus = true;
                    return false;
                }
            }
            else if (IsPlayerConnected)
                return true;
            else
                _lastOfflineStatus = null;

            return false;
        }

        public void MarkAsConnecting()
        {
            IsConnecting = true;
            TicketLifetime = DateTime.Now.AddSeconds(15);
        }
    }
}
