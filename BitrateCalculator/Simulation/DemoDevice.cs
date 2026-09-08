

namespace BitrateCalculator.Simulation
{
    public static class DemoDevice
    {
        public static VideoDeviceSimulator Create(double trafficVariation)
        {
            List<SimulatedNic> nics = new List<SimulatedNic>
            {
                new SimulatedNic(
                    description: "Linksys ABR",
                    mac: "14:91:82:3C:D6:7D",
                    rxBitsPerSecond: 1_000_000_000,
                    txBitsPerSecond: 200_000_000,
                    startingRxOctets: 3698574500,
                    startingTxOctets: 122558800,
                    variation: trafficVariation)
            };

            return new VideoDeviceSimulator("Arista", "X-Video", nics);
        }
    }
}
