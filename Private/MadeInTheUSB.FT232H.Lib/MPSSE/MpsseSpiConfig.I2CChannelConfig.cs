using System.Runtime.InteropServices;

namespace MadeInTheUSB.FT232H
{
    public partial struct SpiConfig
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct I2CChannelConfig
        {
            public int ClockRate;
            public byte LatencyTimer;
            public FtdiMpsseSpiConfigOptions configOptions;
            public int Pin;
            public short reserved;
        }
    }
}
