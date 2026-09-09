using BitrateCalculator.Calculation;
using BitrateCalculator.Models;
using BitrateCalculator.Presentation;
using BitrateCalculator.Responses;
using BitrateCalculator.Simulation;



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

        private const double TrafficVariation = 0.20;

        private const int NumberOfPolls = 50;

        static void Main(string[] args)
        {
            TimeSpan pollingInterval = TimeSpan.FromSeconds(1.0 / PollingRateHz);

            VideoDeviceSimulator device = DemoDevice.Create(TrafficVariation);
            
            DeviceResponseParser parser = new DeviceResponseParser();
            
            RateCalculator calculator = new RateCalculator(
                pollingInterval * StalenessFactor,
                MaximumPlausibleBitsPerSecond);
            
            ConsoleReporter reporter = new ConsoleReporter();

            reporter.ReportStart(PollingRateHz, pollingInterval);

            DeviceSnapshot? previous = null;

            for (int poll = 1; poll <= NumberOfPolls; poll++)
            {
                DeviceSnapshot current = parser.Parse(device.Poll());

                reporter.ReportPollHeader(poll, current);

                if (previous == null)
                {
                    // A bitrate is a difference between two readings, so the first
                    // poll can only be stored as a baseline.
                    reporter.ReportBaseline();
                }
                else
                {
                    reporter.ReportBitrates(calculator.Calculate(previous, current));
                }

                previous = current;

                if (poll < NumberOfPolls)
                {
                    Thread.Sleep(pollingInterval);
                }
            }
        }
    }
}