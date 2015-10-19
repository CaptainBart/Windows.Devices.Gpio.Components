using System;
using Windows.Foundation;

namespace Windows.Devices.Gpio.Components
{
    /// <summary>
    /// Simple push button 
    /// </summary>
    public class Button
    {
        /// <summary>
        /// Event that occurs when the button's pressed state changes
        /// </summary>
        public event TypedEventHandler<Button, EventArgs> PressedChanged;

        private IGpioPin _pin;
        private GpioPinEdge _pressedEdge;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="pin"></param>
        /// <param name="mode">The drive mode of the pin. When inputPullUp is used, the pressed state will be true when a falling edge is detected.</param>
        public Button(IGpioPin pin, GpioPinDriveMode mode)
        {
            if (pin == null) throw new ArgumentNullException("pin");
            if (mode != GpioPinDriveMode.Input && mode != GpioPinDriveMode.InputPullDown && mode != GpioPinDriveMode.InputPullUp) throw new ArgumentOutOfRangeException("mode", "Drive mode should be an input drive type.");

            _pin = pin;
            _pressedEdge = mode == GpioPinDriveMode.InputPullUp ? GpioPinEdge.FallingEdge : GpioPinEdge.RisingEdge;
            _pin.SetDriveMode(mode);
            _pin.ValueChanged += _pin_ValueChanged;
        }

        /// <summary>
        /// Gets or sets whether the button is pressed.
        /// </summary>
        public bool IsPressed
        {
            get;
            private set;
        }

        /// <summary>
        /// Captures the pin's value changed event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void _pin_ValueChanged(IGpioPin sender, ValueChangedEventArgs args)
        {
            this.IsPressed = args.Edge == _pressedEdge;
            this.OnPressedChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the pressedchanged event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnPressedChanged(Button sender, EventArgs e)
        {
            if (this.PressedChanged != null)
            {
                this.PressedChanged(sender, e);
            }
        }
    }
}
