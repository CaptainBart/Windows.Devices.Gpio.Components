using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windows.Devices.Gpio.Components
{
    /// <summary>
    /// Events args for the interrupt event.
    /// </summary>
    public class InterruptEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="interruptFlags">Value of the INTF register.</param>
        /// <param name="interruptValues">Value of the INTCAP register.</param>
        /// <param name="currentValues">Value of the GPIO register.</param>
        public InterruptEventArgs(byte interruptFlags, byte interruptValues, byte currentValues)
        {
            this.InterruptFlags = interruptFlags;
            this.InterruptValues = interruptValues;
            this.CurrentValues = currentValues;
        }

        /// <summary>
        /// Gets the value of the INTF register.
        /// </summary>
        public byte InterruptFlags
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the value of the INTCAP register.
        /// </summary>
        public byte InterruptValues
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the value of the GPIO register.
        /// </summary>
        public byte CurrentValues
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the matching values for the provided pin.
        /// </summary>
        /// <param name="pin">number of the pin.</param>
        /// <returns></returns>
        public PinInterruptValue ValuesForPin(int pin)
        {
            if (pin < 0 || pin >= 8) throw new ArgumentException("", "pin");
            var mask = (byte)Math.Pow(2, pin);

            PinInterruptValue value = null;
            var interruptOccurred = ((this.InterruptFlags & mask) != 0x00);
            var edge = ((this.InterruptValues & mask) != 0x00) ? GpioPinEdge.RisingEdge: GpioPinEdge.FallingEdge;
            var curValue = ((this.CurrentValues & mask) != 0x00) ? GpioPinValue.High : GpioPinValue.Low;

            value = new PinInterruptValue(pin, interruptOccurred, edge, curValue);
            return value;
        }

        /// <summary>
        /// values wrapper for the interrupt for one pin.
        /// </summary>
        public class PinInterruptValue
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="pin">oin the value is for.</param>
            /// <param name="interruptOccurred">True when interrupt occurs, false otherwise.</param>
            /// <param name="edge">Edge that occurred</param>
            /// <param name="currentValue">Current value.</param>
            internal PinInterruptValue(int pin, bool interruptOccurred, GpioPinEdge edge, GpioPinValue currentValue)
            {
                this.Pin = pin;
                this.InterruptOccurred = interruptOccurred;
                this.Edge = edge;
                this.CurrentValue = currentValue;
            }

            /// <summary>
            /// Gets the pin the value is for.
            /// </summary>
            public int Pin { get; private set; }

            /// <summary>
            /// Gets whether an interrupt occurred for this pin.
            /// </summary>
            public bool InterruptOccurred { get; private set; }

            /// <summary>
            /// Gets the value at the time of the interrupt.
            /// </summary>
            public GpioPinEdge Edge{ get; private set; }

            /// <summary>
            /// Gets the current value.
            /// </summary>
            public GpioPinValue CurrentValue { get; private set; }
        }
    }
}
