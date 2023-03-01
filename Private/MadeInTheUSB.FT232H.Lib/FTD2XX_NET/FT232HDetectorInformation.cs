using FTD2XX_NET;
using System.Collections.Generic;
using static FTD2XX_NET.FTDI;

namespace MadeInTheUSB.FT232H
{
    public class FT232HDetectorInformation
    {
        public string SerialNumber;
        public string Description;
        public FT_DEVICE DeviceType;
        public bool Ok;
        FTDI.FT_STATUS _lastStatus;
        public Dictionary<string, object> Properties = new Dictionary<string, object>();

        public FTD2XX_NET.FTDI ft232h = null;

        public override string ToString()
        {
            return $"Type:{DeviceType}, NusbioV2Device:{this.IsNusbioV2}, SerialNumber:{SerialNumber}, Description:{Description}, Properties:{Properties.Count}";
        }

        public static FT232HDetectorInformation Failed = new FT232HDetectorInformation()
        {
            Ok = false
        };

        public bool IsNusbioV2 => this.Description == FT232HDetector.NUSBIO_V2_DEVICE_DESCRIPTION;
        public bool IsFT2323H => this.DeviceType == FT_DEVICE.FT_DEVICE_232H;
        
    }
}
