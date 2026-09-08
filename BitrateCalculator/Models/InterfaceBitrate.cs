
namespace BitrateCalculator.Models
{
    public sealed record InterfaceBitrate
    {
        public required string Description { get; init; }

        public required string Mac { get; init; }

        public required BitrateStatus Status { get; init; }

        public double RxBitsPerSecond { get; init; }

        public double TxBitsPerSecond { get; init; }
        public bool CounterWrapped { get; init; }
    }
}
