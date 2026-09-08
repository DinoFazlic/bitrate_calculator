
namespace BitrateCalculator.Models
{
    public sealed record DeviceSnapshot
    {
        public required string Device { get; init; }
        public required string Model { get; init; }
        public required IReadOnlyList<NicSample> Nics { get; init; }
    }
}
