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
using uint16_t = System.UInt16;
using uint8_t = System.Byte;


namespace MadeInTheUSB
{
    public partial class APDS_9900_DigitalInfraredGestureSensor
    {
        /** ADC gain settings */
        public enum apds9960AGain_t
        {
            APDS9960_AGAIN_1X = 0x00,  /**< No gain */
            APDS9960_AGAIN_4X = 0x01,  /**< 2x gain */
            APDS9960_AGAIN_16X = 0x02, /**< 16x gain */
            APDS9960_AGAIN_64X = 0x03  /**< 64x gain */
        }

        /** Proxmity gain settings */
        public enum apds9960PGain_t
        {
            APDS9960_PGAIN_1X = 0x00, /**< 1x gain */
            APDS9960_PGAIN_2X = 0x01, /**< 2x gain */
            APDS9960_PGAIN_4X = 0x02, /**< 4x gain */
            APDS9960_PGAIN_8X = 0x03  /**< 8x gain */
        }


        /** Pulse length settings */
        public enum apds9960PPulseLen_t
        {
            APDS9960_PPULSELEN_4US = 0x00,  /**< 4uS */
            APDS9960_PPULSELEN_8US = 0x01,  /**< 8uS */
            APDS9960_PPULSELEN_16US = 0x02, /**< 16uS */
            APDS9960_PPULSELEN_32US = 0x03  /**< 32uS */
        }

        /** LED drive settings */
        public enum apds9960LedDrive_t
        {
            APDS9960_LEDDRIVE_100MA = 0x00, /**< 100mA */
            APDS9960_LEDDRIVE_50MA = 0x01,  /**< 50mA */
            APDS9960_LEDDRIVE_25MA = 0x02,  /**< 25mA */
            APDS9960_LEDDRIVE_12MA = 0x03   /**< 12.5mA */
        }

        /** LED boost settings */
        public enum apds9960LedBoost_t
        {
            APDS9960_LEDBOOST_100PCNT = 0x00, /**< 100% */
            APDS9960_LEDBOOST_150PCNT = 0x01, /**< 150% */
            APDS9960_LEDBOOST_200PCNT = 0x02, /**< 200% */
            APDS9960_LEDBOOST_300PCNT = 0x03  /**< 300% */
        }

        /** Dimensions */
        public enum Dimensions
        {
            APDS9960_DIMENSIONS_ALL = 0x00,        // All dimensions
            APDS9960_DIMENSIONS_UP_DOWN = 0x01,    // Up/Down dimensions
            APGS9960_DIMENSIONS_LEFT_RIGHT = 0x02, // Left/Right dimensions
        };

        /** FIFO Interrupts */
        public enum FIFOInterrupts
        {
            APDS9960_GFIFO_1 = 0x00,  // Generate interrupt after 1 dataset in FIFO
            APDS9960_GFIFO_4 = 0x01,  // Generate interrupt after 2 datasets in FIFO
            APDS9960_GFIFO_8 = 0x02,  // Generate interrupt after 3 datasets in FIFO
            APDS9960_GFIFO_16 = 0x03, // Generate interrupt after 4 datasets in FIFO
        };

        /** Gesture Gain */
        public enum GestureGain
        {
            APDS9960_GGAIN_1 = 0x00, // Gain 1x
            APDS9960_GGAIN_2 = 0x01, // Gain 2x
            APDS9960_GGAIN_4 = 0x02, // Gain 4x
            APDS9960_GGAIN_8 = 0x03, // Gain 8x
        };

        /** Pulse Lenghts */
        public enum PulseLenghts
        {
            APDS9960_GPULSE_4US = 0x00,  // Pulse 4us
            APDS9960_GPULSE_8US = 0x01,  // Pulse 8us
            APDS9960_GPULSE_16US = 0x02, // Pulse 16us
            APDS9960_GPULSE_32US = 0x03, // Pulse 32us
        };

        public enum Gestures
        {
            UNDEFINED = 0,
            APDS9960_UP = 0x01,   /**< Gesture Up */
            APDS9960_DOWN = 0x02, /**< Gesture Down */
            APDS9960_LEFT = 0x03, /**< Gesture Left */
            APDS9960_RIGHT = 0x04, /**< Gesture Right */
        }

        class EnableData
        {
            // power on
            public uint8_t PON = 1;

            // ALS enable
            public uint8_t AEN = 1;

            // Proximity detect enable
            public uint8_t PEN = 1;

            // wait timer enable
            public uint8_t WEN = 1;

            // ALS interrupt enable
            public uint8_t AIEN = 1;

            // proximity interrupt enable
            public uint8_t PIEN = 1;

            // gesture enable
            public uint8_t GEN = 1;

            public uint8_t get()
            {
                return (byte)((GEN << 6) | (PIEN << 5) | (AIEN << 4) | (WEN << 3) | (PEN << 2) | (AEN << 1) | PON);
            }
        };

        EnableData _enable = new EnableData();

