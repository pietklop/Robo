using System.Collections.Generic;

namespace Controller.Sensor
{
    public abstract class SensorBase
    {
        public readonly string Name;
        public readonly string Type;
        
        public string Unit { get; protected set; }
        protected int nDigits;
        
        public List<Measurement> Measurements { get; protected set; }
        private const int MAX_MEASUREMENTS = 10;

        protected SensorBase(string type, string name)
        {
            Type = type;
            Name = name;
            Measurements = new List<Measurement>();
        }

        // enforce to set class parameters
        protected abstract void SetUnit();
        protected abstract void SetDigits();

        public void AddMeasurement(string value)
        {
            Measurements.Insert(0, new Measurement(value));
            if (Measurements.Count > MAX_MEASUREMENTS)
                Measurements.RemoveAt(MAX_MEASUREMENTS);
        }

        public double LastValue()
        {
            return Measurements[0].Value;
        }

        public string LastValueString()
        {
            return string.Format("{0} {1}", LastValue().ToString("f" + nDigits), Unit);
        }

        public override string ToString()
        {
            return string.Format("{0} - {1}", Type, Name);
        }
    }
}