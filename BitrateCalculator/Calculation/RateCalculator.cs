using BitrateCalculator.Models;


namespace BitrateCalculator.Calculation
{
    public sealed class RateCalculator
    {
        private const int BitsPerOctet = 8;
        private const ulong Counter32Modulus = 4294967296; // Max for counter to count before wraps back to 0
        private readonly TimeSpan _maximumElapsedTime;
        private readonly double _maximumPlausibleBitsPerSecond;

        public RateCalculator(TimeSpan maximumElapsedTime, double maximumPlausibleBitsPerSecond)
        {
            _maximumElapsedTime = maximumElapsedTime;
            _maximumPlausibleBitsPerSecond = maximumPlausibleBitsPerSecond;
        }

        public List<InterfaceBitrate> Calculate(DeviceSnapshot previous, DeviceSnapshot current)
        {
            Dictionary<string, NicSample> previousByMac = new Dictionary<string, NicSample>();

            foreach(NicSample nic in previous.Nics)
            {
                previousByMac[nic.Mac] = nic;
            }

            List<InterfaceBitrate> results = new List<InterfaceBitrate>();

            foreach(NicSample nic in current.Nics)
            {
                results.Add(CalculateOne(previousByMac, nic));
            }

            return results;
        }

        private InterfaceBitrate CalculateOne(Dictionary<string, NicSample> previousByMac, NicSample current)
        {
            NicSample previous;

            // If no previous counters for this MAC
            if (!previousByMac.TryGetValue(current.Mac, out previous))
            {
                return Unavailable(current, BitrateStatus.NoPreviousSample);
            }

            TimeSpan elapsed = current.Timestamp - previous.Timestamp;

            // negative means the samples arrived out of order.
            if (elapsed <= TimeSpan.Zero)
            {
                return Unavailable(current, BitrateStatus.InvalidElapsedTime);
            }

            // If polling was interrupted, the two samples are too far apart to do an accurate calculation
            if (elapsed > _maximumElapsedTime)
            {
                return Unavailable(current, BitrateStatus.SampleTooOld);
            }

            bool rxWrapped;
            bool txWrapped;

            ulong rxOctets = CounterDelta(previous.RxOctets, current.RxOctets, out rxWrapped);
            ulong txOctets = CounterDelta(previous.TxOctets, current.TxOctets, out txWrapped);

            double seconds = elapsed.TotalSeconds;
            double rxBitsPerSecond = (double)rxOctets * BitsPerOctet / seconds;
            double txBitsPerSecond = (double)txOctets * BitsPerOctet / seconds;


            // If the result is faster than the hardware could possibly be, a restart is possible.
            if (rxBitsPerSecond > _maximumPlausibleBitsPerSecond ||
                txBitsPerSecond > _maximumPlausibleBitsPerSecond)
            {
                return Unavailable(current, BitrateStatus.CounterReset);
            }

            return new InterfaceBitrate
            {
                Description = current.Description,
                Mac = current.Mac,
                Status = BitrateStatus.Ok,
                RxBitsPerSecond = rxBitsPerSecond,
                TxBitsPerSecond = txBitsPerSecond,
                CounterWrapped = rxWrapped || txWrapped
            };
        }

        /// The number of octets that passed between two counter readings,
        /// correcting for a 32-bit counter that wrapped around.
        private static ulong CounterDelta(ulong previous, ulong current, out bool wrapped)
        {
            if (current >= previous)
            {
                wrapped = false;
                return current - previous;
            }

            // The counter went down, so it hit its ceiling and started again from zero.
            // Add back the part of the range that was left before it wrapped.
            wrapped = true;
            return (Counter32Modulus - previous) + current;
        }

        private static InterfaceBitrate Unavailable(NicSample nic, BitrateStatus status)
        {
            return new InterfaceBitrate
            {
                Description = nic.Description,
                Mac = nic.Mac,
                Status = status,
                RxBitsPerSecond = 0,
                TxBitsPerSecond = 0
            };
        }
    }
}
