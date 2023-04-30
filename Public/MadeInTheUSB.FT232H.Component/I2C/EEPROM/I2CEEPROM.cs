using MadeInTheUSB.FT232H.Components;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using MadeInTheUSB.WinUtil;

namespace MadeInTheUSB.FT232H.Component.I2C.EEPROM
{
    public class PocoFS  
    {
        public IFlashEepromInterface _eeprom { get; }
        public long LastExecutionTime;
        public PocoFS(IFlashEepromInterface eeprom)
        {
            _eeprom = eeprom;
        }

        public void Save(object poco)
        {
            var sw = StopWatch.StartNew();
            var json = JsonConvert.SerializeObject(poco, Formatting.None);
            var buffer = Compress(json);
            if(!_eeprom.WritePages(0, buffer.ToList()))
                throw new InvalidOperationException($"Failed writing poco object");

            sw.Stop();
            this.LastExecutionTime = sw.ElapsedMilliseconds;
        }

        public T Load<T>()
        {
            var sw = StopWatch.StartNew();
            try
            {
                var compressedBuffer = new List<byte>();
                var r = _eeprom.ReadPages(0, _eeprom.SizeInByte, compressedBuffer);
                if (r)
                {
                    var buffer = Decompress(compressedBuffer.ToArray());
                    var json = Encoding.Unicode.GetString(buffer);
                    return JsonConvert.DeserializeObject<T>(json);
                }
                else return default(T);
            }
            finally
            {
                sw.Stop();
                this.LastExecutionTime = sw.ElapsedMilliseconds;
            }
        }

        //https://www.dotnetperls.com/decompress
        public static byte[] Decompress(byte[] gzip)
        {
            // Create a GZIP stream with decompression mode.
            // ... Then create a buffer and write into while reading from the GZIP stream.
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }
        }

        public static byte[] Compress(string text)
        {
            byte[] raw = Encoding.Unicode.GetBytes(text);
            using (MemoryStream memory = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(memory, CompressionMode.Compress, true))
                {
                    gzip.Write(raw, 0, raw.Length);
                }
                return memory.ToArray();
            }
        }
    }

    // AT24C256 - 32kb - 1Mhz - 64B Page Write Mode
    // http://ww1.microchip.com/downloads/en/devicedoc/atmel-8568-seeprom-at24c256c-datasheet.pdf
    public class I2CEEPROM_AT24C256 : IFlashEepromInterface
    {
        private I2CDevice _i2cDevice { get; }
        public byte DeviceId { get; set; }

        public const byte AT24C256_DEVICE_ID = 0x50; // 80 A0=GND A1= GND A2=GND == 0x50 == 80 == 01010000 - 64 32 16 8 4-A2 2-A1 1-A0

        public I2CEEPROM_AT24C256(I2CDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;
        }

        public int PageSize => 64;
        public int SizeInByte => 32 * 1024;
        public int SizeInKByte => this.SizeInByte / 1024;
        public int SizeInMByte => this.SizeInByte / 1024 / 1024;
        public int MaxPage => this.SizeInByte / (int)this.PageSize;
        public int Max64KbBlock => this.SizeInByte / FlashMemory._64K_BLOCK_SIZE;

        public bool Begin(byte deviceId = AT24C256_DEVICE_ID)
        {
            this.DeviceId = deviceId;
            if (this._i2cDevice.DetectDevice(deviceId))
            {
                var eepromBufferIn = new List<byte>();
                var rIn = this.ReadPages(0, this.PageSize, eepromBufferIn);
                this._i2cDevice.RegisterDeviceIdForLogging((byte)this.DeviceId, this.GetType());
                return true;
            }
            else return false;
        }

        public bool WritePages(int addr, List<byte> buffer, bool verify = false)
        {
            var blockCountToWrite = (buffer.Count / this.PageSize) + 1;
            var r = false;

            for(var page = 0; page < blockCountToWrite; page ++)
            {
                var tmpBuffer = buffer.Skip(page * this.PageSize).Take(this.PageSize).ToList();
                if(tmpBuffer.Count > 0)
                {
                    r = WritePage(addr + (page * this.PageSize), tmpBuffer);
                }
            }

            return r;
        }

        private bool WritePage(int addr, List<byte> buffer)
        {
            bool r;
            var buffer2 = (new byte[] { (byte)(addr >> 8), (byte)(addr & 0xFF) }).ToList();
            buffer2.AddRange(buffer);

            r = this._i2cDevice.WriteBuffer(buffer2.ToArray(), this.DeviceId);
            Thread.Sleep(10); // Unfortunately
            return r;
        }

        // https://github.com/adamjezek98/Eeprom_at24c256/blob/master/Eeprom_at24c256.cpp
        public bool ReadPages(int addr, int byteToRead, List<byte> buffer)
        {
            if (byteToRead > this.SizeInByte)
                byteToRead = this.SizeInByte;

            var outputBuffer = (new byte[] { (byte)(addr >> 8), (byte)(addr & 0xFF) }).ToList();
            outputBuffer.AddRange(buffer);

            if (this._i2cDevice.WriteBuffer(outputBuffer.ToArray(), this.DeviceId, terminateTransmission: true))
            {
                buffer.AddRange(this._i2cDevice.ReadXByte(byteToRead, this.DeviceId));
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
