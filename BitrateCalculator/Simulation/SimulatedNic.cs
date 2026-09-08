using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace BitrateCalculator.Simulation
{
    public sealed class SimulatedNic
    {
        private const ulong Counter32Modulus = 4294967296;
        public string Description { get; }

        public string Mac { get; }

        public double RxBitsPerSecond { get; }

        public double TxBitsPerSecond { get; }

        public ulong RxOctets { get; private set; }

        public ulong TxOctets { get; private set; }
        public SimulatedNic(
            string description,
            string mac,
            double rxBitsPerSecond,
            double txBitsPerSecond,
            ulong startingRxOctets,
            ulong startingTxOctets)
        {
            Description = description;
            Mac = mac;
            RxBitsPerSecond = rxBitsPerSecond;
            TxBitsPerSecond = txBitsPerSecond;
            RxOctets = startingRxOctets;
            TxOctets = startingTxOctets;
        }

        public void Advance(double elapsedSeconds)
        {
            RxOctets = AddOctets(RxOctets, RxBitsPerSecond, elapsedSeconds);
            TxOctets = AddOctets(TxOctets, TxBitsPerSecond, elapsedSeconds);
        }

        private static ulong AddOctets(ulong current, double bitsPerSecond, double elapsedSeconds)
        {
            double octets = bitsPerSecond * elapsedSeconds / 8.0;
            ulong octetsToAdd = (ulong)octets;

            return (current + octetsToAdd) % Counter32Modulus;
        }

    }
}
