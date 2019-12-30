namespace PrecisionGazeMouse
{
    partial class UserSettingsForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.standardMouseButton = new System.Windows.Forms.RadioButton();
            this.singleSwitchButton = new System.Windows.Forms.RadioButton();
            this.dwellButton = new System.Windows.Forms.RadioButton();
            this.typingMethodLabel = new System.Windows.Forms.Label();
            this.onscreenKeyboardButton = new System.Windows.Forms.RadioButton();
            this.voiceRecognitionButton = new System.Windows.Forms.RadioButton();
            this.standardKeyboardButton = new System.Windows.Forms.RadioButton();
            this.typingMethodPanel = new System.Windows.Forms.Panel();
            this.clickingMethodPanel = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.noDockbarButton = new System.Windows.Forms.RadioButton();
            this.clickButton = new System.Windows.Forms.RadioButton();
            this.quickLookButton = new System.Windows.Forms.RadioButton();
            this.dockbarSelectionLabel = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.mainPage = new System.Windows.Forms.TabPage();
            this.dwellPage = new System.Windows.Forms.TabPage();
            this.label2 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.dwellSpeedSlow = new System.Windows.Forms.RadioButton();
            this.dwellSpeedMedium = new System.Windows.Forms.RadioButton();
            this.dwellSpeedFast = new System.Windows.Forms.RadioButton();
            this.panel3 = new System.Windows.Forms.Panel();
            this.typingSpeedSlow = new System.Windows.Forms.RadioButton();
            this.typingSpeedMedium = new System.Windows.Forms.RadioButton();
            this.typingSpeedFast = new System.Windows.Forms.RadioButton();
            this.label3 = new System.Windows.Forms.Label();
            this.backButton = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.typingMethodPanel.SuspendLayout();
            this.clickingMethodPanel.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.mainPage.SuspendLayout();
            this.dwellPage.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Malgun Gothic", 14F);
            this.label1.Location = new System.Drawing.Point(30, 90);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(154, 25);
            this.label1.TabIndex = 52;
            this.label1.Text = "Clicking Method";
            // 
            // standardMouseButton
            // 
            this.standardMouseButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.standardMouseButton.Font = new System.Drawing.Font("Malgun Gothic", 11F);
            this.standardMouseButton.Location = new System.Drawing.Point(22, 16);
            this.standardMouseButton.Name = "standardMouseButton";
            this.standardMouseButton.Size = new System.Drawing.Size(120, 60);
            this.standardMouseButton.TabIndex = 53;
            this.standardMouseButton.TabStop = true;
            this.standardMouseButton.Text = "Standard Mouse";
            this.standardMouseButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.standardMouseButton.UseVisualStyleBackColor = true;
            this.standardMouseButton.CheckedChanged += new System.EventHandler(this.ClickMethod_CheckedChanged);
            // 
            // singleSwitchButton
            // 
            this.singleSwitchButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.singleSwitchButton.Font = new System.Drawing.Font("Malgun Gothic", 11F);
            this.singleSwitchButton.Location = new System.Drawing.Point(171, 16);
            this.singleSwitchButton.Name = "singleSwitchButton";
            this.singleSwitchButton.Size = new System.Drawing.Size(120, 60);
            this.singleSwitchButton.TabIndex = 53;
            this.singleSwitchButton.TabStop = true;
            this.singleSwitchButton.Text = "Single Switch";
            this.singleSwitchButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.singleSwitchButton.UseVisualStyleBackColor = true;
            this.singleSwitchButton.CheckedChanged += new System.EventHandler(this.ClickMethod_CheckedChanged);
            // 
            // dwellButton
            // 
            this.dwellButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.dwellButton.Font = new System.Drawing.Font("Malgun Gothic", 11F);
            this.dwellButton.Location = new System.Drawing.Point(324, 16);
            this.dwellButton.Name = "dwellButton";
            this.dwellButton.Size = new System.Drawing.Size(120, 60);
            this.dwellButton.TabIndex = 53;
            this.dwellButton.TabStop = true;
            this.dwellButton.Text = "Dwell";
            this.dwellButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.dwellButton.UseVisualStyleBackColor = true;
            this.dwellButton.CheckedChanged += new System.EventHandler(this.ClickMethod_CheckedChanged);
            // 
            // typingMethodLabel
            // 
            this.typingMethodLabel.AutoSize = true;
            this.typingMethodLabel.Font = new System.Drawing.Font("Malgun Gothic", 14F);
            this.typingMethodLabel.Location = new System.Drawing.Point(14, 0);
            this.typingMethodLabel.Name = "typingMethodLabel";
            this.typingMethodLabel.Size = new System.Drawing.Size(143, 25);
            this.typingMethodLabel.TabIndex = 52;
            this.typingMethodLabel.Text = "Typing Method";
            // 
            // onscreenKeyboardButton
            // 
            this.onscreenKeyboardButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.onscreenKeyboardButton.Font = new System.Drawing.Font("Malgun Gothic", 11F);
            this.onscreenKeyboardButton.Location = new System.Drawing.Point(161, 11);
            this.onscreenKeyboardButton.Name = "onscreenKeyboardButton";
            this.onscreenKeyboardButton.Size = new System.Drawing.Size(120, 60);
            this.onscreenKeyboardButton.TabIndex = 53;
            this.onscreenKeyboardButton.TabStop = true;
            this.onscreenKeyboardButton.Text = " On-Screen Keyboard";
            this.onscreenKeyboardButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.onscreenKeyboardButton.UseVisualStyleBackColor = true;
            this.onscreenKeyboardButton.CheckedChanged += new System.EventHandler(this.TypingMethod_CheckedChanged);
            // 
            // voiceRecognitionButton
            // 
            this.voiceRecognitionButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.voiceRecognitionButton.Enabled = false;
            this.voiceRecognitionButton.Font = new System.Drawing.Font("Malgun Gothic", 11F);
            this.voiceRecognitionButton.Location = new System.Drawing.Point(314, 11);
            this.voiceRecognitionButton.Name = "voiceRecognitionButton";
            this.voiceRecognitionButton.Size = new System.Drawing.Size(120, 60);
            this.voiceRecognitionButton.TabIndex = 53;
            this.voiceRecognitionButton.TabStop = true;
            this.voiceRecognitionButton.Text = "Voice Recognition";
            this.voiceRecognitionButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.voiceRecognitionButton.UseVisualStyleBackColor = true;
            this.voiceRecognitionButton.CheckedChanged += new System.EventHandler(this.TypingMethod_CheckedChanged);
            // 
            // standardKeyboardButton
            // 
            this.standardKeyboardButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.standardKeyboardButton.Font = new System.Drawing.Font("Malgun Gothic", 11F);
            this.standardKeyboardButton.Location = new System.Drawing.Point(12, 11);
            this.standardKeyboardButton.Name = "standardKeyboardButton";
            this.standardKeyboardButton.Size = new System.Drawing.Size(120, 60);
            this.standardKeyboardButton.TabIndex = 53;
            this.standardKeyboardButton.TabStop = true;
            this.standardKeyboardButton.Text = "Standard Keyboard";
            this.standardKeyboardButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.standardKeyboardButton.UseVisualStyleBackColor = true;
            this.standardKeyboardButton.CheckedChanged += new System.EventHandler(this.TypingMethod_CheckedChanged);
            // 
            // typingMethodPanel
            // 
            this.typingMethodPanel.Controls.Add(this.standardKeyboardButton);
            this.typingMethodPanel.Controls.Add(this.onscreenKeyboardButton);
            this.typingMethodPanel.Controls.Add(this.voiceRecognitionButton);
            this.typingMethodPanel.Location = new System.Drawing.Point(7, 28);
            this.typingMethodPanel.Name = "typingMethodPanel";
            this.typingMethodPanel.Size = new System.Drawing.Size(545, 85);
            this.typingMethodPanel.TabIndex = 57;
            // 
            // clickingMethodPanel
            // 
            this.clickingMethodPanel.Controls.Add(this.dwellButton);
            this.clickingMethodPanel.Controls.Add(this.singleSwitchButton);
            this.clickingMethodPanel.Controls.Add(this.standardMouseButton);
            this.clickingMethodPanel.Location = new System.Drawing.Point(13, 112);
            this.clickingMethodPanel.Name = "clickingMethodPanel";
            this.clickingMethodPanel.Size = new System.Drawing.Size(459, 87);
            this.clickingMethodPanel.TabIndex = 58;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.noDockbarButton);
            this.panel1.Controls.Add(this.clickButton);
            this.panel1.Controls.Add(this.quickLookButton);
            this.panel1.Location = new System.Drawing.Point(0, 144);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(545, 76);
            this.panel1.TabIndex = 57;
            // 
            // noDockbarButton
            // 
            this.noDockbarButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.noDockbarButton.Font = new System.Drawing.Font("Malgun Gothic", 11F);
            this.noDockbarButton.Location = new System.Drawing.Point(19, 11);
            this.noDockbarButton.Name = "noDockbarButton";
            this.noDockbarButton.Size = new System.Drawing.Size(120, 60);
            this.noDockbarButton.TabIndex = 53;
            this.noDockbarButton.TabStop = true;
            this.noDockbarButton.Text = "No Dockbar";
            this.noDockbarButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.noDockbarButton.UseVisualStyleBackColor = true;
            this.noDockbarButton.CheckedChanged += new System.EventHandler(this.DockbarSelection_CheckedChanged);
            // 
            // clickButton
            // 
            this.clickButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.clickButton.Font = new System.Drawing.Font("Malgun Gothic", 11F);
            this.clickButton.Location = new System.Drawing.Point(168, 11);
            this.clickButton.Name = "clickButton";
            this.clickButton.Size = new System.Drawing.Size(120, 60);
            this.clickButton.TabIndex = 53;
            this.clickButton.TabStop = true;
            this.clickButton.Text = "Click";
            this.clickButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.clickButton.UseVisualStyleBackColor = true;
            this.clickButton.CheckedChanged += new System.EventHandler(this.DockbarSelection_CheckedChanged);
            // 
            // quickLookButton
            // 
            this.quickLookButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.quickLookButton.Font = new System.Drawing.Font("Malgun Gothic", 11F);
            this.quickLookButton.Location = new System.Drawing.Point(321, 11);
            this.quickLookButton.Name = "quickLookButton";
            this.quickLookButton.Size = new System.Drawing.Size(120, 60);
            this.quickLookButton.TabIndex = 53;
            this.quickLookButton.TabStop = true;
            this.quickLookButton.Text = "Quick Look";
            this.quickLookButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.quickLookButton.UseVisualStyleBackColor = true;
            this.quickLookButton.CheckedChanged += new System.EventHandler(this.DockbarSelection_CheckedChanged);
            // 
            // dockbarSelectionLabel
            // 
            this.dockbarSelectionLabel.AutoSize = true;
            this.dockbarSelectionLabel.Font = new System.Drawing.Font("Malgun Gothic", 14F);
            this.dockbarSelectionLabel.Location = new System.Drawing.Point(14, 116);
            this.dockbarSelectionLabel.Name = "dockbarSelectionLabel";
            this.dockbarSelectionLabel.Size = new System.Drawing.Size(169, 25);
            this.dockbarSelectionLabel.TabIndex = 52;
            this.dockbarSelectionLabel.Text = "Dockbar Selection";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.mainPage);
            this.tabControl1.Controls.Add(this.dwellPage);
            this.tabControl1.ItemSize = new System.Drawing.Size(10, 10);
            this.tabControl1.Location = new System.Drawing.Point(12, 205);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(579, 261);
            this.tabControl1.TabIndex = 59;
            // 
            // mainPage
            // 
            this.mainPage.Controls.Add(this.typingMethodLabel);
            this.mainPage.Controls.Add(this.typingMethodPanel);
            this.mainPage.Controls.Add(this.panel1);
            this.mainPage.Controls.Add(this.dockbarSelectionLabel);
            this.mainPage.Location = new System.Drawing.Point(4, 14);
            this.mainPage.Name = "mainPage";
            this.mainPage.Padding = new System.Windows.Forms.Padding(3);
            this.mainPage.Size = new System.Drawing.Size(571, 243);
            this.mainPage.TabIndex = 0;
            this.mainPage.UseVisualStyleBackColor = true;
            // 
            // dwellPage
            // 
            this.dwellPage.Controls.Add(this.label2);
            this.dwellPage.Controls.Add(this.panel2);
            this.dwellPage.Controls.Add(this.panel3);
            this.dwellPage.Controls.Add(this.label3);
            this.dwellPage.Location = new System.Drawing.Point(4, 14);
            this.dwellPage.Name = "dwellPage";
            this.dwellPage.Padding = new System.Windows.Forms.Padding(3);
            this.dwellPage.Size = new System.Drawing.Size(571, 243);
            this.dwellPage.TabIndex = 1;
            this.dwellPage.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Malgun Gothic", 14F);
            this.label2.Location = new System.Drawing.Point(14, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(119, 25);
            this.label2.TabIndex = 58;
            this.label2.Text = "Dwell Speed";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.dwellSpeedSlow);
            this.panel2.Controls.Add(this.dwellSpeedMedium);
            this.panel2.Controls.Add(this.dwellSpeedFast);
            this.panel2.Location = new System.Drawing.Point(7, 28);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(545, 75);
            this.panel2.TabIndex = 60;
            // 
            // dwellSpeedSlow
            // 
            this.dwellSpeedSlow.Appearance = System.Windows.Forms.Appearance.Button;
            this.dwellSpeedSlow.Font = new System.Drawing.Font("Malgun Gothic", 11F);
            this.dwellSpeedSlow.Location = new System.Drawing.Point(12, 3);
            this.dwellSpeedSlow.Name = "dwellSpeedSlow";
            this.dwellSpeedSlow.Size = new System.Drawing.Size(120, 60);
            this.dwellSpeedSlow.TabIndex = 53;
            this.dwellSpeedSlow.TabStop = true;
            this.dwellSpeedSlow.Text = "Slow";
            this.dwellSpeedSlow.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.dwellSpeedSlow.UseVisualStyleBackColor = true;
            this.dwellSpeedSlow.CheckedChanged += new System.EventHandler(this.DwellSpeed_CheckedChanged);
            // 
            // dwellSpeedMedium
            // 
            this.dwellSpeedMedium.Appearance = System.Windows.Forms.Appearance.Button;
            this.dwellSpeedMedium.Font = new System.Drawing.Font("Malgun Gothic", 11F);
            this.dwellSpeedMedium.Location = new System.Drawing.Point(161, 3);
            this.dwellSpeedMedium.Name = "dwellSpeedMedium";
            this.dwellSpeedMedium.Size = new System.Drawing.Size(120, 60);
            this.dwellSpeedMedium.TabIndex = 53;
            this.dwellSpeedMedium.TabStop = true;
            this.dwellSpeedMedium.Text = "Medium";
            this.dwellSpeedMedium.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.dwellSpeedMedium.UseVisualStyleBackColor = true;
            this.dwellSpeedMedium.CheckedChanged += new System.EventHandler(this.DwellSpeed_CheckedChanged);
            // 
            // dwellSpeedFast
            // 
            this.dwellSpeedFast.Appearance = System.Windows.Forms.Appearance.Button;
            this.dwellSpeedFast.Font = new System.Drawing.Font("Malgun Gothic", 11F);
            this.dwellSpeedFast.Location = new System.Drawing.Point(314, 3);
            this.dwellSpeedFast.Name = "dwellSpeedFast";
            this.dwellSpeedFast.Size = new System.Drawing.Size(120, 60);
            this.dwellSpeedFast.TabIndex = 53;
            this.dwellSpeedFast.TabStop = true;
            this.dwellSpeedFast.Text = "Fast";
            this.dwellSpeedFast.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.dwellSpeedFast.UseVisualStyleBackColor = true;
            this.dwellSpeedFast.CheckedChanged += new System.EventHandler(this.DwellSpeed_CheckedChanged);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.typingSpeedSlow);
            this.panel3.Controls.Add(this.typingSpeedMedium);
            this.panel3.Controls.Add(this.typingSpeedFast);
            this.panel3.Location = new System.Drawing.Point(7, 133);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(545, 104);
            this.panel3.TabIndex = 61;
            // 
            // typingSpeedSlow
            // 
            this.typingSpeedSlow.Appearance = System.Windows.Forms.Appearance.Button;
            this.typingSpeedSlow.Font = new System.Drawing.Font("Malgun Gothic", 11F);
            this.typingSpeedSlow.Location = new System.Drawing.Point(12, 11);
            this.typingSpeedSlow.Name = "typingSpeedSlow";
            this.typingSpeedSlow.Size = new System.Drawing.Size(120, 60);
            this.typingSpeedSlow.TabIndex = 53;
            this.typingSpeedSlow.TabStop = true;
            this.typingSpeedSlow.Text = "Slow";
            this.typingSpeedSlow.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.typingSpeedSlow.UseVisualStyleBackColor = true;
            this.typingSpeedSlow.CheckedChanged += new System.EventHandler(this.TypingSpeed_CheckedChanged);
            // 
            // typingSpeedMedium
            // 
            this.typingSpeedMedium.Appearance = System.Windows.Forms.Appearance.Button;
            this.typingSpeedMedium.Font = new System.Drawing.Font("Malgun Gothic", 11F);
            this.typingSpeedMedium.Location = new System.Drawing.Point(161, 11);
            this.typingSpeedMedium.Name = "typingSpeedMedium";
            this.typingSpeedMedium.Size = new System.Drawing.Size(120, 60);
            this.typingSpeedMedium.TabIndex = 53;
            this.typingSpeedMedium.TabStop = true;
            this.typingSpeedMedium.Text = "Medium";
            this.typingSpeedMedium.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.typingSpeedMedium.UseVisualStyleBackColor = true;
            this.typingSpeedMedium.CheckedChanged += new System.EventHandler(this.TypingSpeed_CheckedChanged);
            // 
            // typingSpeedFast
            // 
            this.typingSpeedFast.Appearance = System.Windows.Forms.Appearance.Button;
            this.typingSpeedFast.Font = new System.Drawing.Font("Malgun Gothic", 11F);
            this.typingSpeedFast.Location = new System.Drawing.Point(314, 11);
            this.typingSpeedFast.Name = "typingSpeedFast";
            this.typingSpeedFast.Size = new System.Drawing.Size(120, 60);
            this.typingSpeedFast.TabIndex = 53;
            this.typingSpeedFast.TabStop = true;
            this.typingSpeedFast.Text = "Fast";
            this.typingSpeedFast.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.typingSpeedFast.UseVisualStyleBackColor = true;
            this.typingSpeedFast.CheckedChanged += new System.EventHandler(this.TypingSpeed_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Malgun Gothic", 14F);
            this.label3.Location = new System.Drawing.Point(14, 106);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(325, 25);
            this.label3.TabIndex = 59;
            this.label3.Text = "Typing Speed (On-Screen Keyboard)";
            // 
            // backButton
            // 
            this.backButton.Font = new System.Drawing.Font("Malgun Gothic", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.backButton.Image = global::PrecisionGazeMouse.Properties.Resources.backarrow;
            this.backButton.Location = new System.Drawing.Point(14, 12);
            this.backButton.Name = "backButton";
            this.backButton.Size = new System.Drawing.Size(118, 57);
            this.backButton.TabIndex = 55;
            this.backButton.UseVisualStyleBackColor = true;
            this.backButton.Click += new System.EventHandler(this.confirmButton_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::PrecisionGazeMouse.Properties.Resources.AvraLogoTransparent;
            this.pictureBox1.Location = new System.Drawing.Point(144, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(132, 57);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 51;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // UserSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(685, 478);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.clickingMethodPanel);
            this.Controls.Add(this.backButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox1);
            this.MaximizeBox = false;
            this.Name = "UserSettingsForm";
            this.Text = "UserSettingsForm";
            this.Load += new System.EventHandler(this.UserSettingsForm_Load);
            this.typingMethodPanel.ResumeLayout(false);
            this.clickingMethodPanel.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.mainPage.ResumeLayout(false);
            this.mainPage.PerformLayout();
            this.dwellPage.ResumeLayout(false);
            this.dwellPage.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton standardMouseButton;
        private System.Windows.Forms.RadioButton singleSwitchButton;
        private System.Windows.Forms.RadioButton dwellButton;
        private System.Windows.Forms.Label typingMethodLabel;
        private System.Windows.Forms.Button confirmButton;
        private System.Windows.Forms.RadioButton onscreenKeyboardButton;
        private System.Windows.Forms.RadioButton voiceRecognitionButton;
        private System.Windows.Forms.RadioButton standardKeyboardButton;
        private System.Windows.Forms.Panel typingMethodPanel;
        private System.Windows.Forms.Panel clickingMethodPanel;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton noDockbarButton;
        private System.Windows.Forms.RadioButton clickButton;
        private System.Windows.Forms.RadioButton quickLookButton;
        private System.Windows.Forms.Label dockbarSelectionLabel;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage mainPage;
        private System.Windows.Forms.TabPage dwellPage;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.RadioButton dwellSpeedSlow;
        private System.Windows.Forms.RadioButton dwellSpeedMedium;
        private System.Windows.Forms.RadioButton dwellSpeedFast;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.RadioButton typingSpeedSlow;
        private System.Windows.Forms.RadioButton typingSpeedMedium;
        private System.Windows.Forms.RadioButton typingSpeedFast;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button backButton;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}