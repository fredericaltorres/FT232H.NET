using System;
using System.Collections.Generic;
using System.Linq;

using System.IO;
using static MadeInTheUSB.FAT12.FDriveDirectoryEntry;

namespace MadeInTheUSB.FAT12
{
    public class FDriveFAT12FileSystem
    {
        public const string BLANK_SECTOR_COMMAND = @"#blank_sector";
        
        public string _tempFolder { get; }

        FDriveDirectory _directory;

        public int _bootSector = 0;
        int _startSector = 2;

        public FDriveFAT12FileSystem(string tempFolder = @"c:\temp")
        {
            _tempFolder = tempFolder;
        }

        public string GetTempDirectory()
        {
            var d = Path.Combine(_tempFolder, "fDrive");
            if(!Directory.Exists(d))
                Directory.CreateDirectory(d);
            return d;
        }

        private string WriteEntireDiskToFile(string fileName, List<byte> buffer)
        {
            fileName = Path.Combine(GetTempDirectory(), fileName);
            if (File.Exists(fileName))
                File.Delete(fileName);
            File.WriteAllBytes(fileName, buffer.ToArray());
            return fileName;
        }

        private void Trace(string s)
        {
            System.Console.WriteLine($"{s}");
        }

        public string WriteFlashContentToLocalFile(string fileNameOnly, List<byte> buffer)
        {
            return WriteEntireDiskToFile(fileNameOnly, buffer);
        }

        public string WriteFiles(List<string> files, string volumeName, int fatLinkedListSectorCount)
        {
            Trace($"Initializing FLASH with {files.Count} files");
            _directory = new FDriveDirectory();
            var currentSector = _startSector;

            _directory.Add(new FDriveDirectoryEntry { FileAttribute = FILE_ATTRIBUTE.FA_VOLUME_LABEL, 
                FileName = Path.GetFileNameWithoutExtension(volumeName), 
                Extention = Path.GetExtension(volumeName), LocalFileName = null  });

            foreach (var file in files)
            {
                if (file == FDriveFAT12FileSystem.BLANK_SECTOR_COMMAND)
                {
                    Trace($"Write command:{file}");
                    var directoryEntry = new FDriveDirectoryEntry
                    {
                        FileName = FDriveFAT12FileSystem.BLANK_SECTOR_COMMAND,
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

            var fat12LinkedList = new FDriveFat12Linkedlist(fatLinkedListSectorCount);
            var fat12LinkedListBuffer = fat12LinkedList.GenerateBuffer();
            fat12Buffer.AddRange(fat12LinkedListBuffer);

            var directoryBuffer = _directory.GenerateBuffer();
            fat12Buffer.AddRange(directoryBuffer);

            var filesDataBuffer = _directory.GenerateFilesDataBuffer();
            fat12Buffer.AddRange(filesDataBuffer);

            var r = WriteEntireDiskToFile("fat12.bin", fat12Buffer);
            
            return r;
        }

    }
}
