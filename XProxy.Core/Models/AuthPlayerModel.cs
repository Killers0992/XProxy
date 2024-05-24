using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XProxy.Models
{
    public class AuthPlayerModel
    {
        public string Id { get; set; }
        public string Ip { get; set; }
        public string RequestIp { get; set; }
        public string Asn { get; set; }
        public string AuthSerial { get; set; }
        public string VacSession { get; set; }
    }
}
