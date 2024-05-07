using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XProxy.Models
{
    public class PublicKeyResponseModel
    {
        public string Key { get; set; }
        public string Signature { get; set; }
        public string Credits { get; set; }
    }
}
