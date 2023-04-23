using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MadeInTheUSB.FT232H
{
    public class GpioException : Exception
    {
        public FtdiMpsseResult Reason { get; private set; }
        public GpioException(FtdiMpsseResult res, string message) : base($"GPIO operation failed, {message}. code:{res}")
        {
            Reason = res;
        }
        public GpioException(string message) : base($"GPIO operation failed, {message}")
        {
         
        }
    }
}
