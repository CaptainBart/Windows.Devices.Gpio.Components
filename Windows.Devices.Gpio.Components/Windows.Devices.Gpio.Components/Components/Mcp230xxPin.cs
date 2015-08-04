using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Windows.Devices.Gpio.Components
{
    public class Mcp230xxPin
    {
        private static readonly GpioPinDriveMode[] SupportedDriveModes = { GpioPinDriveMode.Input, GpioPinDriveMode.Output };

        public event TypedEventHandler<GpioPin, GpioPinValueChangedEventArgs> ValueChanged;
        
        internal Mcp230xxPin(Mcp230xx expander, int pinNumber)
        {
            this.Expander = expander;
            this.PinNumber = pinNumber;
            this.SetDriveMode(GpioPinDriveMode.Output);
            this.Write(GpioPinValue.Low);
        }

        public int PinNumber
        {
            get;
            private set;
        }

        internal Mcp230xx Expander
        {
            get;
            private set;
        }

        private GpioPinDriveMode DriveMode
        {
            get;
            set;
        }

        public GpioSharingMode SharingMode
        {
            get { return GpioSharingMode.Exclusive; }
        }
        
        public GpioPinDriveMode GetDriveMode()
        {
            return this.DriveMode;
        }

        public bool IsDriveModeSupported(GpioPinDriveMode driveMode)
        {
            return SupportedDriveModes.Contains(driveMode);
        }

        public void SetDriveMode(GpioPinDriveMode value)
        {
            this.Expander.SetPinDirection(this.PinNumber, value);
            this.DriveMode = value;
        }

        public GpioPinValue Read()
        {
            return this.Expander.GetPinStatus(this.PinNumber);
        }

        public void Write(GpioPinValue value)
        {
            this.Expander.SetPinStatus(this.PinNumber, value);
        }
    }
}
