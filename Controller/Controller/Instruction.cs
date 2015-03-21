using System;
using System.Collections.Generic;
using System.Linq;

namespace Controller
{
    public class Instruction
    {
        private const char SEPERATOR = ';';

        public string Command { get; set; }
        public List<string> Parameters { get; set; }

        public Instruction(string command, List<string> parameters)
        {
            Command = command;
            Parameters = parameters;
        }
        public Instruction(string data)
        {
            string[] parts = data.Split(';');
            if (!parts.Any())
                throw new Exception("Invalid instruction, command is missing");

            Command = parts[0];
            Parameters = parts.Skip(1).ToList();
        }

        public override string ToString()
        {
            return String.Format("{0}{1}{2}",
                Command,
                SEPERATOR,
                string.Join(SEPERATOR.ToString(), Parameters));
        }
    }
}