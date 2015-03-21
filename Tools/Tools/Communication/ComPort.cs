using System;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace Tools.Communication
{
    public delegate void TextDataReceivedEventHandler(string readData);
    public delegate void MessageEventHandler(string message);

    public class ComPort
    {
        #region Event declaration
		/// <summary>
		/// Event will be triggerd at every received input
		/// Use the message events in case suffix is configured
		/// </summary>
        public event EventHandler<BytesReceivedEventArgs> BytesReceived;
        public event EventHandler<StringReceivedEventArgs> StringReceived;

        /// <summary>
        /// Use in case suffix is configured
        /// Event will only pass complete messages
        /// (prefix and suffix are not included in the message)
        /// </summary>
        public event EventHandler<BytesReceivedEventArgs> MessageBytesReceived;
        public event EventHandler<StringReceivedEventArgs> MessageStringReceived;

		public delegate void PortStatusHandler();
		public event PortStatusHandler PortOpened;
		public event PortStatusHandler PortClosed;
		#endregion

        #region Variable declaration
        public const byte STX = 0x2;
        public const byte ETX = 0x3;

        public string Msg;

        private byte[] memData;
        private SerialPort port = new SerialPort();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

		#region Properties
		public bool IsOpen 
		{
			get { return port.IsOpen; }
		}
        public byte Prefix { get; protected set; }
        public byte Suffix { get; protected set; }
		#endregion

		#region Constructors
		public ComPort() { }
        public ComPort(byte prefix, byte suffix) 
        {
            Prefix = prefix;
            Suffix = suffix;
        }

        #endregion

        #region Data handling
        public void SendData(byte[] bytes)
        {
            if (port.IsOpen)
                port.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Send messsage
        /// Prefix and suffix will be added
        /// </summary>
        /// <param name="bytes"></param>
        public void SendMessage(byte[] bytes)
        {
            SendData(AddPrefixSuffix(bytes));
        }

        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int bytes = port.BytesToRead;
            byte[] readByte = new byte[bytes];

            port.Read(readByte, 0, bytes);
            log.DebugFormat("{0}: Data received. bytes={1}", this, readByte.Length);

            ProcessData(readByte);
        }

        private void ProcessData(byte[] bytes)
        {
            if (0 == bytes.Length)
                return; // no data

            if (BytesReceived != null) BytesReceived(this, new BytesReceivedEventArgs(bytes));
            if (StringReceived != null) StringReceived(this, new StringReceivedEventArgs(Encoding.ASCII.GetString(bytes)));
            if (MessageBytesReceived != null || MessageStringReceived != null) 
                CheckForMessage(bytes);
        }

        /// <summary>
        /// Compose complete messages out of received data based on prefix and suffix
        /// </summary>
        /// <param name="receive"></param>
        /// <remarks>
        /// There is no packetlength related check, so the prefix or 
        /// suffix may not be used in the data
        /// </remarks>
        private void CheckForMessage(byte[] receive)
        {
            if (memData == null) memData = new byte[0];

            byte[] data;
            if (memData.Length == 0)
            {   //nothing in memory
                data = receive;
                // check the received data
                if (data[0] != Prefix)
                {
                    log.WarnFormat("{0}: Received message does not start with prefix. start byte={1}", this, data[0]);
                    // look for prefix
                    int iPrefix = Array.FindIndex(data, p => p == Prefix);
                    if (iPrefix == -1)
                        return; // not found
                    else
                    {   // cut off part in front of prefix
                        byte[] dest = new byte[data.Length - iPrefix];
                        Array.Copy(data, iPrefix, dest, 0, data.Length - iPrefix);
                        data = dest;
                    }
                }
            }
            else
            {	// combine memory with new data
                data = new byte[memData.Length + receive.Length];
                Array.Copy(memData, data, memData.Length);
                Array.Copy(receive, 0, data, memData.Length, receive.Length);
                memData = null;
            }

            // look for suffix
            int iSuffix = Array.FindIndex(data, p => p == Suffix);
            if (iSuffix == -1)
            {   // no suffix
                memData = data;
                return;
            }

            if (iSuffix == data.Length - 1) // last byte is suffix
                RaiseMessageEvent(RemovePrefixSuffix(data));
            else // we have bytes after suffix
            {
                // cut of message till suffix
                byte[] msg = new byte[iSuffix -1];
                Array.Copy(data, 1, msg, 0, iSuffix - 1);
                RaiseMessageEvent(msg);
                // keep the rest
                byte[] rest = new byte[data.Length - iSuffix - 1];
                Array.Copy(data, iSuffix + 1, rest, 0, rest.Length);
                // recurse in case of multiple messages
                CheckForMessage(rest);
            }
        }

        private void RaiseMessageEvent(byte[] data)
        {
            if (MessageBytesReceived != null)
                MessageBytesReceived(this, new BytesReceivedEventArgs(data));
            if (MessageStringReceived != null)
                MessageStringReceived(this, new StringReceivedEventArgs(Encoding.ASCII.GetString(data)));
        }

        private byte[] RemovePrefixSuffix(byte[] data)
        {
            return data.Skip(1).Take(data.Length - 2).ToArray();
        }

        private byte[] AddPrefixSuffix(byte[] bytes)
        {
            int nBytes = bytes.Length;
            byte[] toSend = new byte[nBytes + 2];
            Array.Copy(bytes, 0, toSend, 1, nBytes);
            toSend[0] = Prefix;
            toSend[nBytes + 1] = Suffix;
            return toSend;
        }
        
        #endregion

        #region Open / close
        public void PortOpen(string name, int baud, Parity par, StopBits sbits, int dbits)
        {
            log.InfoFormat("Open {0}", name);
            // first check if the port is allready opened
            if (port.IsOpen)
                port.Close();

            port.PortName = name;
            port.BaudRate = baud;
            port.Parity = par;
            port.DataBits = dbits;
            port.StopBits = sbits;

            port.DataReceived += port_DataReceived;
            try
            {
                port.Open();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("{0}: Opening failed. msg={1}", this, ex.Message);
            }
            if (port.IsOpen && PortOpened != null)
                PortOpened(); // trigger event
        }

        public void PortOpen(int portNr, int baud, Parity par, StopBits sbits, int dbits)
        {
            string name = "COM" + portNr;
            PortOpen(name, baud, par, sbits, dbits);
        }

        public void PortClose()
        {
            log.InfoFormat("close {0}", this);
            if (port.IsOpen)
            {
                port.DataReceived -= port_DataReceived;
                if (PortClosed != null)
                    PortClosed();
            }
        }
        
        #endregion

        public override string ToString()
        {
            return port.PortName;
        }

        #region Static methods
        /// <summary>
        /// Get available com ports in logical order
        /// </summary>
        /// <returns>String[] port names</returns>
        public static string[] GetSystemPortNames()
        {
            // Just a placeholder for a successful parsing of a string to an integer
            int num;
            // Order the serial port names in numberic order (if possible)
            return SerialPort.GetPortNames().OrderBy(a => a.Length > 3 && int.TryParse(a.Substring(3), out num) ? num : 0).ToArray();
        }

        /// <summary> Convert a string of hex digits (ex: E4 CA B2) to a byte array. </summary>
        /// <param name="s"> The string containing the hex digits (with or without spaces). </param>
        /// <returns> Returns an array of bytes. </returns>
        public static byte[] HexStringToByteArray(string s)
        {
            s = s.Replace(" ", "");
            byte[] buffer = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
                buffer[i / 2] = Convert.ToByte(s.Substring(i, 2), 16);
            return buffer;
        }

        /// <summary> Converts an array of bytes into a formatted string of hex digits (ex: E4 CA B2)</summary>
        /// <param name="data"> The array of bytes to be translated into a string of hex digits. </param>
        /// <returns> Returns a well formatted string of hex digits with spacing. </returns>
        public static string ByteArrayToHexString(byte[] data)
        {
            StringBuilder sb = new StringBuilder(data.Length * 3);
            foreach (byte b in data)
                sb.Append(Convert.ToString(b, 16).PadLeft(2, '0').PadRight(3, ' '));
            return sb.ToString().ToUpper();
        }

        public static string ByteArrayToHexString(byte[] data, int startIndex, int length)
        {
            if (startIndex + length > data.Length || length <= 0)
                throw new Exception("Unvalid length");

            byte[] array = new byte[length];
            Array.Copy(data, startIndex, array, 0, length);
            return ByteArrayToHexString(array);
        }
        
        #endregion

        #region Test
        public void SimulateIncomingData(string data)
        {
            ProcessData(Encoding.ASCII.GetBytes(data));
        }

        public void SimulateIncomingData(byte[] bytes)
        {
            ProcessData(bytes);
        }

        public void SimulateIncomingMessage(byte[] bytes)
        {
            ProcessData(AddPrefixSuffix(bytes));
        }
        
        #endregion
    }
}
