using BitrateCalculator.Api;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace BitrateCalculator.Simulation
{
    public sealed class VideoDeviceSimulator
    {
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

        public string Poll()
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;

            // On the first read there is no previous
            double elapsedSeconds = 0;

            if(_hasBeenPolled)
            {
                elapsedSeconds = (now - _lastPollTime).TotalSeconds;
            }

            _lastPollTime = now;
            _hasBeenPolled = true;

            List<NicResponse> nicResponses = new List<NicResponse>();

            foreach (SimulatedNic nic in _nics)
            {
                nic.Advance(elapsedSeconds);

                NicResponse nicResponse = new NicResponse
                {
                    Description = nic.Description,
                    Mac = nic.Mac,
                    Timestamp = now.ToString("o", CultureInfo.InvariantCulture),
                    Rx = nic.RxOctets.ToString(CultureInfo.InvariantCulture),
                    Tx = nic.TxOctets.ToString(CultureInfo.InvariantCulture)
                };

                nicResponses.Add(nicResponse);
            }

            DeviceResponse response = new DeviceResponse
            {
                Device = _deviceName,
                Model = _modelName,
                Nics = nicResponses
            };

            return JsonSerializer.Serialize(response);
        }

        }
}
