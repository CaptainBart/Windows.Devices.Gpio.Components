using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windows.Devices.Gpio.Components
{
    /// <summary>
    /// L293 Half H Driver.
    /// </summary>
    public class L293d
    {
        private SoftPwmDriver _enablePin;
        private IGpioPin _in1Pin;
        private IGpioPin _in2Pin;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="enablePin">Enablepin, a soft pwm driver will be connected to the pin.</param>
        /// <param name="in1Pin">In1 pin.</param>
        /// <param name="in2Pin">In2 pin.</param>
        public L293d(int enablePin, int in1Pin, int in2Pin)
        {
            _enablePin = new SoftPwmDriver(enablePin);
            _in1Pin = GenericGpioController.GetDefault().OpenPin(in1Pin);
            _in1Pin.SetDriveMode(GpioPinDriveMode.Output);
            _in2Pin = GenericGpioController.GetDefault().OpenPin(in2Pin);
            _in2Pin.SetDriveMode(GpioPinDriveMode.Output);

            _in1Pin.Write(GpioPinValue.Low);
            _in2Pin.Write(GpioPinValue.Low);
        }

        private MotorDirection? _direction;

        /// <summary>
        /// Gets or sets the direction. Set null to stop.
        /// </summary>
        public MotorDirection? Direction
        {
            get { return _direction; }
            set
            {
                if(_direction != value)
                {
                    _direction = value;

                    if (_direction.HasValue)
                    {
                        if(_direction.Value == MotorDirection.Forward)
                        {
                            _in1Pin.Write(GpioPinValue.High);
                            _in2Pin.Write(GpioPinValue.Low);
                        }
                        else
                        {
                            _in1Pin.Write(GpioPinValue.Low);
                            _in2Pin.Write(GpioPinValue.High);
                        }
                    }
                    else
                    {
                        _in1Pin.Write(GpioPinValue.Low);
                        _in2Pin.Write(GpioPinValue.Low);
                    }
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the speed.
        /// </summary>
        public int Speed
        {
            get { return _enablePin.Value; }
            set
            {
                _enablePin.Value = value;
                if(value == 0)
                {
                    _enablePin.Disable();
                }
                else
                {
                    _enablePin.Enable();
                }
            }

        }
    }
}
