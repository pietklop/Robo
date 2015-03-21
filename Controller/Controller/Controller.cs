
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Controller.Sensor;
using log4net;

namespace Controller
{
    public class Controller
    {
        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private Communicater communicater;
        private List<SensorBase> sensors;

        public Controller()
        {
            communicater = new Communicater(this);
            sensors = new List<SensorBase>();
        }

        public void Start()
        {
            communicater.NewMeasurement += HandleNewMeasurement;
            communicater.Start();
        }

        private void HandleNewMeasurement(object sender, MeasurementEventArgs e)
        {
            log.DebugFormat("New value: {0} {1}", e.Sensor, e.Sensor.LastValueString());
        }

        public void Stop()
        {
            communicater.Stop();
            communicater.NewMeasurement -= HandleNewMeasurement;
        }

        public SensorBase GetOrCreateSensor(string type, string name)
        {
            SensorBase sensor = sensors.SingleOrDefault(x => x.Name == name);
            if (sensor != null) return sensor;

            // new sensor
            sensor = Factory.Create(type, name);
            sensors.Add(sensor);
            log.InfoFormat("Added sensor: {0}", sensor);
            return sensor;
        }

        public void SendTestData()
        {
            communicater.SendTestData(CreateMessage());
        }

        private byte[] CreateMessage()
        {
            List<Instruction> instructions = new List<Instruction>
            {
                new Instruction("x", new List<string> {"us1", "left", "1.1"}),
                new Instruction("x", new List<string> {"us1", "left", "2.0"}),
                new Instruction("x", new List<string> {"us1", "right", "3"}),
            };
            string s = communicater.ComposeMessage(instructions);
            return Encoding.ASCII.GetBytes(s);
        }


    }
}