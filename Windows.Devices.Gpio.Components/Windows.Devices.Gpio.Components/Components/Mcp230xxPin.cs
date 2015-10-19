using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Windows.Devices.Gpio.Components
{
    /// <summary>
    /// GPIO Pin on a mcp230xx chip.
    /// </summary>
    internal sealed class Mcp230xxPin : IGpioPin
    {
        private static readonly GpioPinDriveMode[] SupportedDriveModes = { GpioPinDriveMode.Input, GpioPinDriveMode.InputPullUp, GpioPinDriveMode.Output };

        /// <summary>
        /// Event that occurs when the pin value changes.
        /// </summary>
        public event TypedEventHandler<IGpioPin, ValueChangedEventArgs> ValueChanged;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="expander">MCP chip that 0wns this pin.</param>
        /// <param name="pinNumber">Number of the pin.</param>
        internal Mcp230xxPin(Mcp230xx expander, int pinNumber)
        {
            this.Expander = expander;
            this.PinNumber = pinNumber;
            this.SetDriveMode(GpioPinDriveMode.Output);
            this.Write(GpioPinValue.Low);

            this.Expander.Interrupt += Expander_Interrupt;
        }
        
        /// <summary>
        /// Gets the pinnumber
        /// </summary>
        public int PinNumber
        {
            get;
            private set;
        }

        /// <summary>
        /// MCP chip that 0wns this pin.
        /// </summary>
        internal Mcp230xx Expander
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the pin's drive mode.
        /// </summary>
        private GpioPinDriveMode DriveMode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the pin's sharing mode.
        /// </summary>
        public GpioSharingMode SharingMode
        {
            get { return GpioSharingMode.Exclusive; }
        }
        
        /// <summary>
        /// Gets the pin's drive mode.
        /// </summary>
        /// <returns></returns>
        public GpioPinDriveMode GetDriveMode()
        {
            return this.DriveMode;
        }

        /// <summary>
        /// Checks if the provided drive mode is supported.
        /// </summary>
        /// <param name="driveMode">True when supported, false otherwise.</param>
        /// <returns></returns>
        public bool IsDriveModeSupported(GpioPinDriveMode driveMode)
        {
            return SupportedDriveModes.Contains(driveMode);
        }

        /// <summary>
        /// Sets the pin's drive mode.
        /// </summary>
        /// <param name="value">Mode to set.</param>
        public void SetDriveMode(GpioPinDriveMode value)
        {
            if (!this.IsDriveModeSupported(value)) throw new ArgumentException(String.Format("The drive mode {0} is not supported.", value), "value");
            this.Expander.SetDriveMode(this.PinNumber, value);
            this.DriveMode = value;
        }

        /// <summary>
        /// Reads the pin's value.
        /// </summary>
        /// <returns></returns>
        public GpioPinValue Read()
        {
            return this.Expander.GetPinStatus(this.PinNumber);
        }

        /// <summary>
        /// Writes the pin's value.
        /// </summary>
        /// <param name="value"></param>
        public void Write(GpioPinValue value)
        {
            this.Expander.SetPinStatus(this.PinNumber, value);
        }

        /// <summary>
        /// Handles the MCP expander interrupt event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Expander_Interrupt(object sender, InterruptEventArgs e)
        {
            var interrupt = e.ValuesForPin(this.PinNumber);
            if(interrupt.InterruptOccurred)
            {
                this.OnValueChanged(this, new ValueChangedEventArgs(interrupt.Edge));
            }
        }

        /// <summary>
        /// Raises the value changed event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnValueChanged(IGpioPin sender, ValueChangedEventArgs e)
        {
            if (this.ValueChanged != null)
            {
                this.ValueChanged(sender, e);
            }
        }

        /// <summary>
        /// Disposes and actually does nothing.
        /// </summary>
        public void Dispose()
        { }
    }
}
