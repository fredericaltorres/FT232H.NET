﻿using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace MadeInTheUSB.FT232H.Console
{
    public class FDriveFat12BootSector  
    {
        const int DISK_BLOCK_SIZE = 512;
        // 10 sectors 5k => (10*512/1.5)*512/1024 = 1.7Mb
        // 2 sectors  1k => (2*512/1.5)*512/1024 = 341Kb
        const int FAT_LINKED_LIST_SECTOR_COUNT = 10;
        // 8KB is the smallest size that windows allow to mount - (4+1+1+1+1+10) == 18 * 512 = 9k used
        const int DISK_BLOCK_NUM = ((1/*booSector*/+ FAT_LINKED_LIST_SECTOR_COUNT/*FATLinkedList*/+ 1/*RootDirectory*/) + 1/*readme.txt*/    + 1/* writeme.txt*/  + 1/*master.txt*/  + 1/* not used*/   + 10/*viewmr.jpg*/ ); // value:18


        public List<byte> _data = new List<byte>() {


                0xEB, 0x3C, 0x90, // Dummy jump instruction, not used (e.g., 0xEB 0xFE 0x90)
                //  M     S     D     O     S     5     .     0
                0x4D, 0x53, 0x44, 0x4F, 0x53, 0x35, 0x2E, 0x30,
                // BIOS Parameter Block [19]
                0x00, 0x02,    // Byte per Sector: Value (2 << 8) + 0 == 512, Bytes per logical sector in powers of two; the most common value is 512. [2]
                0x01,          // Sector/Cluster. Logical sectors per cluster. 
                0x01, 0x00,    // Value: 1, Reserved sector count. The number of sectors before the first FAT in the file system image. Should be 1 for FAT12/FAT16. Usually 32 for FAT32
                0x01,          // Number of File Allocation Tables. Almost always 2 but in FAT12 it is 1; [1]   
                0x10, 0x00,    // 16, Maximum number of root directory entries ==> 512/32 == 16 file entries in the root directory. We have only one sector for the root directory.
                               // Since we have 16 directory entry / sector (512) and WE ONLY HAVE 16-1 (*15*) ENTRIES (VOLUME_ID use one entry), this mean we only have one sector for the root directory

                //TOTAL_SECTOR_COUNT, 0x00, // 0x10 == 16, 16 * 512 == 8192 == 6.8kb, Windows:6.5Kb Total sector count.  <<<<<<< THIS MAY BE CHANGED IF WE CHANGE THE SIZE OF THE DISK >>>>
                //                          // 2 Solutions 1) we fit all data into the memory or we use an SPI FLASH / SD Card.

                // 0x84, 0x00, // TOTAL_SECTOR_COUNT 132, All:67584b, 66Kb,  DataOnly:65536b, 64Kb
                // 0x04, 0x01, // TOTAL_SECTOR_COUNT 260, All:133120b, 130Kb,  DataOnly:131072b, 128Kb
                // 0x58, 0x01, // TOTAL_SECTOR_COUNT 344, All:176128b, 172Kb,  DataOnly:174080b, 170Kb     
                // 0x04, 0x10, // TOTAL_SECTOR_COUNT 4100, All:2099200b, 2050Kb,  DataOnly:2097152b, 2048Kb NOT GOOD
                // 0x04, 0x20, // TOTAL_SECTOR_COUNT 8196, All:4196352b, 4098Kb,  DataOnly:4194304b, 4096Kb NOT GOOD
                // 0x04, 0x08, // TOTAL_SECTOR_COUNT 2052, All:1050624b, 1026Kb,  DataOnly:1048576b, 1024Kb Windows 1036288 Free, 1 044 480 Capacity
                0x00, 0x08, // TOTAL_SECTOR_COUNT 2048, All:1048576b, 1024Kb,  DataOnly:1042432b, 1018Kb Windows Capacity: 1042432, Free: 1034240

                0xF8,         /* Media descriptor - Fixed disk (i.e. Hard disk).
                                  0xF8 - Fixed disk (i.e. Hard disk).
                                  0xF0 - 3.5" Double Sided, 80 tracks per side, 18 or 36 sectors per track (1.44MB or 2.88MB). 5.25" Double Sided, 15 sectors per track (1.2MB). Used also for other media types.
                                  0xF9 - 3.5" Double sided, 80 tracks per side, 9 sectors per track (720K). 5.25" Double sided, 40 tracks per side, 15 sectors per track (1.2MB)
                                  0xFA - 5.25" Single sided, 80 tracks per side, 8 sectors per track (320K)
                                  0xFB - 3.5" Double sided, 80 tracks per side, 8 sectors per track (640K)
                                  0xFC - 5.25" Single sided, 40 tracks per side, 9 sectors per track (180K)
                                  0xFD - 5.25" Double sided, 40 tracks per side, 9 sectors per track (360K). Also used for8"
                              */

                FAT_LINKED_LIST_SECTOR_COUNT, 0x00,   /* 2 - Sectors per FAT
                // 0x02, 0x00,   /* 2 - Sectors per FAT
                                    Number of sectors per FAT. Each sector is allocated 12-bits (hence FAT12) in the FAT.
                                    That means that 24-bits or 3 bytes is used for every 2 files.
                                    Since the first three bytes in the FAT are reserved, that leaves 509 byes in the first sector.
                                    With 3 sectors total in the FAT (1536 bytes) 1022 sectors can be allocated
                                    (1536 � 3 reserved bytes = 1533 remaining bytes, 1533 / 3 bytes per group = 511 groups,
                                    2 sectors per group (12-bits each) gives 511 * 2 = 1022 sectors.
                                    ((1 * 512)-3) / 3 * 2 == 339.3 sectors == 173738.6 bytes == 169Kb
                                    ((2 * 512)-3) / 3 * 2 == 680 sectors == 348160 bytes == 340Kb
                                    ((3 * 512)-3) / 3 * 2 == 1022 sectors == 523264 bytes == 511Kb

                                    Explained ((2[Sector] * 512)-3) / 3 * 2[12bEntry] == 680 sectors == 348160 bytes == 340Kb
                              */
                0x01, 0x00,    // 1 - Sectors per track
                0x01, 0x00,    // 1 - Number of heads 
                0x00, 0x00, 0x00, 0x00, // Hidden sectors - ignore
                0x00, 0x00, 0x00, 0x00, // FAT12 mode, Total sector count for FAT32 (0 for FAT12 and FAT16), Total sectors (if greater than 65535; otherwise, see offset 0x13)

                0x80, 0x00,    // Ignore
                0x29,          // ExtendedBootSig - Boot signature - Extended boot signature. This is a signature byte that indicates that the following three fields in the boot sector are present.
                                // The value should be 0x29 to indicate that.
                0x78, 0x56, 0x34, 0x12,  // Volume ID or Volume_serial_number = 0x12345678 [4]; // MSX-DOS 2 disk serial number (default: 0x00000000).
                (byte)'f', (byte)'D', (byte)'r', (byte)'i', (byte)'v', (byte)'e', (byte)' ', (byte)' ', (byte)'v', (byte)'0', (byte)'1',
                0x46, 0x41, 0x54, 0x31, 0x32, 0x20, 0x20, 0x20,  // File system type (e.g. "FAT12   ", "FAT16   ")
                0x00, 0x00,

                // Zero up to 2 last bytes of FAT magic code
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,

        0x00, 0x00, 0x55, 0xAA // <<< FAT magic code -- Boot sector signature (should be 0xAA55)

        };


        public FDriveFat12BootSector()
        {

        }
        public void Populate(List<byte> buffer)
        {
        }

        //                              123456789012345678901234567890

        public List<byte> GenerateBuffer()
        {
            return BufferUtils.GenerateBuffer(_data);
        }
    }
}