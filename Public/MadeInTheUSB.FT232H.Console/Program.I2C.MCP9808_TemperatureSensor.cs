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

        static void ADS1015_ADC_ADC(I2CDevice i2cDevice)
        {
//            i2cDevice.I2C_SetLineStatesIdle();
            var adc = new ADS1015_ADC(i2cDevice);
            if (adc.Begin())
            {
                var v = adc.readADC_SingleEnded(0);
            }
        }


        // C:\DVT\MadeInTheUSB.Nusbio\MadeInTheUSB.2018.02.16\MadeInTheUSB\Nusbio.Samples.TRUNK\MadeInTheUSB.Nusbio.Components\LCD.Display\LiquidCrystal.Demo.cs
        static void LiquidCrystal(I2CDevice i2cDevice)
        {
            var lc = new LiquidCrystal_I2C_PCF8574(i2cDevice, 16, 2, deviceId: 0x3F);
            if (lc.Begin(16, 2))
            {
               lc.Backlight();
                lc.SetCursor(0, 0);
                lc.Cursor();
                lc.Print("Hello World");
            }
        }

        

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

