using System.Text.Json.Serialization;

namespace BitrateCalculator.Responses
{
    public sealed class DeviceResponse
    {
        public string? Device { get; set; }
        public string? Model { get; set; }
        [JsonPropertyName("NIC")]
        public List<NicResponse>? Nics { get; set; } 
    }
}
