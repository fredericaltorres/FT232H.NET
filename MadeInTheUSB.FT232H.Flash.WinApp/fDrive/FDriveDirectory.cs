using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace MadeInTheUSB.FT232H.Console
{

    public class FDriveDirectory : List<FDriveDirectoryEntry>
    {
        public const int SECTOR_SIZE = 512;
        public const int FDriveDirectory_SECTOR_SIZE = 512;

        public FDriveDirectory()
        {

        }
        public void Populate(List<byte> buffer)
        {
        }

        public List<byte> GenerateFilesDataBuffer()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    foreach (var d in this)
                    {
                        d.WriteDataFile(writer);
                    }
                }
                stream.Flush();

                // This buffer can be bigger than 512 and the count must be a multiple of 512
                var buffer = stream.GetBuffer().ToList();

                return buffer;
            }
        }

        public List<byte> GenerateBuffer()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    foreach (var d in this)
                    {
                        if (d.FileName != FDriveFAT12FileSystem.BLANK_SECTOR_COMMAND)
                            d.Write(writer);
                    }
                }
                stream.Flush();
                var buffer = stream.GetBuffer().ToList();
                if (buffer.Count > FDriveDirectory_SECTOR_SIZE)
                    throw new System.ApplicationException("Directory buffer cannot be more than 512 bytes");

                buffer = BufferUtils.PadBuffer(buffer, FDriveDirectory_SECTOR_SIZE);

                return buffer;
            }
        }
    }
}
