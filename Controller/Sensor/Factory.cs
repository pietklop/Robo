using System;

namespace Controller.Sensor
{
    public static class Factory
    {
        public static SensorBase Create(string type, string name)
        {
            switch (type)
            {
                case "us1":
                    return new Ultrasone1(type, name);
                default:
                    throw new Exception(string.Format("Unknown sensor. type={0} name={1}", type, name));
            }
        }
    }
}