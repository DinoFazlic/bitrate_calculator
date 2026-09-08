using System;
using System.Collections.Generic;
using System.Text;

namespace BitrateCalculator.Domain
{
    public enum BitrateStatus
    {
        /// The bitrate was calculated successfully.
        Ok,

        /// This interface was not in the previous poll, so there is nothing to compare against.
        NoPreviousSample,

        /// The two timestamps are equal or out of order, so no rate can be calculated.
        InvalidElapsedTime,

        /// Too much time passed between the two samples for the result to be trustworthy.
        SampleTooOld,

        /// The counters jumped implausibly — the device most likely restarted.
        CounterReset
    }
}
