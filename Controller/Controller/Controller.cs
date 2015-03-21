
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace Controller
{
    public class Controller
    {
        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private Communicater communicater;

        public Controller()
        {
            communicater = new Communicater();
        }

        public void Start()
        {
            communicater.Start();
            communicater.InstructionReceived += HandleIncomingInstruction;
        }

        private void HandleIncomingInstruction(object sender, InstructionEventArgs e)
        {
            switch (e.Instruction.Command)
            {
                case "x":
                    log.InfoFormat("Input {0}", e.Instruction.Parameters.First());
                    break;
                default:
                    log.ErrorFormat("Unknown command received: {0}", e.Instruction.Command); 
                    break;
            }
        }

        public void Stop()
        {
            communicater.Stop();
        }

        public void SendTestData()
        {
            communicater.SendTestData(CreateMessage());
        }

        private byte[] CreateMessage()
        {
            Instruction instruction1 = new Instruction("x", new List<string>{"1.0"});
            Instruction instruction2 = new Instruction("x", new List<string>{"2.0"});
            string s = communicater.ComposeMessage(new List<Instruction> {instruction1});
            return Encoding.ASCII.GetBytes(s);
        }


    }
}