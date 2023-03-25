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
        /// </summary>
        /// <param name="spi"></param>
        static void ADC_MCP3008Demo(ISPI spi, IDigitalWriteRead gpios)
        {
            var adc = new MCP3008(spi);
            var done = false;
            System.Console.Clear();
            const double referenceVoltage = 3.3;
            while(!done)
            {
                gpios.ProgressNext();
                for (var adcPort = 0; adcPort < 1; adcPort++)
                {
                    var adcValue = adc.Read(adcPort);
                    var voltageValue = adc.ComputeVoltage(referenceVoltage, adcValue);
                    System.Console.WriteLine($"ADC [{adcPort}] = {adcValue}, voltage:{voltageValue}");
                }

                if(System.Console.KeyAvailable) {
                    if(System.Console.ReadKey().Key == ConsoleKey.Q)
                    {
                        done = true;
                        break;
                    }
                }
                Thread.Sleep(500);
            }
        }
    }
}
