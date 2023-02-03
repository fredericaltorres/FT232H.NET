using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace MadeInTheUSB.FT232H.Console
{
    public class FDriveFat12Linkedlist
    {
        public List<byte> _dataSector1 = new List<byte>() {
                /*
                This FAT is one sector 512 x 8 = 4096 bit, 4096 bit / 12 bit = 341 12bit-unit
                Every address is 12b, so we have to do the match
                341 * 512 = 174592, we can only address 174592 Kb.
                We could increase the number of sector for the FAT
                FAT 12bit value
                -------------------------
                0x00          Unused
                0xFF0-0xFF6   Reserved cluster
                0xFF7         Bad cluster
                0xFF8-0xFFF   Last cluster in a file (4088-4095)
                0x01-0xFF0-1  This cluster is in use, and the next cluster is in location contains in this value
            */
            0xF8, 0xFF, 0x00, // FAT ID for 12 bit values, 0xFFF for second 12 bit values, - First 2 logical entries must be F8FF - Reserved
                              // BUG in TinyUSB, Since the first three bytes in the FAT are reserved, that leaves 509 byes in the first sector.  -- https://samskalicky.wordpress.com/2013/08/08/a-look-back-at-the-fat12-file-system/
            // 8 sectors - DOES NOT WORK
            // Decode value 1(even)   v1 + ((v2 & ~(128 + 64 + 32 + 16)) << 8)
            // Decode value 2(odd)  ((v2 & ~(8 + 4 + 2 + 1)) >> 4) + (v3 << 4)
            // b1      b2        b3
            // For a 14 sector file, we need the address of the second to 14" block which is 13 value + a Terminator

          // README.TXT is 1 sectors, 1 Terminator + WRITEME.TXT is 1 sectors, 1 Terminator
          0xFF, 0xFF, 0xFF,  // a1:4095, a2:0000, FAT[002, 003]
          // MASTER.TXT is 1 sectors, 1 Terminator + NOT_USED is 1 sectors, 1 Terminator
          0xFF, 0xFF, 0xFF,  // a1:4095, a2:0000, FAT[002, 003]

          // VIEWME.JPG 10s
          // File:FredImage5k.jpg, size:4612, sector:10
          // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
          // FAT_LINKEDLIST: 10 sectors (even), => 9 address + 1 Terminator, fileStartSector: 6
          0x07, 0x80, 0x00,  // a1:0007, a2:0008, FAT[002, 003]
          0x09, 0xA0, 0x00,  // a1:0009, a2:0010, FAT[004, 005]
          0x0B, 0xC0, 0x00,  // a1:0011, a2:0012, FAT[006, 007]
          0x0D, 0xE0, 0x00,  // a1:0013, a2:0014, FAT[008, 009]
          0x0F, 0x00, 0x01,  // a1:0015, a2:0016, FAT[010, 011]
          0x11, 0xF0, 0xFF,  // a1:0017, a2:4095, FAT[/Terminator]  
        };

        public List<byte> _dataSector2 = new List<byte>()
        {
            0 , 0, 0,
        };


        public FDriveFat12Linkedlist()
        {

        }
        public void Populate(List<byte> buffer)
        {
        }

        //                              123456789012345678901234567890

        public List<byte> GenerateBuffer()
        {
            var b1 = BufferUtils.GenerateBuffer(_dataSector1);
            var b2 = BufferUtils.GenerateBuffer(_dataSector2);
            b1.AddRange(b2);
            return b1;
        }
    }
}
