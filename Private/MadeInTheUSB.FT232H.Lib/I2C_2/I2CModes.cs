#define FT232H                // Enable only one of these defines depending on your device type
//#define FT2232H
//#define FT4232H

namespace MadeInTheUSB.FT232H
{
    public enum I2CClockSpeeds
    {
        I2C_CLOCK_STANDARD_MODE_100Khz = 100000,
        I2C_CLOCK_FAST_MODE_400Khz = 400000,
        I2C_CLOCK_FAST_MODE_1_Mhz = 1000000,
        I2C_CLOCK_HIGH_SPEED_MODE_3_4Mhz = 3400000
    }
}


