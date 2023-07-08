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
using MadeInTheUSB.Display;
using System.Diagnostics;

namespace MadeInTheUSB.FT232H.Console
{
    partial class Program
    {
        static void ADS1115_ADC_ADC(I2CDevice i2cDevice)
        {
            System.Console.Clear();
            ConsoleEx.TitleBar(0, "ADC 16bits ADS1115", ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            ConsoleEx.WriteMenu(0, 2, "Q)uit");
            i2cDevice.Log = true;

            var adc = new ADS1X15_ADC(i2cDevice, ADS1X15_ADC.ADS1x15_Type.ADS1115_16b);
            adc.Gain = ADS1X15_ADC.adsGain_t.GAIN_ONE__4_096V;

            if (adc.Begin())
            {
                while (true)
                {
                    i2cDevice.ForceWriteLogCache();
                    if (System.Console.KeyAvailable)
                    {
                        var k = System.Console.ReadKey(true);
                        if (k.Key == ConsoleKey.Q)
                            break;
                    }

                    ConsoleEx.WriteLine($"", ConsoleColor.White);
                    for (var adcIndex = 0; adcIndex < 4; adcIndex++)
                    {
                        var v = adc.readADC_SingleEnded(adcIndex);
                        var volt = adc.ComputeVolts(v);
                        ConsoleEx.WriteLine($"[{DateTime.Now}] ADC[{adcIndex}] voltagle:{volt:0.00}, {v}", ConsoleColor.White);
                    }
                    ConsoleEx.Wait(1);
                }
            }
        }

        static void MCP9808_TemperatureSensor_Sample(I2CDevice i2cDevice)
        {
            System.Console.Clear();
            ConsoleEx.TitleBar(0, "MCP9808 Temperature Sensor Demo", ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            ConsoleEx.WriteMenu(0, 2, "Q)uit");
            
            var ts = new MCP9808_TemperatureSensor(i2cDevice);
            if (!ts.Begin())
                return;

            while (true)
            {
                if(System.Console.KeyAvailable)
                {
                    var k = System.Console.ReadKey(true);
                    if (k.Key == ConsoleKey.Q)
                        break;
                }
                var FahrenheitTemp = ts.GetTemperature(TemperatureType.Fahrenheit);
                var celciusTemp = ts.GetTemperature(TemperatureType.Celsius);
                ConsoleEx.WriteLine($"[{DateTime.Now}] Temp:{FahrenheitTemp:0.00}F /  {celciusTemp:0.00}C", ConsoleColor.White);
                ConsoleEx.Wait(1);

                i2cDevice.Gpios.ProgressNext();


            }
        }
    }
}

