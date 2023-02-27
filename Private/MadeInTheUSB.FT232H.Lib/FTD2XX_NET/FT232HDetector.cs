using System;
using System.Linq;
using DynamicSugar;
using FTD2XX_NET;
using static FTD2XX_NET.FTDI;

namespace MadeInTheUSB.FT232H
{
    public class FT232HDetector
    {
        public const string NUSBIO_V2_DEVICE_DESCRIPTION = "Nusbio /2";

        FTDI.FT_STATUS _lastStatus;

        public static FT232HDetectorInformation Detect(string serialNumber = null)
        {
            var r = new FT232HDetectorInformation();
            var ft232h = new FTD2XX_NET.FTDI();

            UInt32 count = 0;
            Ok(ft232h.GetNumberOfDevices(ref count));

            if (count == 0)
            {
                Console.WriteLine("No FT232H device detected");
                return r;
            }

            FT_DEVICE_INFO_NODE ft232hDevice = null;

            var devices = new FT_DEVICE_INFO_NODE[count];
            ft232h.GetDeviceList(devices);

            if (serialNumber == null)
                ft232hDevice = devices[0];
            else
                ft232hDevice = devices.ToList().FirstOrDefault(d => d.SerialNumber == serialNumber);

            r.SerialNumber = ft232hDevice.SerialNumber;
            r.DeviceType = ft232hDevice.Type;
            r.Description = ft232hDevice.Description;

            if (r.DeviceType == FTDI.FT_DEVICE.FT_DEVICE_232H)
            {
                Console.WriteLine("aa");
            }

            if (ft232hDevice == null)
                return FT232HDetectorInformation.Failed;

            Ok(ft232h.OpenBySerialNumber(ft232hDevice.SerialNumber));
            FT232H_EEPROM_STRUCTURE ee232h;
            
            if (Ok(ReadEEPROM(ft232h, out ee232h)))
            {
                r.Description = ee232h.Description;
                r.Properties = ReflectionHelper.GetDictionary(ee232h);
            }
            ft232h.Close();
            r.Ok = true;
            return r;
        }

        public static bool InitializeAsNusbioV2Device(string serialNumber = null)
        {
            var r = false;
            var ft232h = new FTD2XX_NET.FTDI();
            UInt32 count = 0;
            Ok(ft232h.GetNumberOfDevices(ref count));

            if (count == 0)
            {
                Console.WriteLine("No FT232H device detected");
                return r;
            }

            FT_DEVICE_INFO_NODE ft232hDevice = null;

            var devices = new FT_DEVICE_INFO_NODE[count];
            ft232h.GetDeviceList(devices);

            if (serialNumber == null)
                ft232hDevice = devices[0];
            else
                ft232hDevice = devices.ToList().FirstOrDefault(d => d.SerialNumber == serialNumber);

            var serialNumber2 = ft232hDevice.SerialNumber;
            var deviceType = ft232hDevice.Type;
            var description = ft232hDevice.Description;

            if (ft232hDevice == null)
                return r;

            Ok(ft232h.OpenBySerialNumber(ft232hDevice.SerialNumber));
            FT232H_EEPROM_STRUCTURE ee232h;

            if (Ok(ReadEEPROM(ft232h, out ee232h)))
            {
                ee232h.Description = NUSBIO_V2_DEVICE_DESCRIPTION;
                r = Ok(WriteEEPROM(ft232h, ee232h));
            }
            ft232h.Close();
            return r;
        }

        private static FT_STATUS ReadEEPROM(FTDI ft232h, out FT232H_EEPROM_STRUCTURE ee232h)
        {
            ee232h = new FT232H_EEPROM_STRUCTURE();
            return ft232h.ReadFT232HEEPROM(ee232h);
        }

        private static FT_STATUS WriteEEPROM(FTDI ft232h, FT232H_EEPROM_STRUCTURE ee232h)
        {
            return ft232h.WriteFT232HEEPROM(ee232h);
        }

        private static bool Ok(FT_STATUS status)
        {
            if (status == FT_STATUS.FT_OK)
                return true;
            throw new ApplicationException($"[{nameof(FT232HDetector)}]Last operation failed code:{status}");
        }
    }
}
