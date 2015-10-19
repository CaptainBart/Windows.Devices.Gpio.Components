using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace Windows.Devices.Gpio.Components
{
    /// <summary>
    /// MCP230xx Port expander for managing 8 expander ports.
    /// </summary>
    /// <remarks>Use two instances of this class for the MCP23016 chip, one for the A registers and one for the B registers.</remarks>
    public class Mcp230xx : IPinExpander
    {
        /// <summary>
        /// Event that occurs when a interrupt is detected
        /// </summary>
        public event EventHandler<InterruptEventArgs> Interrupt;

        private I2cDevice Device
        {
            get;
            set;
        }

        private I2cDeviceRegister IODIR;        //IO direction register
        private I2cDeviceRegister IPOL;         //IO Polarity register
        private I2cDeviceRegister GPINTEN;     //Interupt on change register
        private I2cDeviceRegister DEFVAL;       //Interupt default value for comparison register
        private I2cDeviceRegister INTCON;       //Interupt control register
        private I2cDeviceRegister IOCON;        //IO configuration register
        private I2cDeviceRegister GPPU;         //Pull up register
        private I2cDeviceRegister INTF;        //Interupt flag register
        private I2cDeviceRegister INTCAP;       //Interupt capture register
        private I2cDeviceRegister GPIO;         //Pin values register
        private I2cDeviceRegister OLAT;         //Pin latches register
        
        /// <summary>
        /// Creates a mcp23008 port expander
        /// </summary>
        /// <param name="slaveAddress">i2c address of the chip.</param>
        /// <returns></returns>
        public async static Task<Mcp230xx> CreateMcp23008(int slaveAddress, int interruptPin = -1)
        {
            var device = await GetDevice(slaveAddress);
            var iPin = interruptPin == -1 ? null : GenericGpioController.GetDefault().OpenPin(interruptPin);

            var expander = new Mcp230xx(device, 0x00, 0x01, iPin);
            expander.InterruptActiveValue = GpioPinValue.High;
            return expander;
        }

        /// <summary>
        /// Creates a new mcp23017 port expander
        /// </summary>
        /// <param name="slaveAddress">i2c address of the chip.</param>
        /// <returns></returns>
        public async static Task<Mcp230xx[]> CreateMcp23017(int slaveAddress, int interruptPin = -1)
        {
            var registerAddresses = new byte[] { 0x00, 0x01 };
            var expanders = new Mcp230xx[2];
            var device = await GetDevice(slaveAddress);
            var iPin = interruptPin == -1 ? null : GenericGpioController.GetDefault().OpenPin(interruptPin);

            for (var i = 0; i < registerAddresses.Length; i++)
            {
                var expander = new Mcp230xx(device, registerAddresses[i], 0x02, iPin); 
                expander.InterruptMirrorEnabled = true;
                expander.InterruptActiveValue = GpioPinValue.Low;
                expanders[i] = expander;
            }

            return expanders;
        }
        
        private async static Task<I2cDevice> GetDevice(int slaveAddress)
        {
            string deviceSelector = I2cDevice.GetDeviceSelector();
            var deviceControllers = await DeviceInformation.FindAllAsync(deviceSelector);

            if (deviceControllers.Count == 0) throw new InvalidOperationException("No i2c controller found.");
            var i2cSettings = new I2cConnectionSettings(0x20);
            i2cSettings.BusSpeed = I2cBusSpeed.FastMode;
            var device = await I2cDevice.FromIdAsync(deviceControllers[0].Id, i2cSettings);
            return device;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="device">Reference to the i2c device.</param>
        /// <param name="baseRegisterAddress">Address of the first (IODIR) register.</param>
        /// <param name="registerIncrement">Increment to get the next register.</param>
        private Mcp230xx(I2cDevice device, byte baseRegisterAddress, byte registerIncrement, IGpioPin interruptPin)
        {
            this.Device = device;
            this.IODIR = new I2cDeviceRegister(device, baseRegisterAddress);
            baseRegisterAddress += registerIncrement;

            this.IPOL = new I2cDeviceRegister(device, baseRegisterAddress);
            baseRegisterAddress += registerIncrement;

            this.GPINTEN= new I2cDeviceRegister(device, baseRegisterAddress);
            baseRegisterAddress += registerIncrement;

            this.DEFVAL = new I2cDeviceRegister(device, baseRegisterAddress);
            baseRegisterAddress += registerIncrement;

            this.INTCON = new I2cDeviceRegister(device, baseRegisterAddress);
            baseRegisterAddress += registerIncrement;

            this.IOCON = new I2cDeviceRegister(device, baseRegisterAddress);
            baseRegisterAddress += registerIncrement;

            this.GPPU = new I2cDeviceRegister(device, baseRegisterAddress);
            baseRegisterAddress += registerIncrement;

            this.INTF = new I2cDeviceRegister(device, baseRegisterAddress);
            baseRegisterAddress += registerIncrement;

            this.INTCAP = new I2cDeviceRegister(device, baseRegisterAddress);
            baseRegisterAddress += registerIncrement;

            this.GPIO = new I2cDeviceRegister(device, baseRegisterAddress);
            baseRegisterAddress += registerIncrement;

            this.OLAT = new I2cDeviceRegister(device, baseRegisterAddress);
           
            this.IODIR.Write(0x00);     //all pins output
            this.GPIO.Write(0x00);      //all pins low
            this.OLAT.Write(0x00);      //all latches low
            this.INTCON.Write(0x00);    //compare interrupt with previous value
            this.GPINTEN.Write(0x00);   //no interrupts registered

            if(interruptPin != null)
            {
                this.InterruptPin = interruptPin;
                var driveMode = this.InterruptActiveValue == GpioPinValue.Low ? GpioPinDriveMode.InputPullUp : GpioPinDriveMode.InputPullDown;
                this.InterruptPin.SetDriveMode(driveMode);
                this.InterruptPin.ValueChanged += InterruptPin_ValueChanged;

            }
        }

        /// <summary>
        /// Gets the number of pins on the expander.
        /// </summary>
        public int NumberOfPins { get { return 8; } }

        /// <summary>
        /// Gets or sets the interrupt pin. This pin will be listened to for interrupt events.
        /// </summary>
        private IGpioPin InterruptPin
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets or sets whether interrupt mirroring is enabled.
        /// </summary>
        internal bool InterruptMirrorEnabled
        {
            get { return this.IOCON.Read(6); }
            set { this.IOCON.Write(6, value); }
        }
   
        /// <summary>
        /// Gets or sets whether the Interrupt pin will be high or low when a interrupt occurs.
        /// </summary>
        internal GpioPinValue InterruptActiveValue
        {
            get { return this.IOCON.Read(1) ? GpioPinValue.High : GpioPinValue.Low; }
            set
            {
                this.IOCON.Write(1, value == GpioPinValue.High);
                if (this.InterruptPin != null)
                {
                    this.InterruptPin.SetDriveMode(value == GpioPinValue.Low ? GpioPinDriveMode.InputPullUp : GpioPinDriveMode.InputPullDown);
                }
            }
        }

        /// <summary>
        /// Creates a new pin.
        /// </summary>
        /// <param name="pinNumber">Number of the pin.</param>
        /// <returns>A new pin on the port expander.</returns>
        public IGpioPin OpenPin(int pinNumber)
        {
            return new Mcp230xxPin(this, pinNumber);
        }
        
        /// <summary>
        /// Sets the pin direction.
        /// </summary>
        /// <param name="pin">Pin to set direction for.</param>
        /// <param name="driveMode">The pin direction</param>
        internal void SetDriveMode(int pin, GpioPinDriveMode driveMode)
        {
            if (driveMode == GpioPinDriveMode.InputPullDown) throw new ArgumentException("The MCP chip only support pull up resistors", "direction");
            var isInput = driveMode == GpioPinDriveMode.Input || driveMode == GpioPinDriveMode.InputPullUp;
            this.IODIR.Write(pin, isInput);

            var pullUpEnabled = driveMode == GpioPinDriveMode.InputPullUp;
            this.EnablePullUpResistor(pin, pullUpEnabled);
            
            if(isInput)
            {
                this.SetInterrupt(pin, true);
            }
        }

        /// <summary>
        /// Sets the pin's pull up resistor for.
        /// </summary>
        /// <param name="pin">Pin to set resistor for.</param>
        /// <param name="enabled">True to enable the resistor, false otherwise.</param>
        private void EnablePullUpResistor(int pin, bool enabled)
        {
            this.GPPU.Write(pin, enabled);
        }
        
        /// <summary>
        /// Sets the pin for interrupts.
        /// </summary>
        /// <param name="pin">Pin to set interrupt for.</param>
        /// <param name="enabled">True to capture interrupts, false otherwise.</param>
        private void SetInterrupt(int pin, bool enabled)
        {
            this.GPINTEN.Write(pin, enabled);
        }

        /// <summary>
        /// Sets the pin's value.
        /// </summary>
        /// <param name="pin">Pin to set value for.</param>
        /// <param name="value">Value to set.</param>
        internal void SetPinStatus(int pin, GpioPinValue value)
        {
            //TODO: use OLAT register instead?
            this.GPIO.Write(pin, value == GpioPinValue.High);
        }

        /// <summary>
        /// Gets the pin's value.
        /// </summary>
        /// <param name="pin">Pin to get value for.</param>
        /// <returns>The pin's value.</returns>
        internal GpioPinValue GetPinStatus(int pin)
        {
            return this.GPIO.Read(pin) ? GpioPinValue.High : GpioPinValue.Low;
        }

        /// <summary>
        /// Handles the valuechanged event of the interrupt pin.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void InterruptPin_ValueChanged(IGpioPin sender, ValueChangedEventArgs args)
        {
            if (this.GPINTEN.Read() == 0x00) return;            //not listening to interrupts

            var activeValue = this.InterruptActiveValue;
            var edge = activeValue.Edge();

            if (args.Edge == edge)
            {
                var interruptFlags = this.INTF.Read();
                if (interruptFlags == 0x00) return;

                var interruptValues = this.INTCAP.Read();
                var currentValues = this.GPIO.Read();

                //todo raise event that can be captured by registered pins.
                this.OnInterrupt(this, new InterruptEventArgs(interruptFlags, interruptValues, currentValues));
            }
        }

        /// <summary>
        /// Raises the interrupt event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnInterrupt(object sender, InterruptEventArgs e)
        {
            if(this.Interrupt != null)
            {
                this.Interrupt(sender, e);
            }
        }
    }
}
