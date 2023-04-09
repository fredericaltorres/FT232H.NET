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
using System.Text;
using int16_t = System.Int16;
using uint16_t = System.UInt16;
using uint8_t = System.Byte;

namespace MadeInTheUSB
{

    /// <summary>
    /// https://cdn-shop.adafruit.com/datasheets/MCP3008.pdf
    /// </summary>
    public class MCP3008 : MCP300XBaseClass
    {
        public MCP3008(ISPI spi, SpiChipSelectPins cs) : base(8, spi, cs)
        {
        }
    }
}

