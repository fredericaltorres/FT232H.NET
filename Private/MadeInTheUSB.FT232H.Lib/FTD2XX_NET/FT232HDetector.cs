using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicSugar;
using static FTD2XX_NET.FTDI;

namespace MadeInTheUSB.FT232H
{
    public class FT232HDetectorInformation
    {
        public string SerialNumber;
        public string Description;
        public FT_DEVICE DeviceType;
        public bool Ok;
        public Dictionary<string, object> Properties = new Dictionary<string, object>();

        public override string ToString()
        {
            return $"Type:{DeviceType}, SerialNumber:{SerialNumber}, Description:{Description}, {Properties.Count} Properties";
        }

        public static FT232HDetectorInformation Failed = new FT232HDetectorInformation()
        {
            Ok = false
        };

    }

    public class FT232HDetector
    {
        public static FT232HDetectorInformation Detect(string serialNumber = null) {

            var r = new FT232HDetectorInformation();
            var ft232h = new FTD2XX_NET.FTDI();

            UInt32 count = 0;
            Ok(ft232h.GetNumberOfDevices(ref count));

            if(count == 0)
            {
                Console.WriteLine("No FT232H device detected");
                return r;
            }
                

            FT_DEVICE_INFO_NODE ft232hDevice = null;
            var devices = new FT_DEVICE_INFO_NODE[count];
            ft232h.GetDeviceList(devices);

            if(serialNumber == null)
                ft232hDevice = devices[0];
            else
                ft232hDevice = devices.ToList().FirstOrDefault(d => d.SerialNumber == serialNumber);

            r.SerialNumber = ft232hDevice.SerialNumber;
            r.DeviceType = ft232hDevice.Type;
            r.Description = ft232hDevice.Description;

            if(ft232hDevice == null)
                return FT232HDetectorInformation.Failed;

            Ok(ft232h.OpenBySerialNumber(ft232hDevice.SerialNumber));
            var ee232h = new FT232H_EEPROM_STRUCTURE();
            var rr = ft232h.ReadFT232HEEPROM(ee232h);
            if(rr == FT_STATUS.FT_OK)
            {
                r.Description = ee232h.Description;
                r.Properties = ReflectionHelper.GetDictionary(ee232h);
            }
            ft232h.Close();
            r.Ok = true;
            return r;
         }

        private static bool Ok(FT_STATUS status)
        {
            if(status == FT_STATUS.FT_OK)
                return true;
            throw new ApplicationException($"[{nameof(FT232HDetector)}]Last operation failed code:{status}");
        }
    }
}
