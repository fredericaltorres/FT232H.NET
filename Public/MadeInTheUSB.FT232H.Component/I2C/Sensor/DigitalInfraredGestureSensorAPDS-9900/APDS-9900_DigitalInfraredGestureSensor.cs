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
  
    Adafruit_APDS9960 - library - https://github.com/adafruit/Adafruit_MCP9808_Library
  
*/

/*!
 *  @file Adafruit_APDS9960.cpp
 *
 *  @mainpage Adafruit APDS9960 Proximity, Light, RGB, and Gesture Sensor
 *
 *  @section author Author
 *
 *  Ladyada, Dean Miller (Adafruit Industries)
 *
 *  @section license License
 *
 *  Software License Agreement (BSD License)
 *
 *  Copyright (c) 2017, Adafruit Industries
 *  All rights reserved.
 *
 *  Redistribution and use in source and binary forms, with or without
 *  modification, are permitted provided that the following conditions are met:
 *  1. Redistributions of source code must retain the above copyright
 *  notice, this list of conditions and the following disclaimer.
 *  2. Redistributions in binary form must reproduce the above copyright
 *  notice, this list of conditions and the following disclaimer in the
 *  documentation and/or other materials provided with the distribution.
 *  3. Neither the name of the copyright holders nor the
 *  names of its contributors may be used to endorse or promote products
 *  derived from this software without specific prior written permission.
 *
 *  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS ''AS IS'' AND ANY
 *  EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 *  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 *  DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER BE LIABLE FOR ANY
 *  DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 *  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 *  LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 *  ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 *  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 *  THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */


using MadeInTheUSB.FT232H;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using int16_t = System.Int16; // Nice C# feature allowing to use same Arduino/C type
using uint32_t = System.UInt32;
using uint16_t = System.UInt16;
using uint8_t = System.Byte;
using System.Threading;

namespace MadeInTheUSB
{
    /// <summary>
    /// https://cdn.sparkfun.com/assets/learn_tutorials/3/2/1/Avago-APDS-9960-datasheet.pdf
    /// </summary>
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


        public const int APDS9960_ADDRESS = (0x39); /**< I2C Address */
        public int DeviceID = APDS9960_ADDRESS;

        uint8_t gestCnt;
        uint8_t UCount;
        uint8_t DCount;
        uint8_t LCount;
        uint8_t RCount;

        I2CDevice_MPSSE_NotUsed _i2cDevice;
        private readonly int _gpioInterrupt;

        public APDS_9900_DigitalInfraredGestureSensor(I2CDevice_MPSSE_NotUsed i2cDevice, int gpioInterrupt)
        {
            this._i2cDevice = i2cDevice;
            this._gpioInterrupt = gpioInterrupt;
            this._i2cDevice.Gpios.SetPinMode(gpioInterrupt, PinMode.InputPullUp);
        }

        public bool IsInterruptOn
        {
            get
            {
                return this._i2cDevice.Gpios.DigitalRead(_gpioInterrupt) == 0;
            }
        }

        float powf(float x, float y) 
        {
             return (float) (Math.Pow((double) x, (double) y));
        }

        void delay(int val)
        {
            Thread.Sleep(val);
        }

        /*!
         *  @brief  Enables the device
         *          Disables the device (putting it in lower power sleep mode)
         *  @param  en
         *          Enable (True/False)
         */
        public void enable(bool en)
        {
            _enable.PON = (byte)(en ? 1:0);
            this.write8(Registers.APDS9960_ENABLE, _enable.get());
        }

        private bool VerifyDeviceConnection(int recusiveIndex = 0)
        {
            uint8_t x = read8(Registers.APDS9960_ID); /* Make sure we're actually connected */
            var r = (x == 0xAB);
            if(r == false && recusiveIndex == 0)
            {
                Thread.Sleep(10); // Wait and re-try
                return VerifyDeviceConnection(recusiveIndex+1);
            }

            return r;
        }

