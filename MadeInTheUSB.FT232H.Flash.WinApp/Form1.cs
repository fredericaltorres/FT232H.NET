
using BufferUtil;
using MadeInTheUSB.FAT12;
using MadeInTheUSB.FT232H.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static MadeInTheUSB.FT232H.SpiConfig;

namespace MadeInTheUSB.FT232H.Flash.WinApp
{
    public partial class Form1 : Form
    {
        public ISpiInterfaces _interfaces;
        FT232HDetectorInformation _ft232Device;
        SpiDevice _spiDevice;
        FlashMemory _flash;

        public bool HardwareDetected => this._interfaces != null;

        public Form1()
        {
            InitializeComponent();
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.ShowUser("Ready...");
            Form1_Resize(sender, e);
        }

        public void ShowUser(string text)
        {
            this.txtOutput.Text += text + Environment.NewLine;
            this.txtOutput.SelectionStart = this.txtOutput.TextLength;
            this.txtOutput.ScrollToCaret();
            Application.DoEvents();
        }

        public void ShowState(string text = null)
        {
            this.statusBar.Items[0].Text = text;
            Application.DoEvents();
        }

        public ISpiInterfaces DetectFT232H()
        {
            if (_ft232Device == null)
            {
                _ft232Device = FT232HDetector.Detect();
                if (_ft232Device.Ok)
                {
                    this.ShowUser($"FT232H [{_ft232Device}]");
                    // MCP3088 and MAX7219 is limited to 10Mhz
                    var clockSpeed = this.rbMhz30.Checked ? SpiClockSpeeds._30Mhz : SpiClockSpeeds._10Mhz;
                    _spiDevice = new SpiDevice(clockSpeed);
                    _spiDevice.Log = this.chkTraceProtocol.Checked;
                    _interfaces = _spiDevice.Interfaces;
                    return _spiDevice.Interfaces;
                }
                else
                {
                    this.ShowUser("FT232H not detected");
                    return null;
                }
            }
            else
            {
                this.ShowUser($"FT232H:{_ft232Device}");
                return _interfaces;
            }
        }

   

        private void Form1_Resize(object sender, EventArgs e)
        {
            this.txtOutput.Top = 32+64;
            this.txtOutput.Height = this.Height - 64-16 - 64 - 64;
            this.txtOutput.Left = 8;
            this.txtOutput.Width = this.Width - 32;

        }
      

        private void ft232HInfoClick_Click(object sender, EventArgs e)
        {
            DetectIfNeeded();
            this.ShowUser($"");

            this.ShowUser($"FT232H Properties");
            foreach (var p in _ft232Device.Properties)
                this.ShowUser($"    {p.Key}: {p.Value}");
        }

        private void flashInfo_Click(object sender, EventArgs e)
        {
            DetectIfNeeded();
            if (!DetectFlashIfNeeded()) return;
        }

        private void DetectIfNeeded()
        {
            if (!this.HardwareDetected)
            {
                this._interfaces = DetectFT232H();
            }
        }

        private bool DetectFlashIfNeeded()
        {
            if (_flash == null)
            {
                _flash = new FlashMemory(this._interfaces.Spi, SpiChipSelectPins.CsDbus3);
                _flash = new FlashMemory(this._interfaces.Spi, SpiChipSelectPins.CsDbus7);
                if (_flash.ReadIdentification())
                {
                    this.ShowUser($"FLASH: {_flash.GetInformation()}");
                    return true;
                }
                else
                {
                    this.ShowUser($"FLASH: Not detected");
                    _flash = null;
                    return false;
                }
            }
            else return true;
        }

        private void ft232HDetect_Click(object sender, EventArgs e)
        {
            this._interfaces = DetectFT232H();
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.txtOutput.Text = "";
        }

        

        private void fat12WriteDiskToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DetectIfNeeded();
            if (!DetectFlashIfNeeded()) return;

            const int fatLinkedListSectorCount = 10;
            const string volumeName = "fDrive.v01";
            var files = new List<string> {
                @"C:\DVT\LILYGO T-Display-S3 ESP32-S3\mass storage\Files\README.TXT",
                @"C:\DVT\LILYGO T-Display-S3 ESP32-S3\mass storage\Files\WRITEME.TXT",
                @"C:\DVT\LILYGO T-Display-S3 ESP32-S3\mass storage\Files\MASTER.TXT",
                FDriveFAT12FileSystem.BLANK_SECTOR_COMMAND,
                @"C:\DVT\LILYGO T-Display-S3 ESP32-S3\mass storage\Files\VIEWME.JPG",
            };

