using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static FTD2XX_NET.FTDI;
using MadeInTheUSB.FT232H.Components.APA102;

namespace MadeInTheUSB.FT232H.Console
{
    partial class Program
    {
        /// <summary>
        /// Test a strip on 1 APA 102 RGB LED.
        /// Old Nusbio extension use nusbio pin 4 for the clock and pin 5 for the data.
        /// APA 102 use a compatible SPI protocol with no CS and MISO, just CLOCK and MOSI.
        /// </summary>
        /// <param name="spi"></param>
        static void Api102RgbLedSample(ISPI spi)
        {
            var brightness = APA102LEDStrip.MAX_BRIGHTNESS / 3 * 2;
            var wait       = 100;
            var api102     = new APA102LEDStrip(1, spi);
            var done       = false;
            while(!done)
            {
                for(int i = 0; i <= 255; i+= 4)
                {
                    if(System.Console.KeyAvailable) {
                        if(System.Console.ReadKey().Key == ConsoleKey.Q)
                        {
                            done = true;
                            break;
                        }
                    }

                    var color = APA102LEDStrip.Wheel(i);
                    System.Console.WriteLine($"Index:{i:000}, Color:{APA102LEDStrip.ToHexValue(color)}");
                    api102
                        .AllToOneColor(color, brightness)
                        .Show()
                        .Wait(wait);
                }
            }
            api102.AllOff();
        }
    }
}