        class Pers
        {
            // ALS Interrupt Persistence. Controls rate of Clear channel interrupt to
            // the host processor
            public uint8_t APERS = 4;

            // proximity interrupt persistence, controls rate of prox interrupt to host
            // processor
            public uint8_t PPERS = 4;

            public uint8_t get()
            {
                return (byte)((PPERS << 4) | APERS);
            }
        };
        Pers _pers = new Pers();

        class Config1
        {
            public uint8_t WLONG = 1;

            public uint8_t get() { return (byte)(WLONG << 1); }
        };
        Config1 _config1 = new Config1();

        class Ppulse
        {

            /*Proximity Pulse Count. Specifies the number of proximity pulses to be
            generated on LDR. Number of pulses is set by PPULSE value plus 1.
            */
            public uint8_t PPULSE = 6;

            // Proximity Pulse Length. Sets the LED-ON pulse width during a proximity
            // LDR pulse.
            public uint8_t PPLEN = 2;

            public uint8_t get() { return (byte)((PPLEN << 6) | PPULSE); }
        };
        Ppulse _ppulse = new Ppulse();

        public class Control
        {
            // ALS and Color gain control
            public uint8_t AGAIN = 2;

            // proximity gain control
            public uint8_t PGAIN = 2;

            // led drive strength
            public uint8_t LDRIVE = 2;

            public uint8_t get() { return (byte)((LDRIVE << 6) | (PGAIN << 2) | AGAIN); }
        };
        Control _control = new Control();

        class Config2
        {
            /* Additional LDR current during proximity and gesture LED pulses. Current
            value, set by LDRIVE, is increased by the percentage of LED_BOOST.
            */
            public uint8_t LED_BOOST = 2;

            // clear photodiode saturation int enable
            public uint8_t CPSIEN = 1;

            // proximity saturation interrupt enable
            public uint8_t PSIEN = 1;

            public uint8_t get()
            {
                return (byte)((PSIEN << 7) | (CPSIEN << 6) | (LED_BOOST << 4) | 1);
            }
        };
        Config2 _config2 = new Config2();


        class Status
        {
            /* ALS Valid. Indicates that an ALS cycle has completed since AEN was
            asserted or since a read from any of the ALS/Color data registers.
            */
            public uint8_t AVALID = 1;

            /* Proximity Valid. Indicates that a proximity cycle has completed since PEN
            was asserted or since PDATA was last read. A read of PDATA automatically
            clears PVALID.
            */
            public uint8_t PVALID = 1;

            /* Gesture Interrupt. GINT is asserted when GFVLV becomes greater than
            GFIFOTH or if GVALID has become asserted when GMODE transitioned to zero.
            The bit is reset when FIFO is completely emptied (read).
            */
            public uint8_t GINT = 1;

            // ALS Interrupt. This bit triggers an interrupt if AIEN in ENABLE is set.
            public uint8_t AINT = 1;

            // Proximity Interrupt. This bit triggers an interrupt if PIEN in ENABLE is
            // set.
            public uint8_t PINT = 1;

            /* Indicates that an analog saturation event occurred during a previous
            proximity or gesture cycle. Once set, this bit remains set until cleared by
            clear proximity interrupt special function command (0xE5 PICLEAR) or by
            disabling Prox (PEN=0). This bit triggers an interrupt if PSIEN is set.
            */
            public uint8_t PGSAT = 1;

            /* Clear Photodiode Saturation. When asserted, the analog sensor was at the
            upper end of its dynamic range. The bit can be de-asserted by sending a
            Clear channel interrupt command (0xE6 CICLEAR) or by disabling the ADC
            (AEN=0). This bit triggers an interrupt if CPSIEN is set.
            */
            public uint8_t CPSAT = 1;

            public void set(uint8_t data)
            {
                AVALID = (byte)(data & 0x01);
                PVALID = (byte)((data >> 1) & 0x01);
                GINT = (byte)((data >> 2) & 0x01);
                AINT = (byte)((data >> 4) & 0x01);
                PINT = (byte)((data >> 5) & 0x01);
                PGSAT = (byte)((data >> 6) & 0x01);
                CPSAT = (byte)((data >> 7) & 0x01);
            }
        };
        Status _status = new Status();


        class Config3
        {
            // proximity mask
            public uint8_t PMASK_R = 1;
            public uint8_t PMASK_L = 1;
            public uint8_t PMASK_D = 1;
            public uint8_t PMASK_U = 1;

            /* Sleep After Interrupt. When enabled, the device will automatically enter
            low power mode when the INT pin is asserted and the state machine has
            progressed to the SAI decision block. Normal operation is resumed when INT
            pin is cleared over I2C.
            */
            public uint8_t SAI = 1;

            /* Proximity Gain Compensation Enable. This bit provides gain compensation
            when proximity photodiode signal is reduced as a result of sensor masking.
            If only one diode of the diode pair is contributing, then only half of the
            signal is available at the ADC; this results in a maximum ADC value of 127.
            Enabling PCMP enables an additional gain of 2X, resulting in a maximum ADC
            value of 255.
            */
            public uint8_t PCMP = 1;