            var fDriveFS = new FDriveFAT12FileSystem();
            var fileName = fDriveFS.WriteFiles(files, volumeName, fatLinkedListSectorCount);

            if (this.chkUpdateFlash.Checked)
            {
                var buffer = File.ReadAllBytes(fileName).ToList();
                _flash.WritePages(fDriveFS._bootSector, buffer, verify: true, eraseBlock: true);
            }

            this.ShowUser($"FAT12 Disk Created Filename: {fileName}");

            ShowBinary(fileName);
        }
       
        private void fat12ReadDiskToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DetectIfNeeded();
            if (!DetectFlashIfNeeded()) return;

            var fDriveFS = new FDriveFAT12FileSystem();

            var buffer = new List<byte>();

            var fileSize = 52224;
            var sectorToRead = 20;

            _flash.ReadPages(fDriveFS._bootSector, sectorToRead*512, buffer);

            var fileName = fDriveFS.WriteFlashContentToLocalFile("flash.fat12.bin", buffer);
            this.ShowUser($"Flash Content Saved FileName: {fileName}");

            ShowBinary(fileName);
        }

        private void ShowBinary(string fileName)
        {
            var bg = new BinaryToTextGenerator(fileName);
            var bgOptions = new BinaryViewerOption { ShowSector = true };
            this.ShowUser(bg.Generate(bgOptions));
        }

        private void writeTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EEPROM_WriteTest();
        }

        private void EEPROM_WriteTest()
        {
            byte asciValue = 65;
            this.ShowUser($"EEPROM Test - About to write {_flash.MaxPage} pages");
            for (var p = 0; p < _flash.MaxPage; p++)
            {
                if(p % 10 == 0)
                    this.ShowState($"Writing page {p} {p * (int)_flash.PageSize/1024}/{_flash.MaxPage* (int)_flash.PageSize / 1024}");
                _flash.WritePages(p * (int)_flash.PageSize, BufferUtil.BufferUtils.MakeBuffer(256, asciValue));
                asciValue += 1;
                if(asciValue >= 128)
                    asciValue = 65;
            }
            this.ShowUser($"done");
        }

        private void EEPROM_ReadTest()
        {
            var asciValue = 64;
            var allBuffer = new List<byte>();
            this.ShowUser($"EEPROM Test - About to Read {_flash.MaxPage} pages");
            const int pageBatch = 10;
            for (var p = 0; p < _flash.MaxPage; p+= pageBatch)
            {
                if (p % 10 == 0)
                    this.ShowState($"Page {p}");
                var b = new List<byte>();
                _flash.ReadPages(p * (int)_flash.PageSize, (int)_flash.PageSize* pageBatch, b);
                allBuffer.AddRange(b);
            }

            var tmpFileName = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            File.WriteAllBytes(tmpFileName, allBuffer.ToArray());
            this.ShowState($"Generating view...");

            clearToolStripMenuItem_Click(null, null);

            var bg = new BinaryToTextGenerator(tmpFileName);
            this.ShowUser(bg.Generate(new BinaryViewerOption { ShowSector = true, SectorSize = (int)_flash.PageSize }));

            this.ShowState();
        }

        private void readTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EEPROM_ReadTest();
        }

        private void eEPROM25AA1024ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DetectIfNeeded();
            _flash = new FlashMemory(this._interfaces.Spi, SpiChipSelectPins.CsDbus3);
            _flash.ReadIdentification(FlashMemory.FLASH_DEVICE_ID.EEPROM_25AA1024_128Kb);
            this.ShowUser($"");
            this.ShowUser($"EEPROM: {_flash.GetInformation()}");
            
        }

        private void readToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DetectIfNeeded();
            if (!DetectFlashIfNeeded()) return;

            var buffer = new List<byte>();

            //var maxPage = Math.Min(6553, _flash.MaxPage);
            var maxPage = _flash.MaxPage;
            var pageBufferCount = 256; //  256 * 256  = 65536
            this.ShowUser($"About to read {maxPage} pages, batchSize:{pageBufferCount* _flash.PageSize}");

            var ph = new PerformanceHelper().Start();

            for (var p = 0; p < maxPage; p+= pageBufferCount)
            {
                if (p % 10 == 0)
                    this.ShowState($"{p} page loaded, {buffer.Count/1024/1024.0:0.0} Mb loaded");
                var tmpBuffer = new List<byte>();
                _flash.ReadPages(p * _flash.PageSize, pageBufferCount*_flash.PageSize, tmpBuffer);
                buffer.AddRange(tmpBuffer);
                this._interfaces.Gpios.ProgressNext();
            }
            this.ShowUser($"Data read size:{buffer.Count}");
            ph.AddByte(buffer.Count);
            this.ShowUser($"Performance: {ph.GetResultInfo()}");

            var tmpFileName = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            File.WriteAllBytes(tmpFileName, buffer.ToArray().Take(1024*64).ToArray());
            this.ShowState($"Generating view...");

            // clearToolStripMenuItem_Click(null, null);

            var bg = new BinaryToTextGenerator(tmpFileName);
            this.ShowUser(bg.Generate(new BinaryViewerOption { ShowSector = true, SectorSize = GetDisplaySectorSize() }));

            this.ShowState();
            this._interfaces.Gpios.ProgressNext(clear: true);
        }

        private int GetDisplaySectorSize()
        {
            return rbDisplaySector512.Checked ? 512 : 256;
        }

        private void writeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DetectIfNeeded();
            if (!DetectFlashIfNeeded()) return;

            byte asciValue = 65;
            this.ShowUser($"About to write {_flash.MaxPage} pages");
            var maxPage = Math.Min(_flash.MaxPage, _flash.MaxPage);

            var verify = !true;
            /// _flash.ErasePage(0, FlashMemory.ERASE_BLOCK_SIZE.BLOCK_64K);

            for (var p = 0; p < maxPage; p++)
            {
                var totalWritten = p * _flash.PageSize;
                if (p % 10 == 0)
                    this.ShowState($"Writing page {p}/{maxPage},  {totalWritten/1024/1024.0:0.0}/{_flash.SizeInByte / 1024/1024.0:0.0} Mb");

                _flash.WritePages(p * _flash.PageSize, BufferUtils.MakeBuffer(_flash.PageSize, asciValue), verify: verify, eraseBlock: true );
                asciValue += 1;
                if (asciValue >= 128)
                    asciValue = 65;
                this._interfaces.Gpios.ProgressNext();
            }
            this.ShowUser($"done");
        }

        private void eraseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you want to erase the FLASH chip?", "Erase FLASH", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                DetectIfNeeded();
                if (!DetectFlashIfNeeded()) return;

                this.ShowUser($"About to erase chip ");
                _flash.EraseFlash();
            }
        }

        private void writeFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            const string FILE_FILTERS = "Binary Files|*.bin|All Files (*.*)|*.*";
            //var fileName = GetUserOpenFile("", FILE_FILTERS, Path.GetDirectoryName(Application.ExecutablePath));
            var fileName = @"C:\DVT\LILYGO T-Display-S3 ESP32-S3\mass storage\Files\FULL_FAT.bin";
            if(fileName != null)
            {
                DetectIfNeeded();
                if (!DetectFlashIfNeeded()) return;

                var fi = new FileInfo(fileName);
                
                var fileBuffer = File.ReadAllBytes(fileName).ToList();
                var maxPage = fileBuffer.Count / _flash.PageSize;
                var pageSize = _flash.PageSize;

                this.ShowUser($"About to write file:{fileName}, size:{fi.Length}, sectors:{maxPage}");

                for (var p = 0; p < maxPage; p++)
                {
                    var pageBuffer = fileBuffer.Skip(p * pageSize).Take(pageSize).ToList();
                    var totalWritten = p * pageSize;
                    if (p % 10 == 0)
                        this.ShowState($"Writing page {p}/{maxPage} -  {totalWritten / 1024} Kb written");

                    _flash.WritePages(p * pageSize, BufferUtils.PadBuffer(pageBuffer, pageSize), verify: true, eraseBlock: true);
                }
                this.ShowUser($"done");
            }
        }

        public static string GetUserOpenFile(string strTitle, string strMask, string initialDirectory)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.CheckPathExists = true;
            openFileDialog.AddExtension = true;
            openFileDialog.Filter = strMask;
            openFileDialog.Title = strTitle;
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (!string.IsNullOrEmpty(initialDirectory))
            {
                openFileDialog.InitialDirectory = initialDirectory;
                //openFileDialog.FileName = selectedFileName;
            }

            if (openFileDialog.ShowDialog() != DialogResult.OK) return null;

            return openFileDialog.FileName;
        }

        private void initializeDeviceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _ft232Device = FT232HDetector.Detect();
            if (_ft232Device.Ok)
            {
                this.ShowUser($"FT232H:{_ft232Device}");
                this.ShowUser($"IsFT2323H: {_ft232Device.IsFT2323H}, IsNusbioV2: {_ft232Device.IsNusbioV2}");
                FT232HDetector.InitializeAsNusbioV2Device();
            }
        }
    }
}
