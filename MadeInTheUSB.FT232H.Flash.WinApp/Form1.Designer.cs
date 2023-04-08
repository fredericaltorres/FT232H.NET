namespace MadeInTheUSB.FT232H.Flash.WinApp
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fT232HToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.ft232HDetect = new System.Windows.Forms.ToolStripMenuItem();
            this.ft232HInfoClick = new System.Windows.Forms.ToolStripMenuItem();
            this.nusbioV2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.initializeDeviceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fLASHToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.flashInfo = new System.Windows.Forms.ToolStripMenuItem();
            this.fAT12ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fat12ReadDiskToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fat12WriteDiskToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sectorsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.readToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.writeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.writeFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.eraseAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.eEPROMToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.detectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.eEPROM25AA1024ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.writeTestToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.readTestToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.grpSettings = new System.Windows.Forms.GroupBox();
            this.chkUpdateFlash = new System.Windows.Forms.CheckBox();
            this.rbMhz30 = new System.Windows.Forms.RadioButton();
            this.rbMhz10 = new System.Windows.Forms.RadioButton();
            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rbDisplaySector512 = new System.Windows.Forms.RadioButton();
            this.rbDisplaySector256 = new System.Windows.Forms.RadioButton();
            this.menuStrip1.SuspendLayout();
            this.grpSettings.SuspendLayout();
            this.statusBar.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.fT232HToolStripMenuItem1,
            this.fLASHToolStripMenuItem,
            this.eEPROMToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1152, 28);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.quitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(44, 24);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // quitToolStripMenuItem
            // 
            this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            this.quitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Q)));
            this.quitToolStripMenuItem.Size = new System.Drawing.Size(159, 24);
            this.quitToolStripMenuItem.Text = "Quit";
            this.quitToolStripMenuItem.Click += new System.EventHandler(this.quitToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(53, 24);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // clearToolStripMenuItem
            // 
            this.clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            this.clearToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F6;
            this.clearToolStripMenuItem.Size = new System.Drawing.Size(136, 24);
            this.clearToolStripMenuItem.Text = "Clear";
            this.clearToolStripMenuItem.Click += new System.EventHandler(this.clearToolStripMenuItem_Click);
            // 
            // fT232HToolStripMenuItem1
            // 
            this.fT232HToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ft232HDetect,
            this.ft232HInfoClick,
            this.nusbioV2ToolStripMenuItem});
            this.fT232HToolStripMenuItem1.Name = "fT232HToolStripMenuItem1";
            this.fT232HToolStripMenuItem1.Size = new System.Drawing.Size(71, 24);
            this.fT232HToolStripMenuItem1.Text = "FT232H";
            // 
            // ft232HDetect
            // 
            this.ft232HDetect.Name = "ft232HDetect";
            this.ft232HDetect.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.ft232HDetect.Size = new System.Drawing.Size(148, 24);
            this.ft232HDetect.Text = "Detect";
            this.ft232HDetect.Click += new System.EventHandler(this.ft232HDetect_Click);
            // 
            // ft232HInfoClick
            // 
            this.ft232HInfoClick.Name = "ft232HInfoClick";
            this.ft232HInfoClick.Size = new System.Drawing.Size(148, 24);
            this.ft232HInfoClick.Text = "Info";
            this.ft232HInfoClick.Click += new System.EventHandler(this.ft232HInfoClick_Click);
            // 
            // nusbioV2ToolStripMenuItem
            // 
            this.nusbioV2ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.initializeDeviceToolStripMenuItem});
            this.nusbioV2ToolStripMenuItem.Name = "nusbioV2ToolStripMenuItem";
            this.nusbioV2ToolStripMenuItem.Size = new System.Drawing.Size(148, 24);
            this.nusbioV2ToolStripMenuItem.Text = "Nusbio v2 ";
            // 
            // initializeDeviceToolStripMenuItem
            // 
            this.initializeDeviceToolStripMenuItem.Name = "initializeDeviceToolStripMenuItem";
            this.initializeDeviceToolStripMenuItem.Size = new System.Drawing.Size(183, 24);
            this.initializeDeviceToolStripMenuItem.Text = "Initialize Device";
            this.initializeDeviceToolStripMenuItem.Click += new System.EventHandler(this.initializeDeviceToolStripMenuItem_Click);
            // 
            // fLASHToolStripMenuItem
            // 
            this.fLASHToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.flashInfo,
            this.fAT12ToolStripMenuItem,
            this.sectorsToolStripMenuItem});
            this.fLASHToolStripMenuItem.Name = "fLASHToolStripMenuItem";
            this.fLASHToolStripMenuItem.Size = new System.Drawing.Size(64, 24);
            this.fLASHToolStripMenuItem.Text = "FLASH";
            // 
            // flashInfo
            // 
            this.flashInfo.Name = "flashInfo";
            this.flashInfo.Size = new System.Drawing.Size(180, 24);
            this.flashInfo.Text = "Info";
            this.flashInfo.Click += new System.EventHandler(this.flashInfo_Click);
            // 
            // fAT12ToolStripMenuItem
            // 
            this.fAT12ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fat12ReadDiskToolStripMenuItem,
            this.fat12WriteDiskToolStripMenuItem});
            this.fAT12ToolStripMenuItem.Name = "fAT12ToolStripMenuItem";
            this.fAT12ToolStripMenuItem.Size = new System.Drawing.Size(180, 24);
            this.fAT12ToolStripMenuItem.Text = "FAT12";
            // 
            // fat12ReadDiskToolStripMenuItem
            // 
            this.fat12ReadDiskToolStripMenuItem.Name = "fat12ReadDiskToolStripMenuItem";
            this.fat12ReadDiskToolStripMenuItem.Size = new System.Drawing.Size(180, 24);
            this.fat12ReadDiskToolStripMenuItem.Text = "Read Disk";
            this.fat12ReadDiskToolStripMenuItem.Click += new System.EventHandler(this.fat12ReadDiskToolStripMenuItem_Click);
            // 
            // fat12WriteDiskToolStripMenuItem
            // 
            this.fat12WriteDiskToolStripMenuItem.Name = "fat12WriteDiskToolStripMenuItem";
            this.fat12WriteDiskToolStripMenuItem.Size = new System.Drawing.Size(180, 24);
            this.fat12WriteDiskToolStripMenuItem.Text = "Write Disk";
            this.fat12WriteDiskToolStripMenuItem.Click += new System.EventHandler(this.fat12WriteDiskToolStripMenuItem_Click);
            // 
            // sectorsToolStripMenuItem
            // 
            this.sectorsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.readToolStripMenuItem,
            this.writeToolStripMenuItem,
            this.writeFileToolStripMenuItem,
            this.eraseAllToolStripMenuItem});
            this.sectorsToolStripMenuItem.Name = "sectorsToolStripMenuItem";
            this.sectorsToolStripMenuItem.Size = new System.Drawing.Size(180, 24);
            this.sectorsToolStripMenuItem.Text = "Sectors";
            // 
            // readToolStripMenuItem
            // 
            this.readToolStripMenuItem.Name = "readToolStripMenuItem";
            this.readToolStripMenuItem.Size = new System.Drawing.Size(180, 24);
            this.readToolStripMenuItem.Text = "Read";
            this.readToolStripMenuItem.Click += new System.EventHandler(this.readToolStripMenuItem_Click);
            // 
            // writeToolStripMenuItem
            // 
            this.writeToolStripMenuItem.Name = "writeToolStripMenuItem";
            this.writeToolStripMenuItem.Size = new System.Drawing.Size(180, 24);
            this.writeToolStripMenuItem.Text = "Write";
            this.writeToolStripMenuItem.Click += new System.EventHandler(this.writeToolStripMenuItem_Click);
            // 
            // writeFileToolStripMenuItem
            // 
            this.writeFileToolStripMenuItem.Name = "writeFileToolStripMenuItem";
            this.writeFileToolStripMenuItem.Size = new System.Drawing.Size(180, 24);
            this.writeFileToolStripMenuItem.Text = "Write File";
            this.writeFileToolStripMenuItem.Click += new System.EventHandler(this.writeFileToolStripMenuItem_Click);
            // 
            // eraseAllToolStripMenuItem
            // 
            this.eraseAllToolStripMenuItem.Name = "eraseAllToolStripMenuItem";
            this.eraseAllToolStripMenuItem.Size = new System.Drawing.Size(180, 24);
            this.eraseAllToolStripMenuItem.Text = "Erase All";
            this.eraseAllToolStripMenuItem.Click += new System.EventHandler(this.eraseAllToolStripMenuItem_Click);
            // 
            // eEPROMToolStripMenuItem
            // 
            this.eEPROMToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.detectToolStripMenuItem,
            this.writeTestToolStripMenuItem,
            this.readTestToolStripMenuItem});
            this.eEPROMToolStripMenuItem.Name = "eEPROMToolStripMenuItem";
            this.eEPROMToolStripMenuItem.Size = new System.Drawing.Size(78, 24);
            this.eEPROMToolStripMenuItem.Text = "EEPROM";
            // 
            // detectToolStripMenuItem
            // 
            this.detectToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.eEPROM25AA1024ToolStripMenuItem});
            this.detectToolStripMenuItem.Name = "detectToolStripMenuItem";
            this.detectToolStripMenuItem.Size = new System.Drawing.Size(144, 24);
            this.detectToolStripMenuItem.Text = "Detect";
            // 
            // eEPROM25AA1024ToolStripMenuItem
            // 
            this.eEPROM25AA1024ToolStripMenuItem.Name = "eEPROM25AA1024ToolStripMenuItem";
            this.eEPROM25AA1024ToolStripMenuItem.Size = new System.Drawing.Size(209, 24);
            this.eEPROM25AA1024ToolStripMenuItem.Text = "EEPROM_25AA1024";
            this.eEPROM25AA1024ToolStripMenuItem.Click += new System.EventHandler(this.eEPROM25AA1024ToolStripMenuItem_Click);
            // 
            // writeTestToolStripMenuItem
            // 
            this.writeTestToolStripMenuItem.Name = "writeTestToolStripMenuItem";
            this.writeTestToolStripMenuItem.Size = new System.Drawing.Size(144, 24);
            this.writeTestToolStripMenuItem.Text = "Write Test";
            this.writeTestToolStripMenuItem.Click += new System.EventHandler(this.writeTestToolStripMenuItem_Click);
            // 
            // readTestToolStripMenuItem
            // 
            this.readTestToolStripMenuItem.Name = "readTestToolStripMenuItem";
            this.readTestToolStripMenuItem.Size = new System.Drawing.Size(144, 24);
            this.readTestToolStripMenuItem.Text = "Read Test";
            this.readTestToolStripMenuItem.Click += new System.EventHandler(this.readTestToolStripMenuItem_Click);
            // 
            // txtOutput
            // 
            this.txtOutput.Font = new System.Drawing.Font("Consolas", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtOutput.Location = new System.Drawing.Point(12, 111);
            this.txtOutput.Multiline = true;
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.ReadOnly = true;
            this.txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtOutput.Size = new System.Drawing.Size(837, 330);
            this.txtOutput.TabIndex = 2;
            // 
            // grpSettings
            // 
            this.grpSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpSettings.Controls.Add(this.chkUpdateFlash);
            this.grpSettings.Controls.Add(this.rbMhz30);
            this.grpSettings.Controls.Add(this.rbMhz10);
            this.grpSettings.Location = new System.Drawing.Point(7, 40);
            this.grpSettings.Name = "grpSettings";
            this.grpSettings.Size = new System.Drawing.Size(337, 47);
            this.grpSettings.TabIndex = 3;
            this.grpSettings.TabStop = false;
            this.grpSettings.Text = "Settings";
            // 
            // chkUpdateFlash
            // 
            this.chkUpdateFlash.AutoSize = true;
            this.chkUpdateFlash.Location = new System.Drawing.Point(233, 19);
            this.chkUpdateFlash.Name = "chkUpdateFlash";
            this.chkUpdateFlash.Size = new System.Drawing.Size(98, 17);
            this.chkUpdateFlash.TabIndex = 2;
            this.chkUpdateFlash.Text = "Update FLASH";
            this.chkUpdateFlash.UseVisualStyleBackColor = true;
            // 
            // rbMhz30
            // 
            this.rbMhz30.AutoSize = true;
            this.rbMhz30.Location = new System.Drawing.Point(152, 18);
            this.rbMhz30.Name = "rbMhz30";
            this.rbMhz30.Size = new System.Drawing.Size(60, 17);
            this.rbMhz30.TabIndex = 1;
            this.rbMhz30.Text = "30 Mhz";
            this.rbMhz30.UseVisualStyleBackColor = true;
            // 
            // rbMhz10
            // 
            this.rbMhz10.AutoSize = true;
            this.rbMhz10.Checked = true;
            this.rbMhz10.Location = new System.Drawing.Point(30, 20);
            this.rbMhz10.Name = "rbMhz10";
            this.rbMhz10.Size = new System.Drawing.Size(60, 17);
            this.rbMhz10.TabIndex = 0;
            this.rbMhz10.TabStop = true;
            this.rbMhz10.Text = "10 Mhz";
            this.rbMhz10.UseVisualStyleBackColor = true;
            // 
            // statusBar
            // 
            this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusBar.Location = new System.Drawing.Point(0, 709);
            this.statusBar.Name = "statusBar";
            this.statusBar.Size = new System.Drawing.Size(1152, 27);
            this.statusBar.TabIndex = 4;
            this.statusBar.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Font = new System.Drawing.Font("Consolas", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(90, 22);
            this.toolStripStatusLabel1.Text = "Ready...";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.rbDisplaySector512);
            this.groupBox1.Controls.Add(this.rbDisplaySector256);
            this.groupBox1.Location = new System.Drawing.Point(350, 40);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(337, 47);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Settings";
            // 
            // rbDisplaySector512
            // 
            this.rbDisplaySector512.AutoSize = true;
            this.rbDisplaySector512.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.rbDisplaySector512.Checked = true;
            this.rbDisplaySector512.Location = new System.Drawing.Point(96, 19);
            this.rbDisplaySector512.Name = "rbDisplaySector512";
            this.rbDisplaySector512.Size = new System.Drawing.Size(43, 17);
            this.rbDisplaySector512.TabIndex = 1;
            this.rbDisplaySector512.TabStop = true;
            this.rbDisplaySector512.Text = "512";
            this.rbDisplaySector512.UseVisualStyleBackColor = true;
            // 
            // rbDisplaySector256
            // 
            this.rbDisplaySector256.AutoSize = true;
            this.rbDisplaySector256.Location = new System.Drawing.Point(30, 20);
            this.rbDisplaySector256.Name = "rbDisplaySector256";
            this.rbDisplaySector256.Size = new System.Drawing.Size(43, 17);
            this.rbDisplaySector256.TabIndex = 0;
            this.rbDisplaySector256.Text = "256";
            this.rbDisplaySector256.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1152, 736);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.statusBar);
            this.Controls.Add(this.grpSettings);
            this.Controls.Add(this.txtOutput);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FT232H Flash Programming App";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.grpSettings.ResumeLayout(false);
            this.grpSettings.PerformLayout();
            this.statusBar.ResumeLayout(false);
            this.statusBar.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fLASHToolStripMenuItem;
        private System.Windows.Forms.TextBox txtOutput;
        private System.Windows.Forms.ToolStripMenuItem fT232HToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem ft232HDetect;
        private System.Windows.Forms.ToolStripMenuItem ft232HInfoClick;
        private System.Windows.Forms.ToolStripMenuItem flashInfo;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fAT12ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fat12ReadDiskToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fat12WriteDiskToolStripMenuItem;
        private System.Windows.Forms.GroupBox grpSettings;
        private System.Windows.Forms.RadioButton rbMhz30;
        private System.Windows.Forms.RadioButton rbMhz10;
        private System.Windows.Forms.CheckBox chkUpdateFlash;
        private System.Windows.Forms.ToolStripMenuItem eEPROMToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem writeTestToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem readTestToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem detectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem eEPROM25AA1024ToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusBar;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripMenuItem sectorsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem readToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem writeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem eraseAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem writeFileToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rbDisplaySector512;
        private System.Windows.Forms.RadioButton rbDisplaySector256;
        private System.Windows.Forms.ToolStripMenuItem nusbioV2ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem initializeDeviceToolStripMenuItem;
    }
}

