
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

namespace MadeInTheUSB.FT232H.Flash.WinApp
{
    public partial class Form1 : Form
    {
        public IDigitalWriteRead _gpios;
        public ISPI _spi;
        FT232HDetectorInformation _ft232Device;
        GpioSpiDevice _gpioSpiDevice;
        FlashMemory _flash;

        public bool HardwareDetected => this._spi != null;

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

        public ISPI DetectFT232H()
        {
            if (_ft232Device == null)
            {
                _ft232Device = FT232HDetector.Detect();
                if (_ft232Device.Ok)
                {
                    this.ShowUser($"FT232H:{_ft232Device}");
                    // MCP3088 and MAX7219 is limited to 10Mhz
                    var clockSpeed = this.rbMhz30.Checked ? MpsseSpiConfig._30Mhz : MpsseSpiConfig._10Mhz;
                    _gpioSpiDevice = new GpioSpiDevice(MpsseSpiConfig.Make(clockSpeed));
                    _gpios = _gpioSpiDevice.GPIO;
                    _spi = _gpioSpiDevice.SPI;
                    return _gpioSpiDevice.SPI;
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
                return _spi;
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
            DetectFlashIfNeeded();
        }

        private void DetectIfNeeded()
        {
            if (!this.HardwareDetected)
            {
                this._spi = DetectFT232H();
            }
        }

        private void DetectFlashIfNeeded()
        {
            if (_flash == null)
            {
                _flash = new FlashMemory(_spi);
                _flash.ReadIdentification( );
                this.ShowUser($"");
                this.ShowUser($"FLASH: {_flash.GetDeviceInfo()}");
            }
        }

        private void ft232HDetect_Click(object sender, EventArgs e)
        {
            this._spi = DetectFT232H();
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.txtOutput.Text = "";
        }

        

        private void fat12WriteDiskToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DetectIfNeeded();
            DetectFlashIfNeeded();

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
                _flash.WritePages(fDriveFS._bootSector, buffer, verify: true, format: true);
            }

            this.ShowUser($"FAT12 Disk Created Filename: {fileName}");

            ShowBinary(fileName);
        }
       
        private void fat12ReadDiskToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DetectIfNeeded();
            DetectFlashIfNeeded();
            var fDriveFS = new FDriveFAT12FileSystem();

            var buffer = new List<byte>();

            var fileSize = 52224;
            var sectorToRead = (fileSize / 512) + 1;


            _flash.ReadPages(fDriveFS._bootSector, sectorToRead*512, buffer);

            var fileName = fDriveFS.WriteFlashContentToLocalFile("flash.fat12.bin", buffer);
            this.ShowUser($"Flash Content Saved FileName: {fileName}");

            ShowBinary(fileName);
        }

        private void ShowBinary(string fileName)
        {
            var bg = new BinaryToTextGenerator(fileName);
            var bgOptions = new BinaryViewerOption { };
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
            _flash = new FlashMemory(_spi);
            _flash.ReadIdentification(FlashMemory.FLASH_DEVICE_ID.EEPROM_25AA1024_128Kb);
            this.ShowUser($"");
            this.ShowUser($"EEPROM: {_flash.GetDeviceInfo()}");
            
        }
    }
}
