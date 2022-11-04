using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;

namespace RLCMacroReader
{
    public class Settings
    {
        public string PortName;
        public int Baudrate;
        public Parity Parity;
        public int Databits;
        public StopBits StopBits;

        public Settings()
        {
            PortName = "com1";
            Baudrate = 19200;
            Parity = Parity.None;
            Databits = 8;
            StopBits = StopBits.One;
        }
    }
}
