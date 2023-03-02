using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static FTD2XX_NET.FTDI;
using MadeInTheUSB.FT232H.Components.APA102;
using DynamicSugar;
using MadeInTheUSB.Adafruit;

namespace MadeInTheUSB.FT232H.Console
{
    partial class Program
    {
        // MCP9808_TemperatureSensor

        static void MCP9808_TemperatureSensor_Sample(I2CDevice i2cDevice)
        {
            var ts = new MCP9808_TemperatureSensor(i2cDevice);
            ts.Begin();
            while (true)
            {
                if(System.Console.KeyAvailable)
                {
                    var k = System.Console.ReadKey(true);
                    if (k.Key == ConsoleKey.Q)
                        break;
                }
                var temp = ts.GetTemperature(TemperatureType.Fahrenheit);
                ConsoleEx.WriteLine($"Temp:{temp}", ConsoleColor.Blue);
                ConsoleEx.Wait(1);
            }
        }
    }
}

