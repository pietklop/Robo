using System;

namespace Controller.Sensor
{
    public class MeasurementEventArgs : EventArgs
    {
        public SensorBase Sensor { get; protected set; }

        public MeasurementEventArgs(SensorBase sensor)
        {
            Sensor = sensor;
        }
    }
}