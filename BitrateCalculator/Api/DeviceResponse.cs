using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace BitrateCalculator.Api
{
    public sealed class DeviceResponse
    {
        public string? Device { get; set; }
        public string? Model { get; set; }
        [JsonPropertyName("NIC")]
        public List<NicResponse>? Nics { get; set; } 
    }
}
