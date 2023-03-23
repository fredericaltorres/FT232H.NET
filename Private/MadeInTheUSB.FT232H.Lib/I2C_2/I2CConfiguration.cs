#define FT232H                // Enable only one of these defines depending on your device type
//#define FT2232H
//#define FT4232H

namespace MadeInTheUSB.FT232H
{
    public class I2CConfiguration
    {
        public static readonly I2CConfiguration ChannelZeroConfiguration = new I2CConfiguration(0);

        public int ChannelIndex { get; private set; }

        public I2CConfiguration(int channelIndex)
        {
            ChannelIndex = channelIndex;
        }

    }
}