        /*!
         *  @brief  Initializes I2C and configures the sensor
         *  @param  iTimeMS
         *          Integration time
         *  @param  aGain
         *          Gain
         *  @param  addr
         *          I2C address
         *  @param  *theWire
         *          Wire object
         *  @return True if initialization was successful, otherwise false.
         */
        public bool begin(uint16_t iTimeMS = 10, apds9960AGain_t aGain = apds9960AGain_t.APDS9960_AGAIN_4X, uint8_t addr = APDS9960_ADDRESS)
        {
            this.DeviceID = addr;
            if (!this._i2cDevice.InitiateDetectionSequence(addr))
                return false;

            if (!VerifyDeviceConnection())
            {
                return false;
            }
          
            /* Set default integration time and gain */
            setADCIntegrationTime(iTimeMS);
            setADCGain(aGain);

            // disable everything to start
            enableGesture(false);
            enableProximity(false);
            enableColor(false);

            disableColorInterrupt();
            disableProximityInterrupt();
            clearInterrupt();

            /* Note: by default, the device is in power down mode on bootup */
            enable(false);
            delay(10);
            enable(true);
            delay(10);

            // default to all gesture dimensions
            setGestureDimensions((byte)Dimensions.APDS9960_DIMENSIONS_ALL);
            setGestureFIFOThreshold((byte)FIFOInterrupts.APDS9960_GFIFO_4);
            setGestureGain((byte)GestureGain.APDS9960_GGAIN_4);
            setGestureProximityThreshold(50);
            resetCounts();

            _gpulse.GPLEN = (byte)PulseLenghts.APDS9960_GPULSE_32US;
            _gpulse.GPULSE = 9; // 10 pulses
            this.write8(Registers.APDS9960_GPULSE, _gpulse.get());

            return true;
        }

        /*!
         *  @brief  Sets the integration time for the ADC of the APDS9960, in millis
         *  @param  iTimeMS
         *          Integration time
         */
        public void setADCIntegrationTime(uint16_t iTimeMS)
        {
            double temp;

            // convert ms into 2.78ms increments
            temp = iTimeMS;
            temp /= 2.78;
            temp = 256 - temp;
            if (temp > 255)
                temp = 255;
            if (temp < 0)
                temp = 0;

            /* Update the timing register */
            if(!write8(Registers.APDS9960_ATIME, (uint8_t)temp))
            {
                Thread.Sleep(1);
                write8(Registers.APDS9960_ATIME, (uint8_t)temp);
            }
        }

        /*!
         *  @brief  Returns the integration time for the ADC of the APDS9960, in millis
         *  @return Integration time
         */
        public double getADCIntegrationTime()
        {
            double temp;

            temp = read8(Registers.APDS9960_ATIME);

            // convert to units of 2.78 ms
            temp = 256 - temp;
            temp *= 2.78;
            return temp;
        }

        /*!
         *  @brief  Adjusts the color/ALS gain on the APDS9960 (adjusts the sensitivity
         *          to light)
         *  @param  aGain
         *          Gain
         */
        public void setADCGain(apds9960AGain_t aGain)
        {
            _control.AGAIN = (byte)aGain;

            /* Update the timing register */
            write8(Registers.APDS9960_CONTROL, _control.get());
        }

        /*!
         *  @brief  Returns the ADC gain
         *  @return ADC gain
         */
        public apds9960AGain_t getADCGain()
        {
            return (apds9960AGain_t)(read8(Registers.APDS9960_CONTROL) & 0x03);
        }

        /*!
         *  @brief  Adjusts the Proximity gain on the APDS9960
         *  @param  pGain
         *          Gain
         */
        public void setProxGain(apds9960PGain_t pGain)
        {
            _control.PGAIN = (byte)pGain;

            /* Update the timing register */
            write8(Registers.APDS9960_CONTROL, _control.get());
        }

        /*!
         *  @brief  Returns the Proximity gain on the APDS9960
         *  @return Proxmity gain
         */
        public apds9960PGain_t getProxGain()
        {
            return (apds9960PGain_t)((read8(Registers.APDS9960_CONTROL) & 0x0C) >> 2);
        }

