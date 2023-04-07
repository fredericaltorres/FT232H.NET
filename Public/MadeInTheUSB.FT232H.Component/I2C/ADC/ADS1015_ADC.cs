//public const int OPTIMIZE_I2C_CALL
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
    https://github.com/adafruit/Adafruit_ADS1X15

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


    public class ADS1015_ADC
    {

        public const int ADS1X15_ADDRESS = (0x48); ///< 1001 000 (ADDR = GND)
                
        public const int ADS1X15_REG_POINTER_MASK = (0x03);      ///< Point mask
        public const int ADS1X15_REG_POINTER_CONVERT = (0x00);   ///< Conversion
        public const int ADS1X15_REG_POINTER_CONFIG = (0x01);    ///< Configuration
        public const int ADS1X15_REG_POINTER_LOWTHRESH = (0x02); ///< Low threshold
        public const int ADS1X15_REG_POINTER_HITHRESH = (0x03);  ///< High threshold
        
        public const int ADS1X15_REG_CONFIG_OS_MASK = (0x8000); ///< OS Mask
        public const int ADS1X15_REG_CONFIG_OS_SINGLE                                           = (0x8000); ///< Write: Set to start a single-conversion
        public const int ADS1X15_REG_CONFIG_OS_BUSY                                             = (0x0000); ///< Read: Bit = 0 when conversion is in progress
        public const int ADS1X15_REG_CONFIG_OS_NOTBUSY                                          = (0x8000); ///< Read: Bit = 1 when device is not performing a conversion

        public const int ADS1X15_REG_CONFIG_MUX_MASK = (0x7000); ///< Mux Mask
        public const int ADS1X15_REG_CONFIG_MUX_DIFF_0_1                                        = (0x0000); ///< Differential P = AIN0, N = AIN1 = (default);
        public const int ADS1X15_REG_CONFIG_MUX_DIFF_0_3                                        = (0x1000); ///< Differential P = AIN0, N = AIN3
        public const int ADS1X15_REG_CONFIG_MUX_DIFF_1_3                                        = (0x2000); ///< Differential P = AIN1, N = AIN3
        public const int ADS1X15_REG_CONFIG_MUX_DIFF_2_3                                        = (0x3000); ///< Differential P = AIN2, N = AIN3
        public const int ADS1X15_REG_CONFIG_MUX_SINGLE_0 = (0x4000); ///< Single-ended AIN0
        public const int ADS1X15_REG_CONFIG_MUX_SINGLE_1 = (0x5000); ///< Single-ended AIN1
        public const int ADS1X15_REG_CONFIG_MUX_SINGLE_2 = (0x6000); ///< Single-ended AIN2
        public const int ADS1X15_REG_CONFIG_MUX_SINGLE_3 = (0x7000); ///< Single-ended AIN3

        List<uint16_t> MUX_BY_CHANNEL = new List<uint16_t>() {
            ADS1X15_REG_CONFIG_MUX_SINGLE_0, ///< Single-ended AIN0
            ADS1X15_REG_CONFIG_MUX_SINGLE_1, ///< Single-ended AIN1
            ADS1X15_REG_CONFIG_MUX_SINGLE_2, ///< Single-ended AIN2
            ADS1X15_REG_CONFIG_MUX_SINGLE_3  ///< Single-ended AIN3
        };

        public const int ADS1X15_REG_CONFIG_PGA_MASK = (0x0E00);   ///< PGA Mask
        public const int ADS1X15_REG_CONFIG_PGA_6_144V = (0x0000); ///< +/-6.144V range = Gain 2/3
        public const int ADS1X15_REG_CONFIG_PGA_4_096V = (0x0200); ///< +/-4.096V range = Gain 1
        public const int ADS1X15_REG_CONFIG_PGA_2_048V = (0x0400); ///< +/-2.048V range = Gain 2 = (default);
        public const int ADS1X15_REG_CONFIG_PGA_1_024V = (0x0600); ///< +/-1.024V range = Gain 4
        public const int ADS1X15_REG_CONFIG_PGA_0_512V = (0x0800); ///< +/-0.512V range = Gain 8
        public const int ADS1X15_REG_CONFIG_PGA_0_256V = (0x0A00); ///< +/-0.256V range = Gain 16

        public const int ADS1X15_REG_CONFIG_MODE_MASK = (0x0100);   ///< Mode Mask
        public const int ADS1X15_REG_CONFIG_MODE_CONTIN = (0x0000); ///< Continuous conversion mode
        public const int ADS1X15_REG_CONFIG_MODE_SINGLE                                         = (0x0100); ///< Power-down single-shot mode = (default);

        public const int ADS1X15_REG_CONFIG_RATE_MASK = (0x00E0); ///< Data Rate Mask

        public const int ADS1X15_REG_CONFIG_CMODE_MASK = (0x0010); ///< CMode Mask
        public const int ADS1X15_REG_CONFIG_CMODE_TRAD                                          = (0x0000); ///< Traditional comparator with hysteresis = (default);
        public const int ADS1X15_REG_CONFIG_CMODE_WINDOW = (0x0010); ///< Window comparator

        public const int ADS1X15_REG_CONFIG_CPOL_MASK = (0x0008); ///< CPol Mask
        public const int ADS1X15_REG_CONFIG_CPOL_ACTVLOW                                        = (0x0000); ///< ALERT/RDY pin is low when active = (default);
        public const int ADS1X15_REG_CONFIG_CPOL_ACTVHI                                         = (0x0008); ///< ALERT/RDY pin is high when active

        public const int ADS1X15_REG_CONFIG_CLAT_MASK                                           = (0x0004); ///< Determines if ALERT/RDY pin latches once asserted
        public const int ADS1X15_REG_CONFIG_CLAT_NONLAT                                         = (0x0000); ///< Non-latching comparator = (default);
        public const int ADS1X15_REG_CONFIG_CLAT_LATCH = (0x0004); ///< Latching comparator

        public const int ADS1X15_REG_CONFIG_CQUE_MASK = (0x0003); ///< CQue Mask
        public const int ADS1X15_REG_CONFIG_CQUE_1CONV                                          = (0x0000); ///< Assert ALERT/RDY after one conversions
        public const int ADS1X15_REG_CONFIG_CQUE_2CONV                                          = (0x0001); ///< Assert ALERT/RDY after two conversions
        public const int ADS1X15_REG_CONFIG_CQUE_4CONV                                          = (0x0002); ///< Assert ALERT/RDY after four conversions
        public const int ADS1X15_REG_CONFIG_CQUE_NONE                                           = (0x0003); ///< Disable the comparator and put ALERT/RDY in high state = (default);

        ///** Gain settings */
        enum adsGain_t
         {
            GAIN_TWOTHIRDS = ADS1X15_REG_CONFIG_PGA_6_144V,
            GAIN_ONE = ADS1X15_REG_CONFIG_PGA_4_096V,
            GAIN_TWO = ADS1X15_REG_CONFIG_PGA_2_048V,
            GAIN_FOUR = ADS1X15_REG_CONFIG_PGA_1_024V,
            GAIN_EIGHT = ADS1X15_REG_CONFIG_PGA_0_512V,
            GAIN_SIXTEEN = ADS1X15_REG_CONFIG_PGA_0_256V
        };
        
        /** Data rates */
        public const int RATE_ADS1015_128SPS = (0x0000);  ///< 128 samples per second
        public const int RATE_ADS1015_250SPS = (0x0020);  ///< 250 samples per second
        public const int RATE_ADS1015_490SPS = (0x0040);  ///< 490 samples per second
        public const int RATE_ADS1015_920SPS = (0x0060);  ///< 920 samples per second
        public const int RATE_ADS1015_1600SPS = (0x0080); ///< 1600 samples per second = (default);
        public const int RATE_ADS1015_2400SPS = (0x00A0); ///< 2400 samples per second
        public const int RATE_ADS1015_3300SPS = (0x00C0); ///< 3300 samples per second

        public const int RATE_ADS1115_8SPS = (0x0000);   ///< 8 samples per second
        public const int RATE_ADS1115_16SPS = (0x0020);  ///< 16 samples per second
        public const int RATE_ADS1115_32SPS = (0x0040);  ///< 32 samples per second
        public const int RATE_ADS1115_64SPS = (0x0060);  ///< 64 samples per second
        public const int RATE_ADS1115_128SPS = (0x0080); ///< 128 samples per second = (default);
        public const int RATE_ADS1115_250SPS = (0x00A0); ///< 250 samples per second
        public const int RATE_ADS1115_475SPS = (0x00C0); ///< 475 samples per second
        public const int RATE_ADS1115_860SPS = (0x00E0); ///< 860 samples per second

        public int DeviceID;
        I2CDevice _i2cDevice;

        uint8_t m_bitShift;            ///< bit shift amount
        adsGain_t m_gain;              ///< ADC gain
        uint16_t m_dataRate;           ///< Data rate

        public ADS1015_ADC(I2CDevice i2cDevice, byte deviceId = ADS1X15_ADDRESS)
        {
            this._i2cDevice = i2cDevice;

            m_bitShift = 4;
            m_gain = adsGain_t.GAIN_TWOTHIRDS; /* +/- 6.144V range (limited to VDD +0.3V max!) */
            m_dataRate = RATE_ADS1015_1600SPS;
        }

        public bool Begin(byte deviceAddress = ADS1X15_ADDRESS)
        {
            try
            {
                this.DeviceID = deviceAddress;
                //if (!this._i2cDevice.InitiateDetectionSequence(deviceAddress))
                //    return false;

                return true;
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
                return false;
            }
        }

        bool startADCReading(uint16_t mux, bool continuous)
        {
            // Start with default values
            uint16_t config =
                ADS1X15_REG_CONFIG_CQUE_1CONV |   // Set CQUE to any value other than
                                                  // None so we can use it in RDY mode
                ADS1X15_REG_CONFIG_CLAT_NONLAT |  // Non-latching (default val)
                ADS1X15_REG_CONFIG_CPOL_ACTVLOW | // Alert/Rdy active low   (default val)
                ADS1X15_REG_CONFIG_CMODE_TRAD;    // Traditional comparator (default val)

            if (continuous)
            {
                config |= ADS1X15_REG_CONFIG_MODE_CONTIN;
            }
            else
            {
                config |= ADS1X15_REG_CONFIG_MODE_SINGLE;
            }

            // Set PGA/voltage range
            config |= (uint16_t)m_gain;

            // Set data rate
            config |= m_dataRate;

            // Set channels
            config |= mux;

            // Set 'start single-conversion' bit
            config |= ADS1X15_REG_CONFIG_OS_SINGLE;

            // Write config register to the ADC
            if (!writeRegister(ADS1X15_REG_POINTER_CONFIG, config)) return false;

            // Set ALERT/RDY to RDY mode.
            if (!writeRegister(ADS1X15_REG_POINTER_HITHRESH, 0x8000)) return false;
            if (!writeRegister(ADS1X15_REG_POINTER_LOWTHRESH, 0x0000)) return false;
            return true;
        }

        public int readADC_SingleEnded(uint8_t channel)
        {
            if (channel > 3)
                return -1;

            if (!startADCReading(MUX_BY_CHANNEL[channel], /*continuous=*/false)) return -1; 

            // Wait for the conversion to complete
            while (!conversionComplete())
            {
            }
            return getLastConversionResults();
        }

        int16_t getLastConversionResults()
        {
            // Read the conversion results
            uint16_t res = (uint8_t)(readRegister((uint8_t)ADS1X15_REG_POINTER_CONVERT) >> m_bitShift);
            if (m_bitShift == 0)
            {
                return (int16_t)res;
            }
            else
            {
                // Shift 12-bit results right 4 bits for the ADS1015,
                // making sure we keep the sign bit intact
                if (res > 0x07FF)
                {
                    // negative number - extend the sign to 16th bit
                    res |= 0xF000;
                }
                return (int16_t)res;
            }
        }

        bool conversionComplete()
        {
            return (readRegister(ADS1X15_REG_POINTER_CONFIG) & 0x8000) != 0;
        }

        private UInt16 read16(uint8_t reg)
        {
            //UInt16 value = 0;
            //var buffer = new byte[2]; // Allocate the response expected
            //if (Ii2cOutImpl.i2c_WriteReadBuffer(new byte[1] { reg }, buffer))
            //{
            //    value = (System.UInt16)((buffer[0] << 8) + buffer[1]);
            //}

            var r = this._i2cDevice.Write1ByteRead2Byte(reg);
            return (UInt16)r;
        }

        bool writeRegister(uint8_t reg, uint16_t value)
        {
            var buffer = new byte[3];
            buffer[0] = reg;
            buffer[1] = (byte)(value >> 8);
            buffer[2] = (byte)(value & 0xFF);
            return this._i2cDevice.WriteBuffer(buffer);
        }

        uint16_t readRegister(uint8_t reg)
        {
            var buffer = new byte[1];
            buffer[0] = reg;
            var r = this._i2cDevice.Write1ByteRead2Byte(reg);
            return (uint16_t)r;

            //m_i2c_dev->write(buffer, 1);
            //m_i2c_dev->read(buffer, 2);
            //return ((buffer[0] << 8) | buffer[1]);
        }
    }
}

