
namespace BitrateCalculator.Simulation
{
    public sealed class SimulatedNic
    {
        private const ulong Counter32Modulus = 4294967296;
        private const int BitsPerOctet = 8;
        public string Description { get; }

        public string Mac { get; }

        public double RxBitsPerSecond { get; }

        public double TxBitsPerSecond { get; }

        public ulong RxOctets { get; private set; }

        public ulong TxOctets { get; private set; }
        public double Variation { get; }
        public SimulatedNic(
            string description,
            string mac,
            double rxBitsPerSecond,
            double txBitsPerSecond,
            ulong startingRxOctets,
            ulong startingTxOctets,
            double variation = 0.0)
        {
            Description = description;
            Mac = mac;
            RxBitsPerSecond = rxBitsPerSecond;
            TxBitsPerSecond = txBitsPerSecond;
            RxOctets = startingRxOctets;
            TxOctets = startingTxOctets;
            Variation = variation;
        }

        public void Advance(double elapsedSeconds)
        {
            RxOctets = AddOctets(RxOctets, ApplyVariation(RxBitsPerSecond), elapsedSeconds);
            TxOctets = AddOctets(TxOctets, ApplyVariation(TxBitsPerSecond), elapsedSeconds);
        }

        private double ApplyVariation(double bitsPerSecond)
        {
            if (Variation <= 0)
            {
                return bitsPerSecond;
            }

            // A random factor somewhere between (1 - Variation) and (1 + Variation).
            double factor = 1.0 + ((Random.Shared.NextDouble() * 2.0) - 1.0) * Variation;

            return bitsPerSecond * factor;
        }

        private static ulong AddOctets(ulong current, double bitsPerSecond, double elapsedSeconds)
        {
            double octets = bitsPerSecond * elapsedSeconds / BitsPerOctet;
            ulong octetsToAdd = (ulong)octets;

            // A real 32-bit counter wraps back to zero instead of growing forever.
            return (current + octetsToAdd) % Counter32Modulus;
        }

    }
}