            public uint8_t get()
            {
                return (byte)((PCMP << 5) | (SAI << 4) | (PMASK_U << 3) | (PMASK_D << 2) | (PMASK_L << 1) | PMASK_R);
            }
        };
        Config3 _config3 = new Config3();


        class Gconf1
        {
            /* Gesture Exit Persistence. When a number of consecutive ?gesture end?
            occurrences become equal or greater to the GEPERS value, the Gesture state
            machine is exited.
            */
            public uint8_t GEXPERS = 2;

            /* Gesture Exit Mask. Controls which of the gesture detector photodiodes
            (UDLR) will be included to determine a ?gesture end? and subsequent exit
            of the gesture state machine. Unmasked UDLR data will be compared with the
            value in GTHR_OUT. Field value bits correspond to UDLR detectors.
            */
            public uint8_t GEXMSK = 4;

            /* Gesture FIFO Threshold. This value is compared with the FIFO Level (i.e.
            the number of UDLR datasets) to generate an interrupt (if enabled).
            */
            public uint8_t GFIFOTH = 2;

            public uint8_t get() { return (byte)((GFIFOTH << 6) | (GEXMSK << 2) | GEXPERS); }
        };
        Gconf1 _gconf1 = new Gconf1();

        class Gconf2
        {
            /* Gesture Wait Time. The GWTIME controls the amount of time in a low power
            mode between gesture detection cycles.
            */
            public uint8_t GWTIME = 3;

            // Gesture LED Drive Strength. Sets LED Drive Strength in gesture mode.
            public uint8_t GLDRIVE = 2;

            // Gesture Gain Control. Sets the gain of the proximity receiver in gesture
            // mode.
            public uint8_t GGAIN = 2;

            public uint8_t get() { return (byte)((GGAIN << 5) | (GLDRIVE << 3) | GWTIME); }
        };
        Gconf2 _gconf2 = new Gconf2();

        class Gpulse
        {
            /* Number of Gesture Pulses. Specifies the number of pulses to be generated
            on LDR. Number of pulses is set by GPULSE value plus 1.
            */
            public uint8_t GPULSE = 6;

            // Gesture Pulse Length. Sets the LED_ON pulse width during a Gesture LDR
            // Pulse.
            public uint8_t GPLEN = 2;

            public uint8_t get() { return (byte)((GPLEN << 6) | GPULSE); }
        };
        Gpulse _gpulse = new Gpulse();

        class Gconf3
        {
            /* Gesture Dimension Select. Selects which gesture photodiode pairs are
            enabled to gather results during gesture.
            */
            public uint8_t GDIMS = 2;

            public uint8_t get() { return GDIMS; }
        };
        Gconf3 _gconf3 = new Gconf3();
        class Gconf4
        {
            /* Gesture Mode. Reading this bit reports if the gesture state machine is
            actively running, 1 = Gesture, 0= ALS, Proximity, Color. Writing a 1 to this
            bit causes immediate entry in to the gesture state machine (as if GPENTH had
            been exceeded). Writing a 0 to this bit causes exit of gesture when current
            analog conversion has finished (as if GEXTH had been exceeded).
            */
            public uint8_t GMODE = 1;

            /* Gesture interrupt enable. Gesture Interrupt Enable. When asserted, all
            gesture related interrupts are unmasked.
            */
            public uint8_t GIEN = 2;

            public uint8_t get() { return (byte)((GIEN << 1) | GMODE); }
            void set(uint8_t data)
            {
                GIEN = (byte)((data >> 1) & 0x01);
                GMODE = (byte)(data & 0x01);
            }
        };
        Gconf4 _gconf4 = new Gconf4();

        class Gstatus
        {
            /* Gesture FIFO Data. GVALID bit is sent when GFLVL becomes greater than
            GFIFOTH (i.e. FIFO has enough data to set GINT). GFIFOD is reset when GMODE
            = 0 and the GFLVL=0 (i.e. All FIFO data has been read).
            */
            public uint8_t GVALID = 1;

            /* Gesture FIFO Overflow. A setting of 1 indicates that the FIFO has filled
            to capacity and that new gesture detector data has been lost.
            */
            public uint8_t GFOV = 1;

            public void set(uint8_t data)
            {
                GFOV = (byte)((data >> 1) & 0x01);
                GVALID = (byte)(data & 0x01);
            }
        };
        Gstatus _gstatus = new Gstatus();

        private byte ReadRegister(Registers reg)
        {
            return ReadRegister((byte)reg);
        }

        private byte ReadRegister(byte reg)
        {
            var r = this._i2cDevice.ReadByteRegister((byte)reg, this.DeviceID);
            return (byte)r;
        }

        private bool WriteRegister(Registers reg, int value)
        {
            return WriteRegister((byte)reg, (byte)value);
        }

        private bool WriteRegister(byte reg, byte value)
        {
            return this._i2cDevice.WriteByteRegister(reg, value, this.DeviceID);
        }
    }
}

