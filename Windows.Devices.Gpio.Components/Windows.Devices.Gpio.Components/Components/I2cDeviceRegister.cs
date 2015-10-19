using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.I2c;

namespace Windows.Devices.Gpio.Components
{
    /// <summary>
    /// Class to represent a 8-bit i2cdevice register
    /// </summary>
    public class I2cDeviceRegister
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="device">Device</param>
        /// <param name="address">Address of the register on the device</param>
        public I2cDeviceRegister(I2cDevice device, byte address)
        {
            this.Device = device;
            this.Address = address;
            this.Value = 0;
        }

        /// <summary>
        /// Gets the device
        /// </summary>
        public I2cDevice Device
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the register address
        /// </summary>
        public byte Address
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the last read value of the register.
        /// </summary>
        public byte Value
        {
            get;
            private set;
        }

        /// <summary>
        /// Read the value from the register.
        /// </summary>
        /// <returns></returns>
        public byte Read()
        {
            var buff = new byte[1];
            this.Device.WriteRead(new byte[] { this.Address }, buff);
            this.Value = buff[0];
            return this.Value;
        }

        /// <summary>
        /// Writes the value to the register.
        /// </summary>
        /// <param name="newValue">New value of the register.</param>
        /// <returns>True when the value was successfully written, false otherwise.</returns>
        public bool Write(byte newValue)
        {
            this.Device.Write(new byte[] { this.Address, newValue });
            return this.Read() == newValue;
        }

        /// <summary>
        /// Reads the bit value from the provided address from the register.
        /// </summary>
        /// <param name="pos">position of the bit to read. Should be between 0-7</param>
        /// <returns>True when high, false otherwise.</returns>
        public bool Read(int pos)
        {
            var mask = this.PosToMask(pos);
            var value = this.Read();
            return (value & mask) != 0x00;
        }

        /// <summary>
        /// Writes the bit value to the provided address in the register.
        /// </summary>
        /// <param name="pos">position of the bit to read. Should be between 0-7</param>
        /// <param name="value">Value to write.</param>
        public void Write(int pos, bool value)
        {
            var mask = this.PosToMask(pos);
            var curValue = this.Read();
            var newValue = (byte)(value ? curValue | mask : curValue & ~mask);
            this.Write(newValue);
        }

        /// <summary>
        /// Converts a bit position to its mask.
        /// </summary>
        /// <param name="pos">Bit position.</param>
        /// <returns>The mask of the bit position.</returns>
        private byte PosToMask(int pos)
        {
            if (pos < 0 || pos >= 8) throw new ArgumentException("", "pos");
            return (byte)Math.Pow(2, pos);
        }
    }
}
