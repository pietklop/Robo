using System;

namespace Controller
{
    public class InstructionEventArgs : EventArgs
    {
        public Instruction Instruction { get; private set; }

        public InstructionEventArgs(Instruction instruction)
        {
            Instruction = instruction;
        }
    }
}