using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MadeInTheUSB.FT232H.Console
{
    public class BufferUtils
    {
        const int SECTOR_SIZE = 512;
        public static List<byte> MakeBuffer(int len, byte val = 0)
        {
            var buffer = new List<byte>();
            for(int i = 0; i < len; i++)
                buffer.Add(val);
            return buffer;
        }
        public static List<byte> PadBuffer(List<byte> buffer, int len)
        {
            if(buffer.Count < len)
            {
                buffer.AddRange(MakeBuffer(len-buffer.Count, 0));
            }
            return buffer;
        }

        public static void WriteBuffer(BinaryWriter writer, List<byte> buffer)
        {
            writer.Write(buffer.ToArray(), 0, buffer.Count);
        }
        public static void WriteString(BinaryWriter writer, string s, int len)
        {
            var f = s.PadRight(len,' ').Substring(0, len);
            writer.Write(f.ToCharArray(), 0, len);
        }

        public static List<byte> GenerateBuffer(List<byte> bufferIn)
        {
            bufferIn = PadBuffer(bufferIn, SECTOR_SIZE);
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(bufferIn.ToArray(), 0, bufferIn.Count);
                }
                stream.Flush();
                var buffer = stream.GetBuffer().ToList();
                if (buffer.Count > FDriveDirectory.FDriveDirectory_SECTOR_SIZE)
                    throw new System.ApplicationException("buffer cannot be more than 512 bytes");

                buffer = BufferUtils.PadBuffer(buffer, FDriveDirectory.FDriveDirectory_SECTOR_SIZE);

                return buffer;
            }
        }
    }
}
