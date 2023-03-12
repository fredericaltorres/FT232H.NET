/*
    Copyright (C) 2015, 2023 MadeInTheUSB LLC

    Adafruit 8x8 LED matrix with backpack
    This program control Adafruit 8x8 LED matrix with backpack
        https://learn.adafruit.com/adafruit-led-backpack/overview
            https://www.adafruit.com/product/872
            https://www.adafruit.com/product/1049 
    The C# files
        Components\Adafruit\Adafruit_GFX.cs
        Components\adafruit\ledbackpack.cs
    are a port from Adafruit library Adafruit-LED-Backpack-Library
    https://github.com/adafruit/Adafruit-LED-Backpack-Library
  
    Written by FT for MadeInTheUSB
    MIT license, all text above must be included in any redistribution

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
 
*/

// Adafruit_LEDBackpack.cpp
// https://github.com/adafruit/Adafruit-LED-Backpack-Library
/*************************************************** 
  This is a library for our I2C LED Backpacks

  Designed specifically to work with the Adafruit LED Matrix backpacks 
  ----> http://www.adafruit.com/products/
  ----> http://www.adafruit.com/products/

  These displays use I2C to communicate, 2 pins are required to 
  interface. There are multiple selectable I2C addresses. For backpacks
  with 2 Address Select pins: 0x70, 0x71, 0x72 or 0x73. For backpacks
  with 3 Address Select pins: 0x70 thru 0x77

  Adafruit invests time and resources providing this open source code, 
  please support Adafruit and open-source hardware by purchasing 
  products from Adafruit!

  Written by Limor Fried/Ladyada for Adafruit Industries.  
  MIT license, all text above must be included in any redistribution
 ****************************************************/

using System;

namespace MadeInTheUSB.Adafruit
{
    public class I2CCommunicationException : Exception
    {
        public I2CCommunicationException()
            : base() { }

        public I2CCommunicationException(string message)
            : base(message) { }

        public I2CCommunicationException(string format, params object[] args)
            : base(string.Format(format, args)) { }

        public I2CCommunicationException(string message, Exception innerException)
            : base(message, innerException) { }

        public I2CCommunicationException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException) { }
    }
}

