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

        static void Main(string[] args)
        {
            var ft232Device = FT232HDetector.Detect();
            if(ft232Device.Ok)
            {
                System.Console.WriteLine(ft232Device.ToString());
                foreach(var p in ft232Device.Properties)
                {
                    System.Console.WriteLine($"{p.Key}: {p.Value}");
                }
            }

            // MCP3088 and MAX7219 is limited to 10Mhz
            var clockSpeed = MpsseSpiConfig._30Mhz;
            // clockSpeed = MpsseSpiConfig._10Mhz;
            var ft232hGpioSpiDevice = new GpioSpiDevice(MpsseSpiConfig.Make(clockSpeed));

            var spi                 = ft232hGpioSpiDevice.SPI;
            var gpios               = ft232hGpioSpiDevice.GPIO;

            //GpioSample(gpios, true);
            // CheetahBoosterDemo(gpios, false);

            // CypressFlashMemorySample(spi);
            FlashMemoryWriteFDriveFileSystem(spi);
            // FlashMemoryWriteFlashContentToLocalFile(spi);

            //Api102RgbLedSample(spi);
            // ADC_MCP3008Demo(spi);
            //GpioSample(gpios, true);
        }
    }
}
