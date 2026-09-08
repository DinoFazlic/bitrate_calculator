using BitrateCalculator.Api;
using BitrateCalculator.Calculation;
using BitrateCalculator.Domain;
using BitrateCalculator.Simulation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace BitrateCalculator
{
    internal class Program
    {
        private const double PollingRateHz = 2.0;

        // If polls end up more than four intervals apart, polling was interrupted
        // and the two samples are too far apart to average meaningfully.
        private const int StalenessFactor = 4;

        // No interface on this device could carry more than 10 Gbit/s, so anything
        // faster means the counters restarted rather than the traffic being real.
        private const double MaximumPlausibleBitsPerSecond = 10_000_000_000;

        private const int NumberOfPolls = 15;

        private const string NotAvailable = "n/a";

        static void Main(string[] args)
        {
            TimeSpan pollingInterval = TimeSpan.FromSeconds(1.0 / PollingRateHz);

            VideoDeviceSimulator device = CreateDevice();
            DeviceResponseParser parser = new DeviceResponseParser();

            RateCalculator calculator = new RateCalculator(
                pollingInterval * StalenessFactor,
                MaximumPlausibleBitsPerSecond);

            Console.WriteLine("Polling at " + PollingRateHz + " Hz (one poll every "
                + pollingInterval.TotalMilliseconds + " ms)");
            Console.WriteLine();

            DeviceSnapshot? previous = null;

            for (int poll = 1; poll <= NumberOfPolls; poll++)
            {
                string json = device.Poll();
                DeviceSnapshot current = parser.Parse(json);

                Console.WriteLine($"--- poll {poll}  ({current.Device} {current.Model}) ---");

                if (previous == null)
                {
                    // A bitrate is a difference between two readings, so the very first
                    // poll can only be stored as a baseline.
                    Console.WriteLine("  first poll - storing baseline, no bitrate yet");
                }
                else
                {
                    List<InterfaceBitrate> results = calculator.Calculate(previous, current);

                    foreach (InterfaceBitrate result in results)
                    {
                        PrintResult(result);
                    }
                }

                previous = current;
                Console.WriteLine();

                if (poll < NumberOfPolls)
                {
                    Thread.Sleep(pollingInterval);
                }
            }
        }

        private static VideoDeviceSimulator CreateDevice()
        {
            List<SimulatedNic> nics = new List<SimulatedNic>
            {
                new SimulatedNic(
                    description: "Linksys ABR",
                    mac: "14:91:82:3C:D6:7D",
                    rxBitsPerSecond: 1_000_000_000,
                    txBitsPerSecond: 200_000_000,
                    startingRxOctets: 3698574500,
                    startingTxOctets: 122558800)
            };

            return new VideoDeviceSimulator("Arista", "X-Video", nics);
        }

        private static void PrintResult(InterfaceBitrate result)
        {
            if (result.Status != BitrateStatus.Ok)
            {
                Console.WriteLine($"  {result.Description,-14}  Rx {NotAvailable,12}   Tx {NotAvailable,12}"
                    + $"   ({DescribeStatus(result.Status)})");
                return;
            }

            string line = $"  {result.Description,-14}"
                + $"  Rx {FormatBitrate(result.RxBitsPerSecond),12}"
                + $"   Tx {FormatBitrate(result.TxBitsPerSecond),12}";

            if (result.CounterWrapped)
            {
                line = line + "   (counter wrapped, corrected)";
            }

            Console.WriteLine(line);
        }

        /// Formats a bitrate using decimal prefixes, which is the convention for
        /// network speeds: 1 kbit/s is 1000 bit/s, not 1024.
        private static string FormatBitrate(double bitsPerSecond)
        {
            string[] units = { "bit/s", "kbit/s", "Mbit/s", "Gbit/s" };

            double value = bitsPerSecond;
            int unitIndex = 0;

            // The value is shown with two decimals, so keep stepping up while it would
            // still print as 1000 or more. Without this, 999,999,992 bit/s would be
            // shown as "1000.00 Mbit/s" instead of "1.00 Gbit/s".
            while (value >= 999.995 && unitIndex < units.Length - 1)
            {
                value = value / 1000.0;
                unitIndex++;
            }

            return value.ToString("F2", CultureInfo.InvariantCulture) + " " + units[unitIndex];
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