using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Windows.Devices.Gpio.Components
{
    /// <summary>
    /// An HC-SR04 Ultrasonic Sensor.
    /// </summary>
    public class UltraSonicSensor
    {
        private IGpioPin _trigPin;
        private IGpioPin _echoPin;
        private Stopwatch _timer;
        private bool _sensedBefore;

        /// <summary>
        /// Event that occurs when distance is sensed.
        /// </summary>
        public event EventHandler<DistanceSensedEventArgs> DistanceSensed;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="trigPin">pin number of the trig of the sensor.</param>
        /// <param name="echoPin">pint number of the echo of the sensor.</param>
        public UltraSonicSensor(int trigPin, int echoPin)
        {
            _trigPin = GenericGpioController.GetDefault().OpenPin(trigPin);
            _trigPin.SetDriveMode(GpioPinDriveMode.Output);

            _echoPin = GenericGpioController.GetDefault().OpenPin(echoPin);
            _echoPin.SetDriveMode(GpioPinDriveMode.Input);

            _trigPin.Write(GpioPinValue.Low);

            _echoPin.ValueChanged += _echoPin_ValueChanged;
            _timer = new Stopwatch();
        }

        /// <summary>
        /// Senses the distance.
        /// </summary>
        public void SenseDistance()
        {
            var timer = new HighResolutionTimer();
            this.EnsureReady();

            _timer = new Stopwatch();
            _trigPin.Write(GpioPinValue.High);
            timer.Sleep(0.01d);
            _trigPin.Write(GpioPinValue.Low);
        }

        /// <summary>
        /// Waits a couple of millseconds before using the sensor to make sure the sensor is ready.
        /// </summary>
        private void EnsureReady()
        {
            if (!_sensedBefore)
            {
                using (var mre = new ManualResetEvent(false))
                {
                    mre.WaitOne(500);
                    _sensedBefore = true;
                }
            }
        }

        /// <summary>
        /// Handles the pin's value changed event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void _echoPin_ValueChanged(IGpioPin sender, ValueChangedEventArgs args)
        {
            if (args.Edge == GpioPinEdge.RisingEdge)
            {
                _timer.Reset();
                _timer.Start();
            }

            if (args.Edge == GpioPinEdge.FallingEdge)
            {
                _timer.Stop();
                this.OnDistanceSensed(this, new DistanceSensedEventArgs(_timer.Elapsed));
            }
        }

        /// <summary>
        /// Raises the distance sended event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnDistanceSensed(object sender, DistanceSensedEventArgs e)
        {
            if (this.DistanceSensed != null)
            {
                this.DistanceSensed(sender, e);
            }
        }
    }
}
