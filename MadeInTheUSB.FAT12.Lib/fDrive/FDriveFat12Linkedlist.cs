using System.Collections.Generic;
using System.Linq;
using System.IO;
using BufferUtil;

namespace MadeInTheUSB.FAT12
{
    public class FDriveFat12Linkedlist
    {
        public List<byte> _dataSector1 = new List<byte>() 
        {

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

      // File:FImg43k.jpg, size:44022, sector:86
        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        // FAT_LINKEDLIST: 86 sectors (even), => 85 address + 1 Terminator, fileStartSector: 6
        0x07, 0x80, 0x00,  // a1:0007, a2:0008, FAT[002, 003]
        0x09, 0xA0, 0x00,  // a1:0009, a2:0010, FAT[004, 005]
        0x0B, 0xC0, 0x00,  // a1:0011, a2:0012, FAT[006, 007]
        0x0D, 0xE0, 0x00,  // a1:0013, a2:0014, FAT[008, 009]
        0x0F, 0x00, 0x01,  // a1:0015, a2:0016, FAT[010, 011]
        0x11, 0x20, 0x01,  // a1:0017, a2:0018, FAT[012, 013]
        0x13, 0x40, 0x01,  // a1:0019, a2:0020, FAT[014, 015]
        0x15, 0x60, 0x01,  // a1:0021, a2:0022, FAT[016, 017]
        0x17, 0x80, 0x01,  // a1:0023, a2:0024, FAT[018, 019]
        0x19, 0xA0, 0x01,  // a1:0025, a2:0026, FAT[020, 021]
        0x1B, 0xC0, 0x01,  // a1:0027, a2:0028, FAT[022, 023]
        0x1D, 0xE0, 0x01,  // a1:0029, a2:0030, FAT[024, 025]
        0x1F, 0x00, 0x02,  // a1:0031, a2:0032, FAT[026, 027]
        0x21, 0x20, 0x02,  // a1:0033, a2:0034, FAT[028, 029]
        0x23, 0x40, 0x02,  // a1:0035, a2:0036, FAT[030, 031]
        0x25, 0x60, 0x02,  // a1:0037, a2:0038, FAT[032, 033]
        0x27, 0x80, 0x02,  // a1:0039, a2:0040, FAT[034, 035]
        0x29, 0xA0, 0x02,  // a1:0041, a2:0042, FAT[036, 037]
        0x2B, 0xC0, 0x02,  // a1:0043, a2:0044, FAT[038, 039]
        0x2D, 0xE0, 0x02,  // a1:0045, a2:0046, FAT[040, 041]
        0x2F, 0x00, 0x03,  // a1:0047, a2:0048, FAT[042, 043]
        0x31, 0x20, 0x03,  // a1:0049, a2:0050, FAT[044, 045]
        0x33, 0x40, 0x03,  // a1:0051, a2:0052, FAT[046, 047]
        0x35, 0x60, 0x03,  // a1:0053, a2:0054, FAT[048, 049]
        0x37, 0x80, 0x03,  // a1:0055, a2:0056, FAT[050, 051]
        0x39, 0xA0, 0x03,  // a1:0057, a2:0058, FAT[052, 053]
        0x3B, 0xC0, 0x03,  // a1:0059, a2:0060, FAT[054, 055]
        0x3D, 0xE0, 0x03,  // a1:0061, a2:0062, FAT[056, 057]
        0x3F, 0x00, 0x04,  // a1:0063, a2:0064, FAT[058, 059]
        0x41, 0x20, 0x04,  // a1:0065, a2:0066, FAT[060, 061]
        0x43, 0x40, 0x04,  // a1:0067, a2:0068, FAT[062, 063]
        0x45, 0x60, 0x04,  // a1:0069, a2:0070, FAT[064, 065]
        0x47, 0x80, 0x04,  // a1:0071, a2:0072, FAT[066, 067]
        0x49, 0xA0, 0x04,  // a1:0073, a2:0074, FAT[068, 069]
        0x4B, 0xC0, 0x04,  // a1:0075, a2:0076, FAT[070, 071]
        0x4D, 0xE0, 0x04,  // a1:0077, a2:0078, FAT[072, 073]
        0x4F, 0x00, 0x05,  // a1:0079, a2:0080, FAT[074, 075]
        0x51, 0x20, 0x05,  // a1:0081, a2:0082, FAT[076, 077]
        0x53, 0x40, 0x05,  // a1:0083, a2:0084, FAT[078, 079]
        0x55, 0x60, 0x05,  // a1:0085, a2:0086, FAT[080, 081]
        0x57, 0x80, 0x05,  // a1:0087, a2:0088, FAT[082, 083]
        0x59, 0xA0, 0x05,  // a1:0089, a2:0090, FAT[084, 085]
        0x5B, 0xC0, 0x05,  // a1:0091, a2:0092, FAT[086, 087]
        0x5D, 0xF0, 0xFF,  // a1:0093, a2:4095, FAT[/Terminator]

    };

        public List<byte> _blankSector = new List<byte>()
        {
            0 , 0, 0,
        };

        public int SectorCount { get; }

        public FDriveFat12Linkedlist(int sectorCount)
        {
            SectorCount = sectorCount;
        }
        public void Populate(List<byte> buffer)
        {
        }

        public List<byte> GenerateBuffer()
        {
            var b1 = BufferUtils.GenerateSectorBuffer(_dataSector1);
            var b2 = BufferUtils.GenerateSectorBuffer(_blankSector);
            for (var x=0; x<this.SectorCount-1; x++)
            {
                b1.AddRange(b2);
            }
            
            return b1;
        }
    }
}
