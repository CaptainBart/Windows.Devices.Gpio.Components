using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windows.Devices.Gpio
{
    /// <summary>
    /// Gpio Controller that manages onboard pins and pins on expander chips.
    /// </summary>
    public class GenericGpioController
    {
        private static GenericGpioController _default;

        /// <summary>
        /// Gets the default intstance.
        /// </summary>
        /// <returns></returns>
        public static GenericGpioController GetDefault()
        {

            if (_default == null)
            {
                _default = new GenericGpioController();
            }

            return _default;

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        private GenericGpioController()
        {
            _expanders = new Dictionary<int, IPinExpander>();
        }

        private Dictionary<int, IPinExpander> _expanders;

        /// <summary>
        /// Opens the pin at the specified position.
        /// </summary>
        /// <param name="pin">Pin to open.</param>
        /// <param name="sharingMode">The pin's sharing mode.</param>
        /// <returns>A new IGPioPin instance for the specified pin.</returns>
        public IGpioPin OpenPin(int pin, GpioSharingMode sharingMode = GpioSharingMode.Exclusive)
        {
            IGpioPin gpioPin = null;
            if(pin < 64)
            {
                var sysPin = GpioController.GetDefault().OpenPin(pin, sharingMode);
                gpioPin = new SysGpioPin(sysPin);
            }
            else
            {
                if(_expanders.Count > 0)
                {
                    foreach(int startPin in _expanders.Keys)
                    {
                        if(pin >= startPin && pin < startPin + _expanders[startPin].NumberOfPins)
                        {
                            gpioPin = _expanders[startPin].OpenPin(pin - startPin);
                        }
                    }
                }
            }

            if(gpioPin == null) throw new NotSupportedException();
            return gpioPin;
        }

        /// <summary>
        /// Adds a new expander to the controller
        /// </summary>
        /// <param name="expander">Expander to add.</param>
        /// <param name="startPinNumber">Virtual start pin number to add expander at.</param>
        public void RegisterExpander(int startPinNumber, IPinExpander expander)
        {
            if (expander == null) throw new ArgumentNullException("expander");
            foreach (int startPin in _expanders.Keys)
            {
                if (startPinNumber >= startPin && startPinNumber < startPin + _expanders[startPin].NumberOfPins
                    || startPin >= startPinNumber && startPin < startPinNumber + expander.NumberOfPins)
                {
                    throw new ArgumentException(String.Format("The startpin number {0} overlaps with another expander that starts on {1} and has {2} pins.", startPinNumber, startPin, _expanders[startPin].NumberOfPins), "startPinNumber");
                }
            }

            _expanders.Add(startPinNumber, expander);
        }

        public async Task RegisterMcp23008(int startPinNumber, int slaveAddress, int interruptPin = -1)
        {
            var expander = await Components.Mcp230xx.CreateMcp23008(slaveAddress, interruptPin);
            this.RegisterExpander(startPinNumber, expander);
        }

        public async Task RegisterMcp23017(int startPinNumber, int slaveAddress, int interruptPin = -1)
        {
            var expanders = await Components.Mcp230xx.CreateMcp23017(slaveAddress, interruptPin);
            this.RegisterExpander(startPinNumber, expanders[0]);
            this.RegisterExpander(startPinNumber + 8, expanders[1]);
        }
    }
}
