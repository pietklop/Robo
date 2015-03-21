using System;
using System.Globalization;

namespace Controller.Sensor
{
    public class Measurement
    {
        public readonly DateTime Time;
        public readonly double Value;

        public Measurement(string value)
        {
            Time = DateTime.Now;
            Value = double.Parse(value, CultureInfo.InvariantCulture); // always expect a dot
        }
    }
}