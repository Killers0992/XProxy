using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XProxy.Core.Events
{
    public class BaseCancellableEvent : BaseEvent
    {
        public bool IsCancelled { get; set; }
    }
}