        /*!
         *  @brief  Sets number of proxmity pulses
         *  @param  pLen
         *          Pulse Length
         *  @param  pulses
         *          Number of pulses
         */
        public void setProxPulse(apds9960PPulseLen_t pLen, uint8_t pulses)
        {
            if (pulses < 1)
                pulses = 1;
            if (pulses > 64)
                pulses = 64;
            pulses--;

            _ppulse.PPLEN = (byte)pLen;
            _ppulse.PPULSE = pulses;

            write8(Registers.APDS9960_PPULSE, _ppulse.get());
        }

        /*!
         *  @brief  Enable proximity readings on APDS9960
         *  @param  en
         *          Enable (True/False)
         */
        public void enableProximity(bool en)
        {
            _enable.PEN = (byte)(en ? 1: 0);

            write8(Registers.APDS9960_ENABLE, _enable.get());
        }

        /*!
         *  @brief  Enable proximity interrupts
         */
        public void enableProximityInterrupt()
        {
            _enable.PIEN = 1;
            write8(Registers.APDS9960_ENABLE, _enable.get());
            clearInterrupt();
        }

        /*!
         *  @brief  Disable proximity interrupts
         */
        public void disableProximityInterrupt()
        {
            _enable.PIEN = 0;
            write8(Registers.APDS9960_ENABLE, _enable.get());
        }

        /*!
         *  @brief  Set proxmity interrupt thresholds
         *  @param  low
         *          Low threshold
         *  @param  high
         *          High threshold
         *  @param  persistence
         *          Persistence
         */
        public void setProximityInterruptThreshold(uint8_t low, uint8_t high, uint8_t persistence = 4)
        {
            write8(Registers.APDS9960_PILT, low);
            write8(Registers.APDS9960_PIHT, high);

            if (persistence > 7)
                persistence = 7;
            _pers.PPERS = persistence;
            write8(Registers.APDS9960_PERS, _pers.get());
        }

        /*!
         *  @brief  Returns proximity interrupt status
         *  @return True if enabled, false otherwise.
         */
        public bool getProximityInterrupt()
        {
            _status.set(this.read8(Registers.APDS9960_STATUS));
            return _status.PINT == 1;
        }

        /*!
         *  @brief  Read proximity data
         *  @return Proximity
         */
        public uint8_t readProximity() { return read8(Registers.APDS9960_PDATA); }

        /*!
         *  @brief  Returns validity status of a gesture
         *  @return Status (True/False)
         */
        public bool gestureValid()
        {
            _gstatus.set(this.read8(Registers.APDS9960_GSTATUS));
            return _gstatus.GVALID == 1;
        }

        /*!
         *  @brief  Sets gesture dimensions
         *  @param  dims
         *          Dimensions (APDS9960_DIMENSIONS_ALL, APDS9960_DIMENSIONS_UP_DOWN,
         *          APGS9960_DIMENSIONS_LEFT_RIGHT)
         */
        public void setGestureDimensions(uint8_t dims)
        {
            _gconf3.GDIMS = dims;
            this.write8(Registers.APDS9960_GCONF3, _gconf3.get());
        }

        /*!
         *  @brief  Sets gesture FIFO Threshold
         *  @param  thresh
         *          Threshold (APDS9960_GFIFO_1, APDS9960_GFIFO_4, APDS9960_GFIFO_8,
         *          APDS9960_GFIFO_16)
         */
        public void setGestureFIFOThreshold(uint8_t thresh)
        {
            _gconf1.GFIFOTH = thresh;
            this.write8(Registers.APDS9960_GCONF1, _gconf1.get());
        }

        /*!
         *  @brief  Sets gesture sensor gain
         *  @param  gain
         *          Gain (APDS9960_GAIN_1, APDS9960_GAIN_2, APDS9960_GAIN_4,
         *          APDS9960_GAIN_8)
         */
        public void setGestureGain(uint8_t gain)
        {
            _gconf2.GGAIN = gain;
            this.write8(Registers.APDS9960_GCONF2, _gconf2.get());
        }

        /*!
         *  @brief  Sets gesture sensor threshold
         *  @param  thresh
         *          Threshold
         */
        public void setGestureProximityThreshold(uint8_t thresh)
        {
            this.write8(Registers.APDS9960_GPENTH, thresh);
        }

