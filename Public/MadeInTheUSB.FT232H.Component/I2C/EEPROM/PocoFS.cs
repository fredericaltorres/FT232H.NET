using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using MadeInTheUSB.WinUtil;
using Brotli;
using static System.Net.Mime.MediaTypeNames;

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
            var buffer = CompressBrotli(json);
            if (!_eeprom.WritePages(0, buffer.ToList()))
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
                    var buffer = DecompressBrotli(compressedBuffer.ToArray());
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
                using (var memory = new MemoryStream())
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
            using (var memory = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(memory, CompressionMode.Compress, true))
                {
                    gzip.Write(raw, 0, raw.Length);
                }
                return memory.ToArray();
            }
        }

        static byte[] CompressBrotli(string text)
        {
            byte[] raw = Encoding.Unicode.GetBytes(text);
            using (var memory = new MemoryStream())
            {
                using (var brotli = new BrotliStream(memory, CompressionMode.Compress ))
                {
                    brotli.Write(raw, 0, raw.Length);
                }
                return memory.ToArray();
            }
        }

        public static byte[] DecompressBrotli(byte[] gzip)
        {
            // Create a GZIP stream with decompression mode.
            // ... Then create a buffer and write into while reading from the GZIP stream.
            using (var  stream = new BrotliStream(new MemoryStream(gzip), CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (var memory = new MemoryStream())
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

    }
}
