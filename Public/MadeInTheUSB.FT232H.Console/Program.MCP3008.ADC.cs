using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static FTD2XX_NET.FTDI;
using MadeInTheUSB.FT232H.Components.APA102;
using MadeInTheUSB.FT232H.Components;

namespace MadeInTheUSB.FT232H.Console
{
    partial class Program
    {
        static void MAX7219_SPI_8x8_Matrix(ISPI spi, IDigitalWriteRead gpios) 
        {
            var deviceCount = 1;
            var matrix = new MAX_7219_SPI_8x8_Matrix(spi, MAX_7219_SPI_8x8_Matrix.MAX7219_WIRING_TO_8x8_LED_MATRIX.OriginBottomRightCorner, deviceCount: deviceCount);
            matrix.Begin();
            matrix.SetBrightness(1);
            System.Console.Clear();
            ConsoleEx.TitleBar(0, "Draw Round Rectangle Demo");

            var deviceIndex = 0;
            var wait = 100;

            matrix.CurrentDeviceIndex = deviceIndex;
            var done = false;
            while(!done)
            {
                matrix.Clear(deviceIndex);
                var yy = 0;
                while (yy <= 3)
                {
                    matrix.DrawRoundRect(yy, yy, 8 - (yy * 2), 8 - (yy * 2), 2, 1);
                    //matrix.WriteDisplay();
                    matrix.CopyToAll(deviceIndex, true);
                    Thread.Sleep(wait);
                    yy += 1;
                }
                Thread.Sleep(wait);
                yy = 2;
                while (yy >= 0)
                {
                    matrix.DrawRoundRect(yy, yy, 8 - (yy * 2), 8 - (yy * 2), 2, 0);
                    matrix.CopyToAll(deviceIndex, true);
                    Thread.Sleep(wait);
                    yy -= 1;
                }
                matrix.Clear(deviceIndex);
                matrix.CopyToAll(deviceIndex, true);
                Thread.Sleep(wait);

                if (System.Console.KeyAvailable)
                {
                    if (System.Console.ReadKey().Key == ConsoleKey.Q)
                    {
                        done = true;
                        break;
                    }
                }
                Thread.Sleep(500);
            }
        }
        /// <summary>
        /// </summary>
        /// <param name="spi"></param>
        static void ADC_MCP3008Demo(ISPI spi, IDigitalWriteRead gpios)
        {
            var adc = new MCP3008(spi);
            var done = false;
            System.Console.Clear();
            const double referenceVoltage = 5;
            while(!done)
            {
                System.Console.WriteLine("");
                //                gpios.ProgressNext();
                for (var adcPort = 0; adcPort < 3; adcPort++)
                {
                    var adcValue = adc.Read(adcPort);
                    var voltageValue = adc.ComputeVoltage(referenceVoltage, adcValue);
                    System.Console.WriteLine($"ADC [{adcPort}] = {adcValue}, voltage:{voltageValue}");
                }

                if(System.Console.KeyAvailable) 
                {
                    if(System.Console.ReadKey().Key == ConsoleKey.Q)
                    {
                        done = true;
                        break;
                    }
                }
                Thread.Sleep(1000);
            }
        }
    }
}
