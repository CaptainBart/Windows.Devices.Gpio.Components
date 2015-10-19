using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Windows.Devices.Gpio
{
    /// <summary>
    /// wrapper class for the onboard gpio pins
    /// </summary>
    public class SysGpioPin : IGpioPin
    {
        public SysGpioPin(GpioPin pin)
        {
            if (pin == null) throw new ArgumentNullException(nameof(pin));
            this.Pin = pin;
            this.Pin.ValueChanged += Pin_ValueChanged;
        }
        
        public GpioPin Pin
        {
            get;
            private set;
        }

        public TimeSpan DebounceTimeout
        {
            get { return this.Pin.DebounceTimeout; }
            set { this.Pin.DebounceTimeout = value; }
        }
        
        public int PinNumber
        {
            get { return this.Pin.PinNumber; }
        }

        public GpioSharingMode SharingMode
        {
            get { return this.Pin.SharingMode; }
        }

        public event TypedEventHandler<IGpioPin, ValueChangedEventArgs> ValueChanged;
        
        public void Dispose()
        {
            this.Pin.Dispose();
        }

        public GpioPinDriveMode GetDriveMode()
        {
            return this.Pin.GetDriveMode();
        }

        public bool IsDriveModeSupported(GpioPinDriveMode driveMode)
        {
            return this.Pin.IsDriveModeSupported(driveMode);
        }

        public GpioPinValue Read()
        {
            return this.Pin.Read();
        }

        public void SetDriveMode(GpioPinDriveMode value)
        {
            this.Pin.SetDriveMode(value);
        }

        public void Write(GpioPinValue value)
        {
            this.Pin.Write(value);
        }

        private void Pin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            this.OnValueChanged(this, new ValueChangedEventArgs(args.Edge));
        }

        protected virtual void OnValueChanged(IGpioPin sender, ValueChangedEventArgs args)
        {
            if(this.ValueChanged != null)
            {
                this.ValueChanged(sender, args);
            }
        }
    }
}