        /*!
         *  @brief  Sets gesture sensor offset
         *  @param  offset_up
         *          Up offset
         *  @param  offset_down
         *          Down offset
         *  @param  offset_left
         *          Left offset
         *  @param  offset_right
         *          Right offset
         */
        public void setGestureOffset(uint8_t offset_up, uint8_t offset_down,
                                                 uint8_t offset_left,
                                                 uint8_t offset_right)
        {
            this.write8(Registers.APDS9960_GOFFSET_U, offset_up);
            this.write8(Registers.APDS9960_GOFFSET_D, offset_down);
            this.write8(Registers.APDS9960_GOFFSET_L, offset_left);
            this.write8(Registers.APDS9960_GOFFSET_R, offset_right);
        }

        /*!
         *  @brief  Enable gesture readings on APDS9960
         *  @param  en
         *          Enable (True/False)
         */
        public void enableGesture(bool en)
        {
            if (!en)
            {
                _gconf4.GMODE = 0;
                write8(Registers.APDS9960_GCONF4, _gconf4.get());
            }
            _enable.GEN = (byte)(en ? 1 : 0);
            write8(Registers.APDS9960_ENABLE, _enable.get());
            resetCounts();
        }

        /*!
         *  @brief  Resets gesture counts
         */
        public void resetCounts()
        {
            gestCnt = 0;
            UCount = 0;
            DCount = 0;
            LCount = 0;
            RCount = 0;
        }

        /*!
         *  @brief  Reads gesture
         *  @return Received gesture (APDS9960_DOWN APDS9960_UP, APDS9960_LEFT
         *          APDS9960_RIGHT)
         */
        public Gestures readGesture()
        {
            uint8_t toRead;
            List<uint8_t> buf = new List<uint8_t>();
            int t = 0;
            Gestures gestureReceived;
            while (true)
            {
                int up_down_diff = 0;
                int left_right_diff = 0;
                gestureReceived = 0;
                if (!gestureValid())
                    return 0;

                delay(30);
                toRead = this.read8(Registers.APDS9960_GFLVL);

                // produces sideffects needed for readGesture to work
                this.read(Registers.APDS9960_GFIFO_U, buf, toRead);

                var v1 = Math.Abs((int)buf[0] - (int)buf[1]);
                if (v1 > 13)
                    up_down_diff += (int)buf[0] - (int)buf[1];

                var v2 = Math.Abs((int)buf[2] - (int)buf[3]);
                if (v2 > 13)
                    left_right_diff += (int)buf[2] - (int)buf[3];

                if (up_down_diff != 0)
                {
                    if (up_down_diff < 0)
                    {
                        if (DCount > 0)
                            gestureReceived = Gestures.APDS9960_UP;
                        else
                            UCount++;
                    }
                    else if (up_down_diff > 0)
                    {
                        if (UCount > 0)
                            gestureReceived = Gestures.APDS9960_DOWN;
                        else
                            DCount++;
                    }
                }

                if (left_right_diff != 0)
                {
                    if (left_right_diff < 0)
                    {
                        if (RCount > 0)
                        {
                            gestureReceived = Gestures.APDS9960_LEFT;
                        }
                        else
                            LCount++;
                    }
                    else if (left_right_diff > 0)
                    {
                        if (LCount > 0)
                        {
                            gestureReceived = Gestures.APDS9960_RIGHT;
                        }
                        else
                            RCount++;
                    }
                }

                if (up_down_diff != 0 || left_right_diff != 0)
                    t = Environment.TickCount;

                if (gestureReceived > 0 || Environment.TickCount - t > 300)
                {
                    resetCounts();
                    return gestureReceived;
                }
            }
        }

        /*!
         *  @brief  Set LED brightness for proximity/gesture
         *  @param  drive
         *          LED Drive
         *  @param  boost
         *          LED Boost
         */
        public void setLED(apds9960LedDrive_t drive, apds9960LedBoost_t boost)
        {
            // set BOOST
            _config2.LED_BOOST = (byte)boost;
            write8(Registers.APDS9960_CONFIG2, _config2.get());

            _control.LDRIVE = (byte)drive;
            write8(Registers.APDS9960_CONTROL, _control.get());
        }

