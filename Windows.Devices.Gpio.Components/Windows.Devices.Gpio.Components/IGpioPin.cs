using System;
using Windows.Foundation;

namespace Windows.Devices.Gpio
{
    /// <summary>
    /// Interface for the GPIO Pin wrapper
    /// </summary>
    public interface IGpioPin : IDisposable
    {
        /// <summary>
        /// Event that occurs when the pin value changes.
        /// </summary>
        event TypedEventHandler<IGpioPin, ValueChangedEventArgs> ValueChanged;

        /// <summary>
        /// Gets the pin number
        /// </summary>
        int PinNumber { get; }

        /// <summary>
        /// Gets the sharing mode
        /// </summary>
        GpioSharingMode SharingMode { get; }

        /// <summary>
        /// Gets the drive mode.
        /// </summary>
        /// <returns>The pin's drive mode.</returns>
        GpioPinDriveMode GetDriveMode();

        /// <summary>
        /// Checks if the provided drive mode is supported.
        /// </summary>
        /// <param name="driveMode">Drive mode to check.</param>
        /// <returns>True when supported, false otherwise.</returns>
        bool IsDriveModeSupported(GpioPinDriveMode driveMode);

        /// <summary>
        /// Sets the pin's drive mode.
        /// </summary>
        /// <param name="value">Drive mode to set.</param>
        void SetDriveMode(GpioPinDriveMode value);

        /// <summary>
        /// Read the pin value.
        /// </summary>
        /// <returns>The pin's value.</returns>
        GpioPinValue Read();

        /// <summary>
        /// Writes the provided value to the pin.
        /// </summary>
        /// <param name="value">Value to write to the pin.</param>
        void Write(GpioPinValue value);
    }
}
