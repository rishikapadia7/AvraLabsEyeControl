namespace PrecisionGazeMouse
{
    partial class PrecisionGazeMouseForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PrecisionGazeMouseForm));
            this.calibrationAdjusterAutoButton = new System.Windows.Forms.Button();
            this.configurationManagerOpenButton = new System.Windows.Forms.Button();
            this.pauseTrackingButton = new System.Windows.Forms.Button();
            this.displayHotplugTextbox = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.displayInfoTextbox = new System.Windows.Forms.TextBox();
            this.checkCalButton = new System.Windows.Forms.Button();
            this.clearCalButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // calibrationAdjusterAutoButton
            // 
            this.calibrationAdjusterAutoButton.AutoEllipsis = true;
            this.calibrationAdjusterAutoButton.BackColor = System.Drawing.Color.White;
            this.calibrationAdjusterAutoButton.Location = new System.Drawing.Point(251, 238);
            this.calibrationAdjusterAutoButton.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.calibrationAdjusterAutoButton.Name = "calibrationAdjusterAutoButton";
            this.calibrationAdjusterAutoButton.Size = new System.Drawing.Size(605, 191);
            this.calibrationAdjusterAutoButton.TabIndex = 28;
            this.calibrationAdjusterAutoButton.Text = "Calibrate";
            this.calibrationAdjusterAutoButton.UseVisualStyleBackColor = false;
            this.calibrationAdjusterAutoButton.Click += new System.EventHandler(this.calibrationAdjusterAutoButton_Click);
            // 
            // configurationManagerOpenButton
            // 
            this.configurationManagerOpenButton.AutoEllipsis = true;
            this.configurationManagerOpenButton.BackColor = System.Drawing.Color.White;
            this.configurationManagerOpenButton.Location = new System.Drawing.Point(928, 238);
            this.configurationManagerOpenButton.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.configurationManagerOpenButton.Name = "configurationManagerOpenButton";
            this.configurationManagerOpenButton.Size = new System.Drawing.Size(635, 191);
            this.configurationManagerOpenButton.TabIndex = 29;
            this.configurationManagerOpenButton.TabStop = false;
            this.configurationManagerOpenButton.Text = "Settings";
            this.configurationManagerOpenButton.UseVisualStyleBackColor = false;
            this.configurationManagerOpenButton.Click += new System.EventHandler(this.configurationManagerOpenButton_Click);
            // 
            // pauseTrackingButton
            // 
            this.pauseTrackingButton.AutoEllipsis = true;
            this.pauseTrackingButton.BackColor = System.Drawing.Color.White;
            this.pauseTrackingButton.Location = new System.Drawing.Point(928, 761);
            this.pauseTrackingButton.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.pauseTrackingButton.Name = "pauseTrackingButton";
            this.pauseTrackingButton.Size = new System.Drawing.Size(635, 191);
            this.pauseTrackingButton.TabIndex = 46;
            this.pauseTrackingButton.Text = "Stop";
            this.pauseTrackingButton.UseVisualStyleBackColor = false;
            this.pauseTrackingButton.Click += new System.EventHandler(this.pauseTrackingButton_Click);
            // 
            // displayHotplugTextbox
            // 
            this.displayHotplugTextbox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.displayHotplugTextbox.ForeColor = System.Drawing.Color.Red;
            this.displayHotplugTextbox.Location = new System.Drawing.Point(928, 29);
            this.displayHotplugTextbox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.displayHotplugTextbox.Multiline = true;
            this.displayHotplugTextbox.Name = "displayHotplugTextbox";
            this.displayHotplugTextbox.ReadOnly = true;
            this.displayHotplugTextbox.Size = new System.Drawing.Size(628, 169);
            this.displayHotplugTextbox.TabIndex = 49;
            this.displayHotplugTextbox.Text = "If you connect or remove displays while logged in, sometimes Windows does not det" +
    "ect the change and hence Zoombox will not work correctly.  If you experience iss" +
    "ues, please log out and log back in.";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::PrecisionGazeMouse.Properties.Resources.AvraLogoTransparent;
            this.pictureBox1.Location = new System.Drawing.Point(376, 29);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(352, 136);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 50;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(29, 29);
            this.label4.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(192, 32);
            this.label4.TabIndex = 52;
            this.label4.Text = "Rishi Kapadia";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(32, 72);
            this.label5.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(265, 32);
            this.label5.TabIndex = 53;
            this.label5.Text = "rishi@avralabs.com";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(32, 119);
            this.label6.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(209, 32);
            this.label6.TabIndex = 53;
            this.label6.Text = "";
            // 
            // displayInfoTextbox
            // 
            this.displayInfoTextbox.Location = new System.Drawing.Point(309, 992);
            this.displayInfoTextbox.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.displayInfoTextbox.Multiline = true;
            this.displayInfoTextbox.Name = "displayInfoTextbox";
            this.displayInfoTextbox.Size = new System.Drawing.Size(1137, 116);
            this.displayInfoTextbox.TabIndex = 54;
            // 
            // checkCalButton
            // 
            this.checkCalButton.AutoEllipsis = true;
            this.checkCalButton.BackColor = System.Drawing.Color.White;
            this.checkCalButton.Location = new System.Drawing.Point(251, 508);
            this.checkCalButton.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.checkCalButton.Name = "checkCalButton";
            this.checkCalButton.Size = new System.Drawing.Size(605, 191);
            this.checkCalButton.TabIndex = 28;
            this.checkCalButton.Text = "Check Cal.";
            this.checkCalButton.UseVisualStyleBackColor = false;
            this.checkCalButton.Click += new System.EventHandler(this.CheckCalButton_Click);
            // 
            // clearCalButton
            // 
            this.clearCalButton.AutoEllipsis = true;
            this.clearCalButton.BackColor = System.Drawing.Color.White;
            this.clearCalButton.Location = new System.Drawing.Point(928, 508);
            this.clearCalButton.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.clearCalButton.Name = "clearCalButton";
            this.clearCalButton.Size = new System.Drawing.Size(605, 191);
            this.clearCalButton.TabIndex = 28;
            this.clearCalButton.Text = "Clear Cal.";
            this.clearCalButton.UseVisualStyleBackColor = false;
            this.clearCalButton.Click += new System.EventHandler(this.ClearCalButton_Click);
            // 
            // PrecisionGazeMouseForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.AutoScrollMargin = new System.Drawing.Size(50, 50);
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1792, 1371);
            this.Controls.Add(this.displayInfoTextbox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.displayHotplugTextbox);
            this.Controls.Add(this.pauseTrackingButton);
            this.Controls.Add(this.configurationManagerOpenButton);
            this.Controls.Add(this.clearCalButton);
            this.Controls.Add(this.checkCalButton);
            this.Controls.Add(this.calibrationAdjusterAutoButton);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.MaximizeBox = false;
            this.Name = "PrecisionGazeMouseForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Avra";
            this.Activated += new System.EventHandler(this.PrecisionGazeMouseForm_Activated);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button calibrationAdjusterAutoButton;
        private System.Windows.Forms.Button configurationManagerOpenButton;
        private System.Windows.Forms.Button pauseTrackingButton;
        private System.Windows.Forms.TextBox displayHotplugTextbox;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox displayInfoTextbox;
        private System.Windows.Forms.Button checkCalButton;
        private System.Windows.Forms.Button clearCalButton;
    }
}

