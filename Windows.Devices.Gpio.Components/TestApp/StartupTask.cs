using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.System.Threading;
using Windows.Devices.Gpio;
using Windows.Devices.Gpio.Components;
using Windows.Devices.Gpio.Animations;
using System.Diagnostics;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace TestApp
{
    public sealed class StartupTask : IBackgroundTask
    {
        private BackgroundTaskDeferral _deferral;
        private ThreadPoolTimer _timer;

        private Led _led1;
        private Led _led2;
        private BlinkAnimation _animation;
        private UltraSonicSensor _eyes;
        private SoftPwmDriver[] _drivers;
        private int delta = 5;
        private int value = 0;

        public StartupTask()
        { }

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();

            this.UCSensor();
            this.SoftPwm();
        }

        private void UCSensor()
        {
            this.InitGpio();
            _animation = new BlinkAnimation(_led1, _led2);
            _animation.Start();
            _timer = ThreadPoolTimer.CreatePeriodicTimer(Timer_Tick, TimeSpan.FromMilliseconds(500));
        }

        private void SoftPwm()
        {
            _drivers = new SoftPwmDriver[1];
            _drivers[0] = new SoftPwmDriver(5);
            _drivers[0].Enable();

            _timer = ThreadPoolTimer.CreatePeriodicTimer(Timer_PwmTick, TimeSpan.FromMilliseconds(50));

        }

        private void Timer_PwmTick(ThreadPoolTimer timer)
        {
            var newValue = value + delta;
            if (newValue < 0 || newValue > 100)
            {
                delta = delta * -1;
                newValue = value + delta;
            }

            this.value = newValue;

            foreach (var driver in _drivers)
            {
                driver.Value = value;
            }

        }


        private void Timer_Tick(ThreadPoolTimer timer)
        {
            _eyes.SenseDistance();
        }

        private void InitGpio()
        {
            _led1 = new Led(18);
            _led2 = new Led(27);
            _led2.On();

            _eyes = new UltraSonicSensor(23, 24);
            _eyes.DistanceSensed += _eyes_DistanceSensed;
        }

        private void _eyes_DistanceSensed(object sender, DistanceSensedEventArgs e)
        {
            Debug.WriteLine("{0}ms (distance: {1})", e.Duration.TotalMilliseconds, e.Distance);

            var blinkPeriod = Math.Min(400, e.Duration.TotalMilliseconds * 150);
            _animation.Period = TimeSpan.FromMilliseconds(blinkPeriod);
        }


    }
}
