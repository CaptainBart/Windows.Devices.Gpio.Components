using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windows.Devices.Gpio.Components
{
    /// <summary>
    /// A Led.
    /// </summary>
    public class Led
    {
        private GpioPin _pin;
        private GpioPinValue _value = GpioPinValue.Low;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="pin">Pin the led is connected on.</param>
        public Led(int pin)
        {
            _pin = GpioController.GetDefault().OpenPin(pin);
            _pin.SetDriveMode(GpioPinDriveMode.Output);
        }
        
        /// <summary>
        /// Gets or sets the pin value.
        /// </summary>
        public GpioPinValue Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                _pin.Write(_value);
            }
        }

        /// <summary>
        /// Turns the led on.
        /// </summary>
        public void On()
        {
            this.Value = GpioPinValue.High;
        }

        /// <summary>
        /// Turns the led off.
        /// </summary>
        public void Off()
        {
            this.Value = GpioPinValue.Low;
        }

        /// <summary>
        /// Toggles the led.
        /// </summary>
        public void Toggle()
        {
            var newValue = this.Value.Toggle();
            this.Value = newValue;
        }
    }
}
