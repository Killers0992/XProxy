﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace XProxy.Models
{
    public class AuthPlayersModel
    {
        [JsonPropertyName("objects")]
        public AuthPlayerModel[] Players { get; set; } = new AuthPlayerModel[0];
    }
}
