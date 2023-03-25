using System.Runtime.InteropServices;

namespace MadeInTheUSB.FT232H
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SpiChannelConfig
    {
        public int ClockRate;
        public byte LatencyTimer;
        public SpiConfigOptions configOptions;
        public int Pin;
        public short reserved;
    }
}
