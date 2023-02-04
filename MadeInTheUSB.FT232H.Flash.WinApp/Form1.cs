using fLogViewer.PlugIns.FileProviders.BinaryFileProvider;
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
        }

        public ISPI DetectFT232H()
        {
            _ft232Device = FT232HDetector.Detect();
            if (_ft232Device.Ok)
            {
                this.ShowUser($"FT232H:{_ft232Device}");
                // MCP3088 and MAX7219 is limited to 10Mhz
                var clockSpeed =  this.rbMhz30.Checked ? MpsseSpiConfig._30Mhz : MpsseSpiConfig._10Mhz;
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

   

        private void Form1_Resize(object sender, EventArgs e)
        {
            this.txtOutput.Top = 32+64;
            this.txtOutput.Height = this.Height - 64-16 - 64;
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
                _flash.ReadIdentification();
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
            _flash.ReadPages(fDriveFS._bootSector, 16 * 1024, buffer);

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
    }
}
