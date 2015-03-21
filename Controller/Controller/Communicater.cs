using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using log4net;
using Tools.Communication;

namespace Controller
{
    /// <summary>
    /// Class handles serial communication
    /// </summary>
    public class Communicater
    {
        private ComPort comPort;
        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private int lastMsgNr = -1;
        public event EventHandler<InstructionEventArgs> InstructionReceived;
        private const char MESSAGE_SEPERATOR = '%';

        public Communicater()
        {
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

        public void SendTestData(byte[] data)
        {
            comPort.SimulateIncomingMessage(data);
        }

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

            if (InstructionReceived != null)
            {
                for (int i = 1; i < instructions.Length; i++)
                    InstructionReceived(this, new InstructionEventArgs(new Instruction(instructions[i])));
            }
        }

        private void ProcessHeader(string header)
        {
            int msgNr = int.Parse(header.Substring(0, 1));

            if (lastMsgNr >= 0)
            {
                int expectedMsgNr = (lastMsgNr+1) % 10;
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


    }
}