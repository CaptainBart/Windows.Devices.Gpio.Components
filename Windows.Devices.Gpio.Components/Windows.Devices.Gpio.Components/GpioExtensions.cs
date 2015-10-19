using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windows.Devices.Gpio
{
    /// <summary>
    /// General extensions the the GPIO classes.
    /// </summary>
    public static class GPioExtensions
    {
        /// <summary>
        /// Toggles the provided pin value.
        /// </summary>
        /// <param name="cur">The current value</param>
        /// <returns>The new value</returns>
        public static GpioPinValue Toggle(this GpioPinValue cur)
        {
            return cur == GpioPinValue.High ? GpioPinValue.Low : GpioPinValue.High;
        }

        /// <summary>
        /// Waits until the value of the pin changes to the requested value.
        /// </summary>
        /// <param name="pin">Pin to check the value of.</param>
        /// <param name="value">The value to wait for.</param>
        /// <param name="timeout">How long to wait.</param>
        /// <remarks>This can be quite expensive. Better approach would be to use the pin's events.</remarks>
        public static void WaitFor(this GpioPin pin, GpioPinValue value, int timeout = 0)
        {
            timeout = timeout == 0 ? 4000 : timeout;
            var endTicks = DateTime.UtcNow.Ticks + (timeout * TimeSpan.TicksPerMillisecond);
            
            while (pin.Read() != value)
            {
                if (DateTime.UtcNow.Ticks > endTicks)
                {
                    throw new TimeoutException("WaitFor timed out.");
                }
            }
        }


        /// <summary>
        /// Gets the matching edge to detect.
        /// </summary>
        /// <param name="value">The value to get the matching edge for.</param>
        /// <returns>The matchign edge of the value.</returns>
        public static GpioPinEdge Edge(this GpioPinValue value)
        {
            if (value == GpioPinValue.High) return GpioPinEdge.RisingEdge;
            if (value == GpioPinValue.Low) return GpioPinEdge.FallingEdge;
            throw new NotSupportedException();
        }
    }
}
