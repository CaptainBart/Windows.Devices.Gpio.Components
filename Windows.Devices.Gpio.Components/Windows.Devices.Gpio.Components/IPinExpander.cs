using System;

namespace Windows.Devices.Gpio
{
    /// <summary>
    /// Pin expander interface.
    /// </summary>
    public interface IPinExpander
    {
        /// <summary>
        /// Opens a new pin.
        /// </summary>
        /// <param name="pinNumber">Number of the pin to open.</param>
        /// <returns></returns>
        IGpioPin OpenPin(int pinNumber);

        /// <summary>
        /// Gets the number of pins on the expander.
        /// </summary>
        int NumberOfPins { get; }
    }
}