        /*!
         *  @brief  Enable color readings on APDS9960
         *  @param  en
         *          Enable (True/False)
         */
        public void enableColor(bool en)
        {
            _enable.AEN = (byte)(en?1:0);
            write8(Registers.APDS9960_ENABLE, _enable.get());
        }

        /*!
         *  @brief  Returns status of color data
         *  @return True if color data ready, False otherwise
         */
        public bool colorDataReady()
        {
            _status.set(this.read8(Registers.APDS9960_STATUS));
            return _status.AVALID == 1;
        }

        /*!
         *  @brief  Reads the raw red, green, blue and clear channel values
         *  @param  *r
         *          Red value
         *  @param  *g
         *          Green value
         *  @param  *b
         *          Blue value
         *  @param  *c
         *          Clear channel value
         */
        public void getColorData(ref uint16_t r, ref uint16_t g, ref uint16_t b, ref uint16_t c)
        {
            c = read16R(Registers.APDS9960_CDATAL);
            r = read16R(Registers.APDS9960_RDATAL);
            g = read16R(Registers.APDS9960_GDATAL);
            b = read16R(Registers.APDS9960_BDATAL);
        }

        /*!
         *  @brief  Converts the raw R/G/B values to color temperature in degrees Kelvin
         *  @param  r
         *          Red value
         *  @param  g
         *          Green value
         *  @param  b
         *          Blue value
         *  @return Color temperature
         */
        public uint16_t calculateColorTemperature(uint16_t r, uint16_t g,
                                                              uint16_t b)
        {
            float X, Y, Z; /* RGB to XYZ correlation      */
            float xc, yc;  /* Chromaticity co-ordinates   */
            float n;       /* McCamy's formula            */
            float cct;

            /* 1. Map RGB values to their XYZ counterparts.    */
            /* Based on 6500K fluorescent, 3000K fluorescent   */
            /* and 60W incandescent values for a wide range.   */
            /* Note: Y = Illuminance or lux                    */
            X = (-0.14282F * r) + (1.54924F * g) + (-0.95641F * b);
            Y = (-0.32466F * r) + (1.57837F * g) + (-0.73191F * b);
            Z = (-0.68202F * r) + (0.77073F * g) + (0.56332F * b);

            /* 2. Calculate the chromaticity co-ordinates      */
            xc = (X) / (X + Y + Z);
            yc = (Y) / (X + Y + Z);

            /* 3. Use McCamy's formula to determine the CCT    */
            n = (xc - 0.3320F) / (0.1858F - yc);

            /* Calculate the final CCT */
            cct =
                (449.0F * powf(n, 3)) + (3525.0F * powf(n, 2)) + (6823.3F * n) + 5520.33F;

            /* Return the results in degrees Kelvin */
            return (uint16_t)cct;
        }

        /*!
         *  @brief  Calculate ambient light values
         *  @param  r
         *          Red value
         *  @param  g
         *          Green value
         *  @param  b
         *          Blue value
         *  @return LUX value
         */
        public uint16_t calculateLux(uint16_t r, uint16_t g, uint16_t b)
        {
            float illuminance;

            /* This only uses RGB ... how can we integrate clear or calculate lux */
            /* based exclusively on clear since this might be more reliable?      */
            illuminance = (-0.32466F * r) + (1.57837F * g) + (-0.73191F * b);

            return (uint16_t)illuminance;
        }

        /*!
         *  @brief  Enables color interrupt
         */
        public void enableColorInterrupt()
        {
            _enable.AIEN = 1;
            write8(Registers.APDS9960_ENABLE, _enable.get());
        }

        /*!
         *  @brief  Disables color interrupt
         */
        public void disableColorInterrupt()
        {
            _enable.AIEN = 0;
            write8(Registers.APDS9960_ENABLE, _enable.get());
        }

