#define FT232H                // Enable only one of these defines depending on your device type
//#define FT2232H
//#define FT4232H
using System;

namespace MadeInTheUSB.FT232H
{
    // https://www.ftdichip.com/Support/Documents/AppNotes/AN_135_MPSSE_Basics.pdf
    // https://ftdichip.com/wp-content/uploads/2020/08/AN_177_User_Guide_For_LibMPSSE-I2C.pdf

    // PROBLEM
    // https://www.ftdicommunity.com/index.php?topic=241.0

    // Look into the I2C FT232H Implementation
    //https://github.com/chadwyck-w/FT232H-MPSSE-I2C-SSD1306-OLED/blob/master/i2c_lib.c

    // C:\DVT\Adafruit_Blinka\src\adafruit_blinka\microcontroller\ftdi_mpsse\mpsse\i2c.py

    public class I2CChannelNotConnectedException : Exception
    {
        public FtdiMpsseSPIResult Reason { get; private set; }

        public I2CChannelNotConnectedException(FtdiMpsseSPIResult res)
        {
            Reason = res;
        }
    }
}


