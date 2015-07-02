using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio.Components;
using Windows.System.Threading;

namespace Windows.Devices.Gpio.Animations
{
    /// <summary>
    /// Blink animation
    /// </summary>
    public class BlinkAnimation
    {
        private ThreadPoolTimer _timer;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="leds">Leds to run animation on.</param>
        public BlinkAnimation(params Led[] leds)
        {
            this.Leds = new List<Led>(leds);
            this.Period = TimeSpan.FromMilliseconds(500);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="leds">Leds to run animation on.</param>
        public BlinkAnimation(IEnumerable<Led> leds)
        {
            this.Leds = new List<Led>(leds);
            this.Period = TimeSpan.FromMilliseconds(500);
        }

        /// <summary>
        /// Gets the leds the animation runs on.
        /// </summary>
        public IList<Led> Leds
        {
            get;
            private set;
        }

        private TimeSpan _period;

        /// <summary>
        /// Gets the period of the animation
        /// </summary>
        public TimeSpan Period
        {
            get { return _period; }
            set
            {
                if (value != _period)
                {
                    _period = value;
                    if (_timer != null)
                    {
                        _timer.Cancel();
                        this.CreateTimer();
                    }
                }
            }
        }

        private void CreateTimer()
        {
            _timer = ThreadPoolTimer.CreatePeriodicTimer(this.Timer_Tick, _period);
        }

        /// <summary>
        /// Starts the animation
        /// </summary>
        public void Start()
        {
            if (_timer == null) this.CreateTimer();
        }

        /// <summary>
        /// Stops the animation
        /// </summary>
        public void Stop()
        {
            if (_timer != null)
            {
                _timer.Cancel();
                _timer = null;
            }
        }

        private void Timer_Tick(ThreadPoolTimer timer)
        {
            this.Toggle();
        }

        private void Toggle()
        {
            foreach (var l in this.Leds)
            {
                l.Toggle();
            }

        }
    }
}
