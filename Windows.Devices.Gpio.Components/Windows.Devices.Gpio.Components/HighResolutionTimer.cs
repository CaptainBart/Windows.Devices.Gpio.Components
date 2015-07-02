using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windows.Devices.Gpio
{
    /// <summary>
    /// Timer that allows sleeping for very short periods.
    /// </summary>
    public class HighResolutionTimer
    {
        private Stopwatch _sw;

        /// <summary>
        /// Constructor
        /// </summary>
        public HighResolutionTimer()
        {
            _sw = new Stopwatch();
        }

        /// <summary>
        /// Sleep for short period by blocking the thread in a while loop
        /// </summary>
        /// <param name="durationMS"></param>
        internal void Sleep(double durationMS)
        {
            _sw.Start();
            while (_sw.Elapsed.TotalMilliseconds < durationMS)
            { }
            _sw.Reset();
        }
    }
}
