using BufferUtil.Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace MadeInTheUSB.FT232H
{
    /// <summary>
    /// Based class for any FT232H derived classes
    /// </summary>
    public class FT232HDeviceBaseClass
    {
        internal static IntPtr _spiHandle = IntPtr.Zero;
        //internal static IntPtr                _i2cHandle = IntPtr.Zero;
        protected I2CDevice _i2cDevice = null;
        internal static SpiConfig _globalConfig;
        protected const int _maxGpio = 8;
        protected const int ValuesDefaultMask = 0;
        protected const int DirectionDefaultMask = 0xFF;
                
        protected const int _gpioStartIndex = 0;

        public byte GpioStartIndex { get { return _gpioStartIndex; } }

        public byte MaxGpio
        {
            get { return _maxGpio; }
        }


        public List<int> PowerOf2 = new List<int>()
        {
            1, 2, 4, 8, 16, 32, 64, 128,
            256, 512, 1024, 2048
        };

        public string LogFile = @"c:\temp\spi.log";
        public bool Log = false;

        public void LogSpiTransaction(byte[] bufferOut, byte[] bufferIn, int recursiveCounter = 0)
        {
            if (this.Log)
            {
                var sb = new StringBuilder();

                sb.Append($"[{DateTime.Now}]");
                sb.Append("SPI_TRAN ");

                if (bufferOut.Length > 0)
                {
                    sb.Append("OUT:[");
                    sb.Append(HexaString.ConvertTo(bufferOut, itemFormat: "{0}, "));
                    sb.Append("]");
                }

                if (bufferIn.Length > 0)
                {
                    if (bufferOut.Length > 0)
                    {
                        sb.Append(", ");
                    }

                    sb.Append("IN:[");
                    sb.Append(HexaString.ConvertTo(bufferIn, itemFormat: "{0}, "));
                    sb.Append("]");
                }

                try
                {
                    File.AppendAllText(this.LogFile, sb.ToString() + Environment.NewLine);
                }
                catch(System.Exception ex)
                {
                    if (recursiveCounter == 0)
                    {
                        Thread.Sleep(11);
                        LogSpiTransaction(bufferOut, bufferIn, recursiveCounter + 1);
                    }
                    else throw;
                }
            }
        }
    }
}