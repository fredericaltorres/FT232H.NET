using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MadeInTheUSB.FT232H
{
    /// <summary>
    /// The channel is the number of FT232H on the chip
    /// FT232H      1
    /// FT2232H     2
    /// FT4232H     4
    /// We only support the FT232H which is channel 0
    /// </summary>
    public class MpsseChannelConfiguration
    {
        public static readonly MpsseChannelConfiguration FtdiMpsseChannelZeroConfiguration = new MpsseChannelConfiguration(0);
        public int ChannelIndex { get; private set; }

        public MpsseChannelConfiguration(int channelIndex)
        {
            ChannelIndex = channelIndex;
        }
    }
}
