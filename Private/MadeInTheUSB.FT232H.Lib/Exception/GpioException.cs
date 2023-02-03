using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MadeInTheUSB.FT232H
{
    public class GpioException : Exception
    {
        public FtdiMpsseSPIResult Reason { get; private set; }
        public GpioException(FtdiMpsseSPIResult res, string message) : base($"GPIO operation failed, {message}. code:{res}")
        {
            Reason = res;
        }
    }
}
