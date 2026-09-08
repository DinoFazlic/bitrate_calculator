using BitrateCalculator.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace BitrateCalculator.Api
{
    public sealed class DeviceResponseParser
    {
        public DeviceSnapshot Parse(string json)
        {
            if(string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("The JSON payload is empty", nameof(json));

            DeviceResponse? response;

            try
            {
                response = JsonSerializer.Deserialize<DeviceResponse>(json);
            }
            catch (JsonException ex)
            {
                throw new FormatException("The payload is not valid JSON", ex);
            }

            if (response is null)
                throw new FormatException("The payload deserializer is null");

            List<NicSample> nics = new List<NicSample>();

            if (response.Nics != null)
            {
                foreach(var nic in response.Nics)
                {
                    nics.Add(MapNic(nic));
                }
            }

            return new DeviceSnapshot
            {
                Device = Require(response.Device, "Device"),
                Model = Require(response.Model, "Model"),
                Nics = nics
            };
            
        }

        private static NicSample MapNic(NicResponse nic)
        {
            var mac = Require(nic.Mac, "MAC");

            return new NicSample
            {
                Description = nic.Description ?? "(unnamed interface)",
                Mac = mac,
                Timestamp = ParseTimestamp(nic.Timestamp, mac),
                RxOctets = ParseCounter(nic.Rx, "Rx", mac),
                TxOctets = ParseCounter(nic.Tx, "Tx", mac)
            };

        }

        private static string Require(string? value, string fieldName)
        {
            if(string.IsNullOrWhiteSpace(value))
                throw new FormatException($"The payload is missing a value for '{fieldName}'.");

            return value;
        }

        private static DateTimeOffset ParseTimestamp(string? value, string mac)
        {
            var text = Require(value, "Timestamp");

            DateTimeOffset timestamp;

            bool parsed = DateTimeOffset.TryParse(
                text,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out timestamp);

            if (!parsed)
            {
                throw new FormatException($"Interface {mac}: '{text}' is not a valid timestamp.");
            }

            return timestamp;
        }

        private static ulong ParseCounter(string? value, string fieldName, string mac)
        {
            var text = Require(value, fieldName);

            ulong counter;

            bool parsed = ulong.TryParse(
                text,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out counter);

            if (!parsed)
            {
                throw new FormatException(
                    $"Interface {mac}: {fieldName} value '{text}' is not a valid octet counter.");
            }

            return counter;
        }
    }
}
