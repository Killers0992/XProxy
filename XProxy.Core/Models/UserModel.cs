using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XProxy.Core.Models
{
    public class UserModel
    {
        [Description("If player can join when maintenance is enabled.")]
        public bool IgnoreMaintenance { get; set; }
    }
}
