using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace Windows.Devices.Gpio.Components
{
    public class Mcp230xx
    {
        private static class Register
        {
            internal static byte IODIRA = 0x00;
            internal static byte IODIRB = 0x01;
            internal static byte IPOLA = 0x02;
            internal static byte IPOLB = 0x03;
            internal static byte GPPUA = 0x0c;
            internal static byte GPPUB = 0x0d;
            internal static byte GPIOA = 0x12;
            internal static byte GPIOB = 0x13;
        }

        private static readonly int[] PinAddresses = { 0x0001,0x0002, 0x0004, 0x0008, 0x0010,0x0020,0x0040,0x0080, 0x0101, 0x0102, 0x0104, 0x0108, 0x0110, 0x0120, 0x0140, 0x0180 };

        //private const byte PORT_EXPANDER_IODIR_REGISTER_ADDRESS = 0x00; // IODIR register controls the direction of the GPIO on the port expander
        //private const byte PORT_EXPANDER_GPIO_REGISTER_ADDRESS = 0x09; // GPIO register is used to read the pins input
        //private const byte PORT_EXPANDER_OLAT_REGISTER_ADDRESS = 0x0A; // Output Latch register is used to set the pins output high/low

        public async static Task<Mcp230xx> Create(int slaveAddress)
        {
            string deviceSelector = I2cDevice.GetDeviceSelector();
            var deviceControllers = await DeviceInformation.FindAllAsync(deviceSelector);

            if (deviceControllers.Count == 0) throw new InvalidOperationException("No i2c controller found.");
            var i2cSettings = new I2cConnectionSettings(0x20);
            i2cSettings.BusSpeed = I2cBusSpeed.FastMode;
            var device = await I2cDevice.FromIdAsync(deviceControllers[0].Id, i2cSettings);

            return new Mcp230xx(device);
        }

        private Mcp230xx(I2cDevice device)
        {
            this.Device = device;
        }

        public Mcp230xxPin OpenPin(int pinNumber)
        {
            return new Mcp230xxPin(this, pinNumber);
        }


        internal void SetPinDirection(int pin, GpioPinDriveMode direction)
        {
            var pinAddress = PinAddresses[pin];
            var register = (pinAddress & 0x0100) == 0x0000 ? Register.IODIRA : Register.IODIRB;
            var bit = (byte)(pinAddress & 0xFF);

            var buff = new byte[1];
            this.Device.WriteRead(new byte[] { register }, buff);
            var directions = buff[0];
            
            var newDirections = (byte)((direction == GpioPinDriveMode.Input)
                                    ? directions | bit
                                    : directions & ~bit);

            this.Device.Write(new byte[] { register, newDirections });
        }

        internal void SetPinStatus(int pin, GpioPinValue value)
        {
            var pinAddress = PinAddresses[pin];
            var register = (pinAddress & 0x0100) == 0x0000 ? Register.GPIOA : Register.GPIOB;
            var bit = (byte)(pinAddress & 0xFF);
            

            var buff = new byte[1];
            this.Device.WriteRead(new byte[] { register }, buff);
            var status = buff[0];

            var newStatus = (byte)((value== GpioPinValue.High)
                                    ? status | bit
                                    : status & ~bit);

            this.Device.Write(new byte[] { register, newStatus });
        }

        internal GpioPinValue GetPinStatus(int pin)
        {
            var pinAddress = PinAddresses[pin];
            var register = (pinAddress & 0x0100) == 0x0000 ? Register.GPIOA : Register.GPIOB;
            var bit = (byte)(pinAddress & 0xFF);

            var buff = new byte[1];
            this.Device.WriteRead(new byte[] { register }, buff);
            var status = buff[0];

            return (status & bit) != 0x00 ? GpioPinValue.High : GpioPinValue.Low;
        }

        //public void DoeKunstje()
        //{
        //    try
        //    {
        //        const int LED_GPIO_PIN = 0xf | 0x8;

        //        // initialize local copies of the IODIR, GPIO, and OLAT registers
        //        var i2CReadBuffer = new byte[1];

        //        // read in each register value on register at a time (could do this all at once but
        //        // for example clarity purposes we do it this way)
        //        this.Device.WriteRead(new byte[] { PORT_EXPANDER_IODIR_REGISTER_ADDRESS }, i2CReadBuffer);
        //        var iodirRegister = i2CReadBuffer[0];

        //        this.Device.WriteRead(new byte[] { PORT_EXPANDER_GPIO_REGISTER_ADDRESS }, i2CReadBuffer);
        //        var gpioRegister = i2CReadBuffer[0];

        //        this.Device.WriteRead(new byte[] { PORT_EXPANDER_OLAT_REGISTER_ADDRESS }, i2CReadBuffer);
        //        var olatRegister = i2CReadBuffer[0];

        //        // configure the LED pin output to be logic high, leave the other pins as they are.
        //        olatRegister |= LED_GPIO_PIN;
        //        var i2CWriteBuffer = new byte[] { PORT_EXPANDER_OLAT_REGISTER_ADDRESS, olatRegister };

        //        this.Device.Write(i2CWriteBuffer);

        //        // configure only the LED pin to be an output and leave the other pins as they are.
        //        // input is logic low, output is logic high
        //        var bitMask = (byte)(0xFF ^ LED_GPIO_PIN); // set the LED GPIO pin mask bit to '0', all other bits to '1'
        //        iodirRegister &= bitMask;
        //        i2CWriteBuffer = new byte[] { PORT_EXPANDER_IODIR_REGISTER_ADDRESS, iodirRegister };
        //        this.Device.Write(i2CWriteBuffer);
        //    }
        //    catch (Exception ex)
        //    {

        //    }

        //}

        private I2cDevice Device
        {
            get;
            set;
        }
    }
}
