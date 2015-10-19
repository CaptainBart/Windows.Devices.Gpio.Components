using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windows.Devices.Gpio
{
    /// <summary>
    /// Event arguments for the GPIO value changed event.
    /// </summary>
    public class ValueChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="edge">Edge of the event that occurred.</param>
        public ValueChangedEventArgs(GpioPinEdge edge)
        {
            this.Edge = edge;
        }

        /// <summary>
        /// Gets the edge value.
        /// </summary>
        public GpioPinEdge Edge
        {
            get;
            private set;
        }
    }
}
