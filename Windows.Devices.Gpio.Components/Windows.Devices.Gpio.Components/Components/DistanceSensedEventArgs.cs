using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windows.Devices.Gpio.Components
{
    /// <summary>
    /// Event arguments for the distance sensed event.
    /// </summary>
    public class DistanceSensedEventArgs : EventArgs
    {
        private const decimal SpeedOfSound = 340 / 1000m;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="duration">Duration of the echo signal</param>
        public DistanceSensedEventArgs(TimeSpan duration)
        {
            this.Duration = duration;
            this.Distance = (decimal)this.Duration.TotalMilliseconds * SpeedOfSound;
        }

        /// <summary>
        /// Gets the duration of the echo signal
        /// </summary>
        public TimeSpan Duration
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the distance of the object.
        /// </summary>
        public decimal Distance
        {
            get;
            private set;
        }
    }
}
