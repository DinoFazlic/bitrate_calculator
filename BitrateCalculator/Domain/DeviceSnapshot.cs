using System;
using System.Collections.Generic;
using System.Text;

namespace BitrateCalculator.Domain
{
    public sealed record DeviceSnapshot
    {
        public required string Device { get; init; }
        public required string Model { get; init; }
        public required IReadOnlyList<NicSample> Nics { get; init; }
    }
}
