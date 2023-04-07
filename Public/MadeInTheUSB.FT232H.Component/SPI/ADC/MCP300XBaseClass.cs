/*
    MCP3008, MadeInTheUSB.FT232H 
    10-bit Analog-to-Digital Converter - https://www.microchip.com/wwwproducts/en/en010530
    Written in C# by FT for MadeInTheUSB
    Copyright (C) 2018 MadeInTheUSB LLC

    Copyright (C) 2015, 2023 MadeInTheUSB LLC
    Written by FT for MadeInTheUSB
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
  
    MIT license, all text above must be included in any redistribution
  
    Written with the help of
        https://rheingoldheavy.com/mcp3008-tutorial-02-sampling-dc-voltage/
  
    Mcp300X 10bit ADC Breakout Board from RheinGoldHeavy.com supported
    https://rheingoldheavy.com/product/breakout-board-mcp3008/
    https://rheingoldheavy.com/mcp3008-tutorial-02-sampling-dc-voltage/
    
    Datasheet http://www.adafruit.com/datasheets/Mcp300X.pdf
*/

using MadeInTheUSB.FT232H;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MadeInTheUSB
{
    /// <summary>
    /// Base class to support analog to digital converters:
    /// MCP3008 : 8 AD
    /// MCP3004 : 4 AD
    /// </summary>
    public class MCP300XBaseClass
    {
        private ISPI _spi;

        public int MaxAdConverter = 8;

        /// <summary>
        /// The 8 analog to digital (AD) channels/ports configured in single mode.
        /// Differential mode is not implemented.
        /// </summary>
        private List<int> _channelInSingleMode = new List<int>() {
            0x08, // ADC Channel 0
            0x09,
            0x0A,
            0x0B,
            0x0C,
            0x0D,
            0x0E,
            0x0F // ADC Channel 7
        };
        
        public MCP300XBaseClass(int maxADConverter, ISPI spi) 
        {
            this._spi = spi;
            this.MaxAdConverter = maxADConverter;
        }

        /// <summary>
        /// https://github.com/adafruit/Adafruit_Python_MCP3008/blob/master/Adafruit_MCP3008/MCP3008.py
        /// </summary>
        /// <param name="port"></param>
        /// <param name="percentageAdjust"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>

        public int Read(int port, int percentageAdjust = -1)
        {
            if ((port > 7) || (port < 0))
                throw new ArgumentException(string.Format("Invalid analog port {0}", port));

            var command = 0b11 << 6;            //                  # Start bit, single channel read
            command |= (port & 0x07) << 3;      //  # Channel number (in 3 bits)
            
            var bufferReceive = new byte[3] { 0, 0, 0 };
            var bufferSend = new List<Byte>() { (byte)command, 0, 0  };

            if (this._spi.Ok(this._spi.QueryReadWrite(bufferSend.ToArray(), bufferReceive)))
            {

                var r1 = new SPIResult().Succeed(bufferReceive.ToList());
                // System.Console.WriteLine($"bufferOut: {bufferReceive[0]}, {bufferReceive[1]}, {bufferReceive[2]}");
                var v = ValidateOperation(r1);
                if (percentageAdjust != -1)
                {
                    v = v + (v * percentageAdjust / 100);
                }
                return v;
            }
            else return -1;
        }

        /// <summary>
        /// Read the value of one analog port using Nusbio spi/hardware acceleration.
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public int Read2(int port, int percentageAdjust = -1)
        {
            if ((port > 7) || (port < 0))
                throw new ArgumentException(string.Format("Invalid analog port {0}", port));
            
            const byte junk    = (byte)0;
            var port2          = (byte)((_channelInSingleMode[port] << 4) & 0x03);
            var bufferReceive  = new byte[3] { 0, 0, 0 };
            var bufferSend     = new List<Byte>() { 0x1, port2, junk };

            if(this._spi.Ok(this._spi.QueryReadWrite(bufferSend.ToArray(), bufferReceive))) {

                var r1 = new SPIResult().Succeed(bufferReceive.ToList());
                var v = ValidateOperation2(r1);
                if(percentageAdjust != -1)
                {
                    v = v + (v * percentageAdjust / 100);
                }
                return v;
            }
            else return -1;
        }

        public double ComputeVoltage(double voltageReference, int adcValue)
        {
            var voltageValue = (adcValue * voltageReference) / 1024;
            return voltageValue;
        }

        private int ValidateOperation(SPIResult result)
        {
            //var r2 = (((uint16_t)(result.Buffer[1] & 0x07)) << 8) | result.Buffer[2];
            //return r2;
            if (result.OperationSucceeded && result.Buffer.Count == 3)
            {
                int r = (result.Buffer[0] & 0x01) << 9;
                r |= (result.Buffer[1] & 0xFF) << 1;
                r |= (result.Buffer[2] & 0x80) >> 7;
                return r & 0x3FF;
            }
            else return -1;
        }

        private int ValidateOperation2(SPIResult result)
        {
            
            if (result.OperationSucceeded && result.Buffer.Count == 3)
            {
                int r = 0;
                if (WinUtil.BitUtil.IsSet(result.Buffer[1], 1))
                    r += 256;
                if (WinUtil.BitUtil.IsSet(result.Buffer[1], 2))
                    r += 512;
                r += result.Buffer[2];

                return r;
            }
            else return -1;
        }
    }
}

