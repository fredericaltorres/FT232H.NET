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
        class Enable
        {
            // power on
            uint8_t PON = 1;

            // ALS enable
            uint8_t AEN = 1;

            // Proximity detect enable
            uint8_t PEN = 1;

            // wait timer enable
            uint8_t WEN = 1;

            // ALS interrupt enable
            uint8_t AIEN = 1;

            // proximity interrupt enable
            uint8_t PIEN = 1;

            // gesture enable
            uint8_t GEN = 1;

            uint8_t get()
            {
                return (byte)((GEN << 6) | (PIEN << 5) | (AIEN << 4) | (WEN << 3) | (PEN << 2) | (AEN << 1) | PON);
            }
        };

        Enable _enable = new Enable();

        class Pers
        {
            // ALS Interrupt Persistence. Controls rate of Clear channel interrupt to
            // the host processor
            uint8_t APERS = 4;

            // proximity interrupt persistence, controls rate of prox interrupt to host
            // processor
            uint8_t PPERS = 4;

            uint8_t get()
            {
                return (byte)((PPERS << 4) | APERS);
            }
        };
        Pers _pers = new Pers();

        class Config1
        {
            uint8_t WLONG = 1;

            uint8_t get() { return (byte)(WLONG << 1); }
        };
        Config1 _config1 = new Config1();

        class Ppulse
        {

            /*Proximity Pulse Count. Specifies the number of proximity pulses to be
            generated on LDR. Number of pulses is set by PPULSE value plus 1.
            */
            uint8_t PPULSE = 6;

            // Proximity Pulse Length. Sets the LED-ON pulse width during a proximity
            // LDR pulse.
            uint8_t PPLEN = 2;

            uint8_t get() { return (byte)((PPLEN << 6) | PPULSE); }
        };
        Ppulse _ppulse = new Ppulse();

        public class Control
        {
            // ALS and Color gain control
            uint8_t AGAIN = 2;

            // proximity gain control
            uint8_t PGAIN = 2;

            // led drive strength
            uint8_t LDRIVE = 2;

            uint8_t get() { return (byte)((LDRIVE << 6) | (PGAIN << 2) | AGAIN); }
        };
        Control _control = new Control();

        class Config2
        {
            /* Additional LDR current during proximity and gesture LED pulses. Current
            value, set by LDRIVE, is increased by the percentage of LED_BOOST.
            */
            uint8_t LED_BOOST = 2;

            // clear photodiode saturation int enable
            uint8_t CPSIEN = 1;

            // proximity saturation interrupt enable
            uint8_t PSIEN = 1;

            uint8_t get()
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
            uint8_t AVALID = 1;

            /* Proximity Valid. Indicates that a proximity cycle has completed since PEN
            was asserted or since PDATA was last read. A read of PDATA automatically
            clears PVALID.
            */
            uint8_t PVALID = 1;

            /* Gesture Interrupt. GINT is asserted when GFVLV becomes greater than
            GFIFOTH or if GVALID has become asserted when GMODE transitioned to zero.
            The bit is reset when FIFO is completely emptied (read).
            */
            uint8_t GINT = 1;

            // ALS Interrupt. This bit triggers an interrupt if AIEN in ENABLE is set.
            uint8_t AINT = 1;

            // Proximity Interrupt. This bit triggers an interrupt if PIEN in ENABLE is
            // set.
            uint8_t PINT = 1;

            /* Indicates that an analog saturation event occurred during a previous
            proximity or gesture cycle. Once set, this bit remains set until cleared by
            clear proximity interrupt special function command (0xE5 PICLEAR) or by
            disabling Prox (PEN=0). This bit triggers an interrupt if PSIEN is set.
            */
            uint8_t PGSAT = 1;

            /* Clear Photodiode Saturation. When asserted, the analog sensor was at the
            upper end of its dynamic range. The bit can be de-asserted by sending a
            Clear channel interrupt command (0xE6 CICLEAR) or by disabling the ADC
            (AEN=0). This bit triggers an interrupt if CPSIEN is set.
            */
            uint8_t CPSAT = 1;

            void set(uint8_t data)
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
            uint8_t PMASK_R = 1;
            uint8_t PMASK_L = 1;
            uint8_t PMASK_D = 1;
            uint8_t PMASK_U = 1;

            /* Sleep After Interrupt. When enabled, the device will automatically enter
            low power mode when the INT pin is asserted and the state machine has
            progressed to the SAI decision block. Normal operation is resumed when INT
            pin is cleared over I2C.
            */
            uint8_t SAI = 1;

            /* Proximity Gain Compensation Enable. This bit provides gain compensation
            when proximity photodiode signal is reduced as a result of sensor masking.
            If only one diode of the diode pair is contributing, then only half of the
            signal is available at the ADC; this results in a maximum ADC value of 127.
            Enabling PCMP enables an additional gain of 2X, resulting in a maximum ADC
            value of 255.
            */
            uint8_t PCMP = 1;

            uint8_t get()
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
            uint8_t GEXPERS = 2;

            /* Gesture Exit Mask. Controls which of the gesture detector photodiodes
            (UDLR) will be included to determine a ?gesture end? and subsequent exit
            of the gesture state machine. Unmasked UDLR data will be compared with the
            value in GTHR_OUT. Field value bits correspond to UDLR detectors.
            */
            uint8_t GEXMSK = 4;

            /* Gesture FIFO Threshold. This value is compared with the FIFO Level (i.e.
            the number of UDLR datasets) to generate an interrupt (if enabled).
            */
            uint8_t GFIFOTH = 2;

            uint8_t get() { return (byte)((GFIFOTH << 6) | (GEXMSK << 2) | GEXPERS); }
        };
        Gconf1 _gconf1 = new Gconf1();

        class Gconf2
        {
            /* Gesture Wait Time. The GWTIME controls the amount of time in a low power
            mode between gesture detection cycles.
            */
            uint8_t GWTIME = 3;

            // Gesture LED Drive Strength. Sets LED Drive Strength in gesture mode.
            uint8_t GLDRIVE = 2;

            // Gesture Gain Control. Sets the gain of the proximity receiver in gesture
            // mode.
            uint8_t GGAIN = 2;

            uint8_t get() { return (byte)((GGAIN << 5) | (GLDRIVE << 3) | GWTIME); }
        };
        Gconf2 _gconf2 = new Gconf2();

        class Gpulse
        {
            /* Number of Gesture Pulses. Specifies the number of pulses to be generated
            on LDR. Number of pulses is set by GPULSE value plus 1.
            */
            uint8_t GPULSE = 6;

            // Gesture Pulse Length. Sets the LED_ON pulse width during a Gesture LDR
            // Pulse.
            uint8_t GPLEN = 2;

            uint8_t get() { return (byte)((GPLEN << 6) | GPULSE); }
        };
        Gpulse _gpulse = new Gpulse();

        class Gconf3
        {
            /* Gesture Dimension Select. Selects which gesture photodiode pairs are
            enabled to gather results during gesture.
            */
            uint8_t GDIMS = 2;

            uint8_t get() { return GDIMS; }
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
            uint8_t GMODE = 1;

            /* Gesture interrupt enable. Gesture Interrupt Enable. When asserted, all
            gesture related interrupts are unmasked.
            */
            uint8_t GIEN = 2;

            uint8_t get() { return (byte)((GIEN << 1) | GMODE); }
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
            uint8_t GVALID = 1;

            /* Gesture FIFO Overflow. A setting of 1 indicates that the FIFO has filled
            to capacity and that new gesture detector data has been lost.
            */
            uint8_t GFOV = 1;

            void set(uint8_t data)
            {
                GFOV = (byte)((data >> 1) & 0x01);
                GVALID = (byte)(data & 0x01);
            }
        };
        Gstatus _gstatus = new Gstatus();

        private int read8(Registers reg)
        {
            var r = this._i2cDevice.Send1ByteRead1ByteCommand(this.DeviceID, (byte)reg);
            return r;
        }

        private bool write8(Registers reg, byte value)
        {
            var r = this._i2cDevice.WriteBuffer(this.DeviceID, new List<byte>() { (byte)reg, value }.ToArray());
            return r;
        }
    }
}

