using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.System.Threading;
using Windows.Devices.Gpio.Components;
using Windows.Devices.Gpio.Animations;
using Windows.Devices.Gpio;
using System.Diagnostics;
using System.Threading.Tasks;

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
            //this.LedTest();
            //this.UCSensor();
            //this.SoftPwm();
            //this.L293dTest();
            this.PortExpanderTest();
        }

        //private void LedTest()
        //{
        //    var led = new Led(22);
        //    led.On();
        //    led.Off();
        //}

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
            _drivers[0] = new SoftPwmDriver(18);
            _drivers[0].Enable();

            _timer = ThreadPoolTimer.CreatePeriodicTimer(Timer_PwmTick, TimeSpan.FromMilliseconds(50));

        }

        private void L293dTest()
        {
            var leftMotor = new L293d(12, 26, 16);
            var rightMotor = new L293d(5, 13, 6);
            leftMotor.Direction = MotorDirection.Forward;
            rightMotor.Direction = MotorDirection.Forward;
            leftMotor.Speed = 80;
            rightMotor.Speed = 80;
        }

        private async void PortExpanderTest()
        {
            var controller = GenericGpioController.GetDefault();
            await controller.RegisterMcp23017(64, 0x20, 4); //add 16 additional ports from 64 to 79
            
            var pins = new IGpioPin[8];
            var btns = new Button[8];
            GpioPinValue value = GpioPinValue.Low;
            int ledToSkip = -1;

            for (var i = 0; i < btns.Length; i++)
            {
                var btnPin = controller.OpenPin(64 + i);
                var btn = new Button(btnPin, GpioPinDriveMode.InputPullUp);
                btn.PressedChanged += delegate(Button b, EventArgs e) { value = btn.IsPressed ? GpioPinValue.High : GpioPinValue.Low; ledToSkip = Array.IndexOf(btns, b); };
                btns[i] = btn;
            }
            
            for (var i = 0; i < pins.Length; i++)
            {
                var p = controller.OpenPin(72 + i);
                p.SetDriveMode(GpioPinDriveMode.Output);
                p.Write(GpioPinValue.Low);

                pins[i] = p;
            }

            while (true)
            {
                for (var i = 0; i < 8; i++)
                {
                    if (i != ledToSkip)
                    {
                        var p = pins[i];
                        p.Write(value);
                    }
                }

                await Task.Delay(TimeSpan.FromMilliseconds(50));
            }
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