using BitrateCalculator.Responses;
using System.Globalization;
using System.Text.Json;

namespace BitrateCalculator.Simulation
{
    public sealed class VideoDeviceSimulator
    {
        private const string TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffffff'Z'";

        private readonly string _deviceName;
        private readonly string _modelName;
        private readonly List<SimulatedNic> _nics;

        private DateTimeOffset _lastPollTime;
        private bool _hasBeenPolled;

        public VideoDeviceSimulator(string deviceName, string modelName, List<SimulatedNic> nics)
        {
            _deviceName = deviceName;
            _modelName = modelName;
            _nics = nics;
        }


        /// Reads the device. Advances every counter by the traffic that passed since the
        /// previous read, then returns the JSON payload.
        public string Poll()
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;

            // On the very first read there is no previous read to measure from, so the
            // counters stay exactly as they were configured.
            double elapsedSeconds = 0;

            if (_hasBeenPolled)
            {
                elapsedSeconds = (now - _lastPollTime).TotalSeconds;
            }

            _lastPollTime = now;
            _hasBeenPolled = true;

            DeviceResponse response = new DeviceResponse
            {
                Device = _deviceName,
                Model = _modelName,
                Nics = BuildNicResponses(now, elapsedSeconds)
            };

            return JsonSerializer.Serialize(response);
        }

        private List<NicResponse> BuildNicResponses(DateTimeOffset now, double elapsedSeconds)
        {
            List<NicResponse> responses = new List<NicResponse>();

            foreach (SimulatedNic nic in _nics)
            {
                nic.Advance(elapsedSeconds);

                responses.Add(new NicResponse
                {
                    Description = nic.Description,
                    Mac = nic.Mac,
                    Timestamp = now.UtcDateTime.ToString(TimestampFormat, CultureInfo.InvariantCulture),
                    Rx = nic.RxOctets.ToString(CultureInfo.InvariantCulture),
                    Tx = nic.TxOctets.ToString(CultureInfo.InvariantCulture)
                });
            }

            return responses;
        }
    }
}
