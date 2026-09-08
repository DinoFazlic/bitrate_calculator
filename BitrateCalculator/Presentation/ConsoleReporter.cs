using BitrateCalculator.Models;
using System.Globalization;

namespace BitrateCalculator.Presentation
{
    public sealed class ConsoleReporter
    {
        private const string NotAvailable = "n/a";

        private static readonly string[] Units = { "bit/s", "kbit/s", "Mbit/s", "Gbit/s" };

        public void ReportStart(double pollingRateHz, TimeSpan pollingInterval)
        {
            Console.WriteLine($"Polling at {pollingRateHz} Hz "
                + $"(one poll every {pollingInterval.TotalMilliseconds:F0} ms)");
            Console.WriteLine();
        }

        public void ReportPollHeader(int pollNumber, DeviceSnapshot snapshot)
        {
            Console.WriteLine($"--- poll {pollNumber}  ({snapshot.Device} {snapshot.Model}) ---");
        }

        public void ReportBaseline()
        {
            Console.WriteLine("  first poll - storing baseline, no bitrate yet");
            Console.WriteLine();
        }

        public void ReportBitrates(List<InterfaceBitrate> results)
        {
            foreach (InterfaceBitrate result in results)
            {
                ReportOne(result);
            }

            Console.WriteLine();
        }

        private static void ReportOne(InterfaceBitrate result)
        {
            if (result.Status != BitrateStatus.Ok)
            {
                Console.WriteLine($"  {result.Description,-14}"
                    + $"  Rx {NotAvailable,13}   Tx {NotAvailable,13}"
                    + $"   ({DescribeStatus(result.Status)})");
                return;
            }

            string line = $"  {result.Description,-14}"
                + $"  Rx {FormatBitrate(result.RxBitsPerSecond),13}"
                + $"   Tx {FormatBitrate(result.TxBitsPerSecond),13}";

            if (result.CounterWrapped)
            {
                line = line + "   (counter wrapped, corrected)";
            }

            Console.WriteLine(line);
        }


        /// Formats a bitrate using decimal prefixes, which is the convention for
        /// network speeds: 1 kbit/s is 1000 bit/s.
        private static string FormatBitrate(double bitsPerSecond)
        {
            double value = bitsPerSecond;
            int unitIndex = 0;

            while (value >= 999.995 && unitIndex < Units.Length - 1)
            {
                value = value / 1000.0;
                unitIndex++;
            }

            return value.ToString("F2", CultureInfo.InvariantCulture) + " " + Units[unitIndex];
        }

        private static string DescribeStatus(BitrateStatus status)
        {
            switch (status)
            {
                case BitrateStatus.NoPreviousSample:
                    return "new interface, no previous sample";

                case BitrateStatus.InvalidElapsedTime:
                    return "timestamps did not advance";

                case BitrateStatus.SampleTooOld:
                    return "gap between polls too large";

                case BitrateStatus.CounterReset:
                    return "counter discontinuity, re-baselining";

                default:
                    return status.ToString();
            }
        }
    }
}
