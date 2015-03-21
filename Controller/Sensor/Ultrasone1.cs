namespace Controller.Sensor
{
    public class Ultrasone1 : SensorBase
    {
        public Ultrasone1(string type, string name) : base(type, name){}

        protected override void SetUnit()
        {
            Unit = "mm";
        }

        protected override void SetDigits()
        {
            nDigits = 0;
        }
    }
}