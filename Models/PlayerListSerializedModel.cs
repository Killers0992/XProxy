using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace XProxy.Models
{
    public class PlayerListSerializedModel
    {
        [JsonPropertyName("objects")]
        public List<string> UserIds { get; set; } = new List<string>();
    }
}
