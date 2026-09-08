using System;
using System.Collections.Generic;
using System.Text;

namespace BitrateCalculator.Domain
{
    public sealed record NicSample
    {
        public required string Description { get; init; }
        public required string Mac { get; init; }
        public required DateTimeOffset Timestamp { get; init; }
        public required ulong RxOctets { get; init; }
        public required ulong TxOctets { get; init; }
    }
}
