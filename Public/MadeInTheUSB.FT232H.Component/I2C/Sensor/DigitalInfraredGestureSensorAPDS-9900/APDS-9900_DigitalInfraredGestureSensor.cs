//#define OPTIMIZE_I2C_CALL
/*
    Written by FT for MadeInTheUSB
    Copyright (C) 2015, 2023 MadeInTheUSB LLC

    The MIT License (MIT)

        Permission is hereby granted, free of charge, to any person obtaining a copy
        of this software and associated documentation files (the "Software"), to deal
        in the Software without restriction, including without limitation the rights
        to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
        copies of the Software, and to permit persons to whom the Software is
        furnished to do so, subject to the following conditions:

        The above copyright notice and this permission notice shall be included in
        all copies or substantial portions of the Software.

        THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
        IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
        FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
        AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
        LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
        OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
        THE SOFTWARE.
 
    This code is based from 
  
    Adafruit_MCP9808 - library - https://github.com/adafruit/Adafruit_MCP9808_Library
  
    See Adafruit: 
        - MCP9808 High Accuracy I2C Temperature Sensor Breakout Board - https://www.adafruit.com/product/1782
        - Adafruit MCP9808 Precision I2C Temperature Sensor Guide - https://learn.adafruit.com/adafruit-mcp9808-precision-i2c-temperature-sensor-guide/overview
*/

using MadeInTheUSB.FT232H;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using int16_t = System.Int16; // Nice C# feature allowing to use same Arduino/C type
using uint16_t = System.UInt16;
using uint8_t = System.Byte;


namespace MadeInTheUSB
{
    public partial class APDS_9900_DigitalInfraredGestureSensor
    {
       

        public enum Registers
        {
            APDS9960_RAM = 0x00,
            APDS9960_ENABLE = 0x80,
            APDS9960_ATIME = 0x81,
            APDS9960_WTIME = 0x83,
            APDS9960_AILTIL = 0x84,
            APDS9960_AILTH = 0x85,
            APDS9960_AIHTL = 0x86,
            APDS9960_AIHTH = 0x87,
            APDS9960_PILT = 0x89,
            APDS9960_PIHT = 0x8B,
            APDS9960_PERS = 0x8C,
            APDS9960_CONFIG1 = 0x8D,
            APDS9960_PPULSE = 0x8E,
            APDS9960_CONTROL = 0x8F,
            APDS9960_CONFIG2 = 0x90,
            APDS9960_ID = 0x92,
            APDS9960_STATUS = 0x93,
            APDS9960_CDATAL = 0x94,
            APDS9960_CDATAH = 0x95,
            APDS9960_RDATAL = 0x96,
            APDS9960_RDATAH = 0x97,
            APDS9960_GDATAL = 0x98,
            APDS9960_GDATAH = 0x99,
            APDS9960_BDATAL = 0x9A,
            APDS9960_BDATAH = 0x9B,
            APDS9960_PDATA = 0x9C,
            APDS9960_POFFSET_UR = 0x9D,
            APDS9960_POFFSET_DL = 0x9E,
            APDS9960_CONFIG3 = 0x9F,
            APDS9960_GPENTH = 0xA0,
            APDS9960_GEXTH = 0xA1,
            APDS9960_GCONF1 = 0xA2,
            APDS9960_GCONF2 = 0xA3,
            APDS9960_GOFFSET_U = 0xA4,
            APDS9960_GOFFSET_D = 0xA5,
            APDS9960_GOFFSET_L = 0xA7,
            APDS9960_GOFFSET_R = 0xA9,
            APDS9960_GPULSE = 0xA6,
            APDS9960_GCONF3 = 0xAA,
            APDS9960_GCONF4 = 0xAB,
            APDS9960_GFLVL = 0xAE,
            APDS9960_GSTATUS = 0xAF,
            APDS9960_IFORCE = 0xE4,
            APDS9960_PICLEAR = 0xE5,
            APDS9960_CICLEAR = 0xE6,
            APDS9960_AICLEAR = 0xE7,
            APDS9960_GFIFO_U = 0xFC,
            APDS9960_GFIFO_D = 0xFD,
            APDS9960_GFIFO_L = 0xFE,
            APDS9960_GFIFO_R = 0xFF,
        }


        const int I2C_ADDR_DEFAULT = 0x39;
        public int DeviceID = I2C_ADDR_DEFAULT;

        uint8_t gestCnt;
        uint8_t UCount;
        uint8_t DCount;
        uint8_t LCount;
        uint8_t RCount;

        I2CDevice _i2cDevice;

        public APDS_9900_DigitalInfraredGestureSensor(I2CDevice i2cDevice)
        {
            this._i2cDevice = i2cDevice;
        }

        public bool Begin(byte deviceAddress = I2C_ADDR_DEFAULT)
        {
            try
            {
                this.DeviceID = deviceAddress;
                if (!this._i2cDevice.InitiateDetectionSequence(deviceAddress))
                    return false;

                var x = read8(Registers.APDS9960_ID); /* Make sure we're actually connected */
                if (x != 0xAB)
                    return false;


                //if (read16(MCP9808_REG_MANUF_ID) != MCP9808_REG_MANUF_ID_ANSWER) return false;
                //if (read16(MCP9808_REG_DEVICE_ID) != MCP9808_REG_DEVICE_ID_ANSWER) return false;
                return true;
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
                return false;
            }
        }

     
    }
}