        /*!
         *  @brief  Clears interrupt
         */
        public void clearInterrupt()
        {
            if (!this.write(Registers.APDS9960_AICLEAR, null, 0))
            {
                Thread.Sleep(1);
                this.write(Registers.APDS9960_AICLEAR, null, 0);
            }
        }

        /*!
         *  @brief  Sets interrupt limits
         *  @param  low
         *          Low limit
         *  @param  high
         *          High limit
         */
        public void setIntLimits(uint16_t low, uint16_t high)
        {
            write8(Registers.APDS9960_AILTIL, low & 0xFF);
            write8(Registers.APDS9960_AILTH, low >> 8);
            write8(Registers.APDS9960_AIHTL, high & 0xFF);
            write8(Registers.APDS9960_AIHTH, high >> 8);
        }

        /*!
         *  @brief  Writes specified value to given register
         *  @param  reg
         *          Register to write to
         *  @param  value
         *          Value to write
         */
        //void write8(byte reg, byte value)
        //{
        //    this.write(reg, &value, 1);
        //}

        /*!
         *  @brief  Reads 8 bits from specified register
         *  @param  reg
         *          Register to write to
         *  @return Value in register
         */
        //uint8_t read8(byte reg)
        //{
        //    uint8_t ret;
        //    this.read(reg, &ret, 1);

        //    return ret;
        //}

        /*!
         *  @brief  Reads 32 bits from specified register
         *  @param  reg
         *          Register to write to
         *  @return Value in register
         */
        public uint32_t read32(uint8_t reg)
        {
            //uint8_t ret[4];
            //uint32_t ret32;
            //this.read(reg, ret, 4);
            //ret32 = ret[3];
            //ret32 |= (uint32_t)ret[2] << 8;
            //ret32 |= (uint32_t)ret[1] << 16;
            //ret32 |= (uint32_t)ret[0] << 24;
            //return ret32;
            return 0;
        }

        /*!
         *  @brief  Reads 16 bites from specified register
         *  @param  reg
         *          Register to write to
         *  @return Value in register
         */
        uint16_t read16(uint8_t reg)
        {
            var ret = new List<byte>();
            this.read(reg, ret, 2);

            return (uint16_t)((ret[0] << 8) | ret[1]);
        }

        uint16_t read16R(Registers reg)
        {
            return read16R((byte)reg);
        }
        /*!
         *  @brief  Reads 16 bites from specified register
         *  @param  reg
         *          Register to write to
         *  @return Value in register
         */
        uint16_t read16R(uint8_t reg)
        {
            var ret = new List<byte>();
            this.read(reg, ret, 2);

            return (uint16_t)((ret[1] << 8) | ret[0]);
        }

        uint8_t read(Registers reg, List<byte> buf, uint8_t num)
        {
            return read((byte)reg, buf, num);
        }

        /*!
         *  @brief  Reads num bytes from specified register into a given buffer
         *  @param  reg
         *          Register
         *  @param  *buf
         *          Buffer
         *  @param  num
         *          Number of bytes
         *  @return Position after reading
         */
        uint8_t read(uint8_t reg, List<byte> buf, uint8_t num, int recusiveIndex = 0)
        {
            var newBuf = this._i2cDevice.Send1ByteReadXByteCommand(this.DeviceID, reg, num);

            if(newBuf == null && recusiveIndex == 0) // Retry once
            {
                Thread.Sleep(10);
                return read(reg, buf, num, recusiveIndex+1);
            }

            buf.AddRange(newBuf);
            return num;
        }

        bool write(Registers reg, List<byte> buf, uint8_t num)
        {
            return write((byte)reg, buf, num);
        }

        /*!
         *  @brief  Writes num bytes from specified buffer into a given register
         *  @param  reg
         *          Register
         *  @param  *buf
         *          Buffer
         *  @param  num
         *          Number of bytes
         */
        bool write(uint8_t reg, List<byte> buf, uint8_t num)
        {
            //uint8_t prefix[1] = { reg };
            //i2c_dev->write(buf, num, true, prefix, 1);
            var l = new List<byte>() { reg };
            if(buf != null)
                l.AddRange(buf);
            return this._i2cDevice.WriteBuffer(this.DeviceID, l.ToArray());
        }
    }
}

