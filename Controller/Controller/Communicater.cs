using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using Controller.Sensor;
using log4net;
using Tools.Communication;

namespace Controller
{
    /// <summary>
    /// Class handles serial communication
    /// </summary>
    public class Communicater
    {
        private readonly Controller controller;
        private ComPort comPort;
        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private int lastMsgNr = -1;
        private const char MESSAGE_SEPERATOR = '%';

        public event EventHandler<MeasurementEventArgs> NewMeasurement; 

        public Communicater(Controller controller)
        {
            this.controller = controller;
            comPort = new ComPort(ComPort.STX, ComPort.ETX);
        }

        public void Start()
        {
            comPort.MessageStringReceived += HandleIncomingMessage;
            comPort.PortOpen(1, 19200, Parity.None, StopBits.One, 8);
        }

        public void Stop()
        {
            comPort.PortClose();
            comPort.MessageStringReceived -= HandleIncomingMessage;
        }

        #region Instruction handling
        private void HandleIncomingInstruction(Instruction instruction)
        {
            switch (instruction.Command)
            {
                case "x":
                    HandleInputInstruction(instruction);
                    break;
                default:
                    log.ErrorFormat("Unknown command received: {0}", instruction.Command);
                    break;
            }
        }

        private void HandleInputInstruction(Instruction instruction)
        {
            if (instruction.Parameters.Count < 3)
            {
                log.ErrorFormat("Input instruction has to less parameters");
                return;
            }

            string type = instruction.Parameters[0];
            string name = instruction.Parameters[1];
            string value = instruction.Parameters[2];
            SensorBase sensor = controller.GetOrCreateSensor(type, name);
            sensor.AddMeasurement(value);
            if (NewMeasurement != null)
                NewMeasurement(this, new MeasurementEventArgs(sensor));
        }
        
        #endregion

        #region Message handling
        private void HandleIncomingMessage(object sender, StringReceivedEventArgs e)
        {
            log.InfoFormat("Received: {0}", e.Data);

            string[] instructions = e.Data.Split(MESSAGE_SEPERATOR);

            if (!instructions.Any())
            {
                log.ErrorFormat("Invalid message received, header is missing");
                return;
            }

            ProcessHeader(instructions.First());

            for (int i = 1; i < instructions.Length; i++)
                HandleIncomingInstruction(new Instruction(instructions[i]));
        }

        private void ProcessHeader(string header)
        {
            int msgNr = int.Parse(header.Substring(0, 1));

            if (lastMsgNr >= 0)
            {
                int expectedMsgNr = (lastMsgNr + 1) % 10;
                if (expectedMsgNr != msgNr)
                {
                    log.ErrorFormat("Missed message msg nr={0} expected={1}", msgNr, expectedMsgNr);
                }
            }
            lastMsgNr = msgNr;
        }

        public string ComposeMessage(List<Instruction> instructions)
        {
            return string.Format("{0}{1}{2}",
                0, MESSAGE_SEPERATOR,
                string.Join(MESSAGE_SEPERATOR.ToString(), instructions.Select(x => x.ToString())));
        }

        public void SendTestData(byte[] data)
        {
            comPort.SimulateIncomingMessage(data);
        }
        
        #endregion

    }
}