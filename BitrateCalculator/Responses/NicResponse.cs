using System.Text.Json.Serialization;

namespace BitrateCalculator.Responses
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
