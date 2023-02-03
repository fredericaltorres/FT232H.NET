using System;
using System.Collections.Generic;
using System.Linq;
using MadeInTheUSB.FT232H.Components;
using System.IO;
using static MadeInTheUSB.FT232H.Console.FDriveDirectoryEntry;

namespace MadeInTheUSB.FT232H.Console
{
    public class FDriveFileSystem
    {
        public const string BLANK_SECTOR_COMMAND = @"#blank_sector";
        public CypressFlashMemory _flash { get; }
        FDriveDirectory _directory;

        int _bootSector = 0;
        int _startSector = 2;

        public FDriveFileSystem(CypressFlashMemory flash)
        {
            _flash = flash;
        }

        public string GetTempDirectory()
        {
            var d = Path.Combine(@"c:\temp", "fDrive");
            if(!Directory.Exists(d))
                Directory.CreateDirectory(d);
            return d;
        }

        private void WriteEntireDiskToFile(string fileName, List<byte> buffer)
        {
            fileName = Path.Combine(GetTempDirectory(), fileName);
            if (File.Exists(fileName))
                File.Delete(fileName);
            File.WriteAllBytes(fileName, buffer.ToArray());
        }

        private void Trace(string s)
        {
            System.Console.WriteLine($"{s}");
        }

        public void WriteFlashContentToLocalFile(string fileNameOnly)
        {
            var buffer = new List<byte>();
            _flash.ReadPages(_bootSector, 16*1024, buffer);
            WriteEntireDiskToFile(fileNameOnly, buffer);
        }

        public void WriteFiles(List<string> files)
        {
            Trace($"Initializing FLASH with {files.Count} files");
            _directory = new FDriveDirectory();
            var currentSector = _startSector;

            _directory.Add(new FDriveDirectoryEntry { FileAttribute = FILE_ATTRIBUTE.FA_VOLUME_LABEL, FileName = "fDrive--", Extention = "v01", LocalFileName = null  });

            foreach (var file in files)
            {
                if (file == FDriveFileSystem.BLANK_SECTOR_COMMAND)
                {
                    Trace($"Write command:{file}");
                    var directoryEntry = new FDriveDirectoryEntry
                    {
                        FileName = FDriveFileSystem.BLANK_SECTOR_COMMAND,
                        Extention = null,
                        FirstLogicalSector = (ushort)currentSector,
                        FileSize = FDriveDirectory.SECTOR_SIZE,
                    };
                    _directory.Add(directoryEntry);
                    currentSector += 1;
                }
                else
                {
                    var fi = new FileInfo(file);
                    var len = fi.Length;
                    Trace($"Write file:{Path.GetFileName(file)}, len:{len}");
                    var directoryEntry = new FDriveDirectoryEntry
                    {
                        FileName = Path.GetFileNameWithoutExtension(file),
                        Extention = Path.GetExtension(file).Replace(".", ""),
                        FirstLogicalSector = (ushort)currentSector,
                        FileSize = (Int32)len,
                        LocalFileName = file
                    };
                    _directory.Add(directoryEntry);
                    currentSector += directoryEntry.SectorCount;
                }
            }

            var bootSector = new FDriveFat12BootSector();
            var bootSectorBuffer = bootSector.GenerateBuffer();
            var fat12Buffer = bootSector.GenerateBuffer();

            var fat12LinkedList = new FDriveFat12Linkedlist();
            var fat12LinkedListBuffer = fat12LinkedList.GenerateBuffer();
            fat12Buffer.AddRange(fat12LinkedListBuffer);

            var directoryBuffer = _directory.GenerateBuffer();
            fat12Buffer.AddRange(directoryBuffer);

            var filesDataBuffer = _directory.GenerateFilesDataBuffer();
            fat12Buffer.AddRange(filesDataBuffer);

            WriteEntireDiskToFile("fat12.bin", fat12Buffer);
            WriteEntireDiskToFlash(fat12Buffer);
        }

        private bool WriteEntireDiskToFlash(List<byte> buffer)
        {
            return WriteSector512(_bootSector, buffer);
        }
        
        private bool WriteSector512(int addr, List<byte> buffer)
        {
            return _flash.WritePages(addr, buffer, verify: true, format: true);
        }
    }
}
