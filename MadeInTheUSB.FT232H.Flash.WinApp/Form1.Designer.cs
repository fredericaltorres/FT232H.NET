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
            this.fT232HToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.ft232HDetect = new System.Windows.Forms.ToolStripMenuItem();
            this.ft232HInfoClick = new System.Windows.Forms.ToolStripMenuItem();
            this.fLASHToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.flashInfo = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fAT12ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fat12ReadDiskToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fat12WriteDiskToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.fT232HToolStripMenuItem1,
            this.fLASHToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1067, 28);
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
            this.quitToolStripMenuItem.Size = new System.Drawing.Size(106, 24);
            this.quitToolStripMenuItem.Text = "Quit";
            this.quitToolStripMenuItem.Click += new System.EventHandler(this.quitToolStripMenuItem_Click);
            // 
            // fT232HToolStripMenuItem1
            // 
            this.fT232HToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ft232HDetect,
            this.ft232HInfoClick});
            this.fT232HToolStripMenuItem1.Name = "fT232HToolStripMenuItem1";
            this.fT232HToolStripMenuItem1.Size = new System.Drawing.Size(71, 24);
            this.fT232HToolStripMenuItem1.Text = "FT232H";
            // 
            // ft232HDetect
            // 
            this.ft232HDetect.Name = "ft232HDetect";
            this.ft232HDetect.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.ft232HDetect.Size = new System.Drawing.Size(180, 24);
            this.ft232HDetect.Text = "Detect";
            this.ft232HDetect.Click += new System.EventHandler(this.ft232HDetect_Click);
            // 
            // ft232HInfoClick
            // 
            this.ft232HInfoClick.Name = "ft232HInfoClick";
            this.ft232HInfoClick.Size = new System.Drawing.Size(180, 24);
            this.ft232HInfoClick.Text = "Info";
            this.ft232HInfoClick.Click += new System.EventHandler(this.ft232HInfoClick_Click);
            // 
            // fLASHToolStripMenuItem
            // 
            this.fLASHToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.flashInfo,
            this.fAT12ToolStripMenuItem});
            this.fLASHToolStripMenuItem.Name = "fLASHToolStripMenuItem";
            this.fLASHToolStripMenuItem.Size = new System.Drawing.Size(64, 24);
            this.fLASHToolStripMenuItem.Text = "FLASH";
            // 
            // txtOutput
            // 
            this.txtOutput.Font = new System.Drawing.Font("Consolas", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtOutput.Location = new System.Drawing.Point(0, 43);
            this.txtOutput.Multiline = true;
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.ReadOnly = true;
            this.txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtOutput.Size = new System.Drawing.Size(837, 330);
            this.txtOutput.TabIndex = 2;
            // 
            // flashInfo
            // 
            this.flashInfo.Name = "flashInfo";
            this.flashInfo.Size = new System.Drawing.Size(180, 24);
            this.flashInfo.Text = "Info";
            this.flashInfo.Click += new System.EventHandler(this.flashInfo_Click);
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
            this.clearToolStripMenuItem.Size = new System.Drawing.Size(180, 24);
            this.clearToolStripMenuItem.Text = "Clear";
            this.clearToolStripMenuItem.Click += new System.EventHandler(this.clearToolStripMenuItem_Click);
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
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1067, 524);
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
    }
}

