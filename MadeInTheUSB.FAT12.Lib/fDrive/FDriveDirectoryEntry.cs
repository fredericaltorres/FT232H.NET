using BufferUtil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MadeInTheUSB.FAT12
{
    public class FDriveDirectoryEntry
    {
        public enum FILE_ATTRIBUTE {

            FA_READ_ONLY      = 0x01,
            FA_HIDDEN         = 0x02,
            FA_SYSTEM         = 0x04,
            FA_VOLUME_LABEL   = 0x08,
            FA_SUB_DIRECTORY  = 0x10,
            FA_FILE_ARCHIVE   = 0x20,
        }

        const int MAX_FILENAME_LEN = 8;
        const int MAX_EXTENSION_LEN = 3;
        public string FileName;
        public string Extention;
        public Int32 FileSize;
        public UInt16 FirstLogicalSector;
        public FILE_ATTRIBUTE FileAttribute;

        public string LocalFileName;

        public Int32 SectorCount => (Int32)((this.FileSize / FDriveDirectory.SECTOR_SIZE)+1);

        public void WriteDataFile(BinaryWriter writer)
        {
            if (this.FileName == FDriveFAT12FileSystem.BLANK_SECTOR_COMMAND)
            {
                var tmpBuffer = BufferUtils.MakeBuffer(FDriveDirectory.SECTOR_SIZE, 0);
                BufferUtils.WriteBuffer(writer, tmpBuffer);
            }
            else if (this.FileAttribute != FILE_ATTRIBUTE.FA_VOLUME_LABEL)
            {
                var buffer = File.ReadAllBytes(LocalFileName);
                for (var sx = 0; sx < SectorCount; sx++)
                {
                    var tmpBuffer = buffer.Skip(sx * FDriveDirectory.SECTOR_SIZE).Take(FDriveDirectory.SECTOR_SIZE).ToList();
                    tmpBuffer = BufferUtils.PadBuffer(tmpBuffer, FDriveDirectory.SECTOR_SIZE);
                    BufferUtils.WriteBuffer(writer, tmpBuffer);
                }
            }
        }

        public void Write(BinaryWriter writer)
        {
            // 8 + 3 + 4 + 4 + '|'
            BufferUtils.WriteString(writer, this.FileName, MAX_FILENAME_LEN);
            BufferUtils.WriteString(writer, this.Extention, MAX_EXTENSION_LEN);
            writer.Write((byte)this.FileAttribute);

            BufferUtils.WriteBuffer(writer, new List<byte>()
            {
                0x00, 0x00,                             // Reserved
                0x21, 0x08,                             // Creation time 01:01:01
                0x8B, 0x09,                             // Creation date 1984/12/11
                0x00, 0x00,                             // Last accessed date
                0x00, 0x00,                             // Ignored in FAT 12
                0x4F, 0x6D,                             // Last write time
                0x65, 0x43,                             // Last write date
            });
            writer.Write(this.FirstLogicalSector);
            writer.Write(this.FileSize);
        }
    }
}
