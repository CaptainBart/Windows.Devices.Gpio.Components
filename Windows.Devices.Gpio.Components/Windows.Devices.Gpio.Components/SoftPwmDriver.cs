using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windows.Devices.Gpio
{
    /// <summary>
    /// Soft Pwm Driver. This will cost ya CPU cycles!
    /// </summary>
    /// <remarks>
    /// A poor man's software based PWM signal driver. In my test it takes 15% - 25% CPU time per running driver instance.
    /// Inspired by: https://projects.drogon.net/raspberry-pi/wiringpi/software-pwm-library/
    /// </remarks>
    public class SoftPwmDriver
    {
        /// <summary>
        /// This is the minimal width of the PWM Pulse.
        /// </summary>
        public const double MinimalPulseWidth = 0.1d;

        private GpioPin _pin;
        private int _value;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="pin">pin to drive.</param>
        /// <param name="range">Range of the value, default 100, which means the value can be set between 0 and 100</param>
        /// <remarks>With a range of 100 the frequency will be 100Hz.
        /// Lower the range to get a higher frequency, but with a lower resolution.
        /// Increase the range to get a higher resolution, but a lower frequency.
        /// </remarks>
        public SoftPwmDriver(int pin, int range = 100)
        {
            _pin = GpioController.GetDefault().OpenPin(pin);
            _pin.SetDriveMode(GpioPinDriveMode.Output);

            if (range < 1) throw new ArgumentOutOfRangeException("range", range, "The range should be a positive value.");
            this.Range = range;
            this.IsEnabled = false;
        }

        /// <summary>
        /// Gets the range.
        /// </summary>
        public int Range
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets whether the signal on the pin is enabled.
        /// </summary>
        public bool IsEnabled
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the value.
        /// <remarks>the value must be within the range.</remarks>
        /// </summary>
        public int Value
        {
            get { return _value; }
            set
            {
                if (_value != value)
                {
                    if (value < 0 || value > this.Range) throw new ArgumentOutOfRangeException("value", value, String.Format(CultureInfo.InvariantCulture, "The value should be between 0 and {0}.", this.Range));
                    _value = value;

                    
                }
            }
        }

        /// <summary>
        /// Enables the Pwm Signal
        /// </summary>
        public void Enable()
        {
            if (!this.IsEnabled)
            {
                this.IsEnabled = true;
                this.EmulatePwm();
            }
        }

        /// <summary>
        /// Disables the Pwn Signal
        /// </summary>
        public void Disable()
        {
            this.IsEnabled = false;
        }

        private void EmulatePwm()
        {
            _pin.Write(GpioPinValue.Low);
            HighResolutionTimer timer = new HighResolutionTimer();

            Task.Run(() =>
            {
                while (this.IsEnabled)
                {
                    var space = this.Range - this.Value;

                    if (this.Value > 0)
                    {
                        _pin.Write(GpioPinValue.High);
                        timer.Sleep(this.Value * SoftPwmDriver.MinimalPulseWidth);
                    }

                    if (space > 0)
                    {
                        _pin.Write(GpioPinValue.Low);
                        timer.Sleep(space * SoftPwmDriver.MinimalPulseWidth);
                    }
                }
            });
        }

    }
}
