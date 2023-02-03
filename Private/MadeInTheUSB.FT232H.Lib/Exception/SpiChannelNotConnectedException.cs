using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MadeInTheUSB.FT232H
{
    public class SpiChannelNotConnectedException : Exception
    {
        public FtdiMpsseSPIResult Reason { get; private set; }

        public SpiChannelNotConnectedException(FtdiMpsseSPIResult res)
        {
            Reason = res;
        }
    }
}
