using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MadeInTheUSB.FT232H
{
    public class SpiChannelNotConnectedException : Exception
    {
        public FtdiMpsseResult Reason { get; private set; }

        public SpiChannelNotConnectedException(FtdiMpsseResult res)
        {
            Reason = res;
        }
    }
}
