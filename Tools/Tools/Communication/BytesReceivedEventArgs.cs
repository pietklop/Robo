using System;

namespace Tools.Communication
{
    public class BytesReceivedEventArgs : EventArgs
    {
        public byte[] Data { get; private set; }

        public BytesReceivedEventArgs(byte[] data)
        {
            Data = data;
        }
    }
}
