using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace BitrateCalculator.Api
{
    public sealed class NicResponse
    {
        public string? Description { get; set; }
        [JsonPropertyName("MAC")]
        public string? Mac { get; set; }
        public string? Timestamp { get; set; }
        public string? Rx { get; set; }
        public string? Tx { get; set; }

    }
}
