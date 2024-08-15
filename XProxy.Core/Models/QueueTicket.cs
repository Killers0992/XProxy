using System;

namespace XProxy.Core.Models
{
    public class QueueTicket
    {
        public string UserId { get; set; }
        public string ServerKey { get; set; }
        public bool IsConnecting { get; set; }
        public DateTime TicketLifetime { get; set; }
    }
}
