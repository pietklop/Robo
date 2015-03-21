using System;

namespace Tools.Communication
{
    public class StringReceivedEventArgs : EventArgs
    {
        public string Data { get; private set; }

        public StringReceivedEventArgs(string data)
        {
            Data = data;
        }
    }
}