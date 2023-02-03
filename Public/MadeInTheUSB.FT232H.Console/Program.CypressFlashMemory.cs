using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static FTD2XX_NET.FTDI;
using MadeInTheUSB.FT232H.Components.APA102;
using MadeInTheUSB.FT232H.Components;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MadeInTheUSB.FT232H.Console
{

    partial class Program
    {
        static void FlashMemoryWriteFlashContentToLocalFile(ISPI spi)
        {
            var flash = new CypressFlashMemory(spi);
            flash.ReadIdentification();
            System.Console.WriteLine(flash.GetDeviceInfo());

            var fDriveFS = new FDriveFileSystem(flash);
            fDriveFS.WriteFlashContentToLocalFile("flash.fat12.bin");
        }
        static void FlashMemoryWriteFDriveFileSystem(ISPI spi)
        {
            var flash = new CypressFlashMemory(spi);
            flash.ReadIdentification();
            System.Console.WriteLine(flash.GetDeviceInfo());

            var fDriveFS = new FDriveFileSystem(flash);
            fDriveFS.WriteFiles(new List<string> {
                @"C:\DVT\LILYGO T-Display-S3 ESP32-S3\mass storage\Files\README.TXT",
                @"C:\DVT\LILYGO T-Display-S3 ESP32-S3\mass storage\Files\WRITEME.TXT",
                @"C:\DVT\LILYGO T-Display-S3 ESP32-S3\mass storage\Files\MASTER.TXT",
                FDriveFileSystem.BLANK_SECTOR_COMMAND,
                @"C:\DVT\LILYGO T-Display-S3 ESP32-S3\mass storage\Files\VIEWME.JPG",
            });
        }


        static void CypressFlashMemorySample(ISPI spi)
        {
            var flash = new CypressFlashMemory(spi);
            flash.ReadIdentification();
            System.Console.WriteLine(flash.GetDeviceInfo());

            var _64kAbdcString = PerformanceHelper.Get64kStringAbcd();
            var _64kAbdcBuffer = PerformanceHelper.GetAsciiBuffer(_64kAbdcString).ToList();

            var _64kFredString = PerformanceHelper.Get64kStringFred();
            var _64kFredBuffer = PerformanceHelper.GetAsciiBuffer(_64kFredString).ToList();

            var _64k0123String = PerformanceHelper.Get64kString0123();
            var _64k0123Buffer = PerformanceHelper.GetAsciiBuffer(_64k0123String).ToList();
            var ph = new PerformanceHelper();
            var WRITE_FLASH = false;

            if (WRITE_FLASH)
            {
                // flash.FormatFlash((done) => { System.Console.WriteLine($"{done} formatted"); });

                // Write the 16 Mb of FLASH using 64k string
                // Each block or page must be erased before being written

                ph.Start();
                for (var _64kBlock = 0; _64kBlock < flash.MaxBlock; _64kBlock++)
                {
                    ph.AddByte(CypressFlashMemory.BLOCK_SIZE);
                    System.Console.WriteLine($"Writing block:{_64kBlock}/{flash.MaxBlock}, {_64kBlock * 100.0 / flash.MaxBlock:0}%");
                    var r = false;

                    if (_64kBlock == 11)
                    {
                        r = flash.WritePages(_64kBlock * CypressFlashMemory.BLOCK_SIZE, _64k0123Buffer, format: true);
                    }
                    else
                    {
                        if (_64kBlock % 3 == 0)
                            r = flash.WritePages(_64kBlock * CypressFlashMemory.BLOCK_SIZE, _64kFredBuffer, format: true);
                        else
                            r = flash.WritePages(_64kBlock * CypressFlashMemory.BLOCK_SIZE, _64kAbdcBuffer, format: true);

                    }
                    if (!r)
                        System.Console.WriteLine($"Error writing block:{_64kBlock}");
                }
                ph.Stop();
                System.Console.WriteLine($"Write Operation:{ph.GetResultInfo()}");
                System.Console.ReadKey();
            }

            // Read the 16 Mb of FLASH and verify result
            ph = new PerformanceHelper();
            ph.Start();
            for (var _64kBlock = 0; _64kBlock < flash.MaxBlock; _64kBlock++)
            {
                System.Console.WriteLine($"Reading block:{_64kBlock}/{flash.MaxBlock}, {_64kBlock * 100.0 / flash.MaxBlock:0}%");
                var buffer = new List<byte>();
                if (flash.ReadPages(_64kBlock * CypressFlashMemory.BLOCK_SIZE, CypressFlashMemory.BLOCK_SIZE, buffer))
                {
                    var resultString = PerformanceHelper.AsciiBufferToString(buffer.ToArray());
                    ph.AddByte(CypressFlashMemory.BLOCK_SIZE);
                    var result = false;
                    if (_64kBlock == 11)
                    {
                        result = (resultString == _64k0123String);
                        PerformanceHelper.AssertString(resultString, _64k0123String);
                        System.Console.WriteLine($"Reading block:{_64kBlock}, Status:{result}");
                    }
                    else if (_64kBlock % 3 == 0)
                    {
                        result = (resultString == _64kFredString);
                        PerformanceHelper.AssertString(resultString, _64kFredString);
                        System.Console.WriteLine($"Reading block:{_64kBlock}, Status:{result}");
                        if (!result && Debugger.IsAttached) Debugger.Break();
                    }
                    else
                    {
                        result = (resultString == _64kAbdcString);
                        PerformanceHelper.AssertString(resultString, _64kAbdcString);
                        System.Console.WriteLine($"Reading block:{_64kBlock}, Status:{result}");
                        if (!result && Debugger.IsAttached) Debugger.Break();
                    }
                }
            }
            ph.Stop();
            System.Console.WriteLine($"Read Operation:{ph.GetResultInfo()}");
            System.Console.ReadKey();
        }
    }
}
