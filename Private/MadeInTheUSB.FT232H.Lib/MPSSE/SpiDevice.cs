using static MadeInTheUSB.FT232H.SpiConfig;

namespace MadeInTheUSB.FT232H
{
    public class SpiDevice : SpiDeviceBaseClass
    {
        public SpiDevice(SpiClockSpeeds clockSpeed) : base(clockSpeed)
        {
            
        }
    }
}
