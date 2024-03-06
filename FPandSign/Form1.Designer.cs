namespace FPandSign
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            textBoxLog = new TextBox();
            DeviceInfoBox = new ListBox();
            checkBoxVisualization = new CheckBox();
            ImageBox = new PictureBox();
            comboBox_Position = new ComboBox();
            comboBox_Impression = new ComboBox();
            checkBoxPAD = new CheckBox();
            checkBox1000dpi = new CheckBox();
            btnAcquire = new Button();
            checkBoxFlexFlatCapture = new CheckBox();
            checkBoxFlexRollCapture = new CheckBox();
            checkBoxAltTrigger = new CheckBox();
            checkBoxAutocontrast = new CheckBox();
            radioButtonInsufficientObjectCount = new RadioButton();
            DeviceStatus = new Label();
            radioButtonInsufficientQuality = new RadioButton();
            btnCloseDevice = new Button();
            comboBox_NumObjCapture = new ComboBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            btnOpenDevice = new Button();
            btnCancelAcquire = new Button();
            btnSave = new Button();
            pictureBox1 = new PictureBox();
            button2 = new Button();
            button1 = new Button();
            LEDlightPanel = new LightPanel();
            ((System.ComponentModel.ISupportInitialize)ImageBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // textBoxLog
            // 
            textBoxLog.Location = new Point(12, 440);
            textBoxLog.Multiline = true;
            textBoxLog.Name = "textBoxLog";
            textBoxLog.Size = new Size(650, 178);
            textBoxLog.TabIndex = 0;
            // 
            // DeviceInfoBox
            // 
            DeviceInfoBox.FormattingEnabled = true;
            DeviceInfoBox.ItemHeight = 15;
            DeviceInfoBox.Location = new Point(12, 27);
            DeviceInfoBox.Name = "DeviceInfoBox";
            DeviceInfoBox.Size = new Size(297, 109);
            DeviceInfoBox.TabIndex = 1;
            // 
            // checkBoxVisualization
            // 
            checkBoxVisualization.AutoSize = true;
            checkBoxVisualization.Location = new Point(195, 329);
            checkBoxVisualization.Name = "checkBoxVisualization";
            checkBoxVisualization.Size = new Size(92, 19);
            checkBoxVisualization.TabIndex = 2;
            checkBoxVisualization.Text = "Visualization";
            checkBoxVisualization.UseVisualStyleBackColor = true;
            // 
            // ImageBox
            // 
            ImageBox.BackColor = Color.White;
            ImageBox.Location = new Point(325, 53);
            ImageBox.Name = "ImageBox";
            ImageBox.Size = new Size(337, 381);
            ImageBox.SizeMode = PictureBoxSizeMode.Zoom;
            ImageBox.TabIndex = 3;
            ImageBox.TabStop = false;
            // 
            // comboBox_Position
            // 
            comboBox_Position.FormattingEnabled = true;
            comboBox_Position.Location = new Point(12, 367);
            comboBox_Position.Name = "comboBox_Position";
            comboBox_Position.Size = new Size(297, 23);
            comboBox_Position.TabIndex = 5;
            comboBox_Position.SelectedIndexChanged += comboBox_Position_SelectedIndexChanged;
            // 
            // comboBox_Impression
            // 
            comboBox_Impression.FormattingEnabled = true;
            comboBox_Impression.Location = new Point(12, 411);
            comboBox_Impression.Name = "comboBox_Impression";
            comboBox_Impression.Size = new Size(297, 23);
            comboBox_Impression.TabIndex = 6;
            // 
            // checkBoxPAD
            // 
            checkBoxPAD.AutoSize = true;
            checkBoxPAD.Location = new Point(195, 203);
            checkBoxPAD.Name = "checkBoxPAD";
            checkBoxPAD.Size = new Size(98, 19);
            checkBoxPAD.TabIndex = 7;
            checkBoxPAD.Text = "Check of PAD";
            checkBoxPAD.UseVisualStyleBackColor = true;
            // 
            // checkBox1000dpi
            // 
            checkBox1000dpi.AutoSize = true;
            checkBox1000dpi.Location = new Point(195, 228);
            checkBox1000dpi.Name = "checkBox1000dpi";
            checkBox1000dpi.Size = new Size(67, 19);
            checkBox1000dpi.TabIndex = 8;
            checkBox1000dpi.Text = "1000dpi";
            checkBox1000dpi.UseVisualStyleBackColor = true;
            // 
            // btnAcquire
            // 
            btnAcquire.Location = new Point(12, 623);
            btnAcquire.Name = "btnAcquire";
            btnAcquire.Size = new Size(128, 46);
            btnAcquire.TabIndex = 10;
            btnAcquire.Text = "Acquire";
            btnAcquire.UseVisualStyleBackColor = true;
            btnAcquire.Click += btnAcquire_Click;
            // 
            // checkBoxFlexFlatCapture
            // 
            checkBoxFlexFlatCapture.AutoSize = true;
            checkBoxFlexFlatCapture.Location = new Point(195, 304);
            checkBoxFlexFlatCapture.Name = "checkBoxFlexFlatCapture";
            checkBoxFlexFlatCapture.Size = new Size(114, 19);
            checkBoxFlexFlatCapture.TabIndex = 11;
            checkBoxFlexFlatCapture.Text = "Flex Flat Capture";
            checkBoxFlexFlatCapture.UseVisualStyleBackColor = true;
            checkBoxFlexFlatCapture.CheckedChanged += checkBoxFlexFlatCapture_CheckedChanged;
            // 
            // checkBoxFlexRollCapture
            // 
            checkBoxFlexRollCapture.AutoSize = true;
            checkBoxFlexRollCapture.Location = new Point(195, 279);
            checkBoxFlexRollCapture.Name = "checkBoxFlexRollCapture";
            checkBoxFlexRollCapture.Size = new Size(115, 19);
            checkBoxFlexRollCapture.TabIndex = 12;
            checkBoxFlexRollCapture.Text = "Flex Roll Capture";
            checkBoxFlexRollCapture.UseVisualStyleBackColor = true;
            checkBoxFlexRollCapture.CheckedChanged += checkBoxFlexRollCapture_CheckedChanged;
            // 
            // checkBoxAltTrigger
            // 
            checkBoxAltTrigger.AutoSize = true;
            checkBoxAltTrigger.Location = new Point(15, 203);
            checkBoxAltTrigger.Name = "checkBoxAltTrigger";
            checkBoxAltTrigger.Size = new Size(83, 19);
            checkBoxAltTrigger.TabIndex = 13;
            checkBoxAltTrigger.Text = "Alt. Trigger";
            checkBoxAltTrigger.UseVisualStyleBackColor = true;
            // 
            // checkBoxAutocontrast
            // 
            checkBoxAutocontrast.AutoSize = true;
            checkBoxAutocontrast.Location = new Point(195, 254);
            checkBoxAutocontrast.Name = "checkBoxAutocontrast";
            checkBoxAutocontrast.Size = new Size(95, 19);
            checkBoxAutocontrast.TabIndex = 14;
            checkBoxAutocontrast.Text = "Autocontrast";
            checkBoxAutocontrast.UseVisualStyleBackColor = true;
            // 
            // radioButtonInsufficientObjectCount
            // 
            radioButtonInsufficientObjectCount.AutoSize = true;
            radioButtonInsufficientObjectCount.Checked = true;
            radioButtonInsufficientObjectCount.Location = new Point(15, 227);
            radioButtonInsufficientObjectCount.Name = "radioButtonInsufficientObjectCount";
            radioButtonInsufficientObjectCount.Size = new Size(142, 19);
            radioButtonInsufficientObjectCount.TabIndex = 15;
            radioButtonInsufficientObjectCount.TabStop = true;
            radioButtonInsufficientObjectCount.Text = "Insufficient Finger Cnt";
            radioButtonInsufficientObjectCount.UseVisualStyleBackColor = true;
            // 
            // DeviceStatus
            // 
            DeviceStatus.Location = new Point(12, 9);
            DeviceStatus.Name = "DeviceStatus";
            DeviceStatus.Size = new Size(297, 15);
            DeviceStatus.TabIndex = 17;
            DeviceStatus.Text = "DeviceStatus";
            // 
            // radioButtonInsufficientQuality
            // 
            radioButtonInsufficientQuality.AutoSize = true;
            radioButtonInsufficientQuality.Location = new Point(15, 253);
            radioButtonInsufficientQuality.Name = "radioButtonInsufficientQuality";
            radioButtonInsufficientQuality.Size = new Size(125, 19);
            radioButtonInsufficientQuality.TabIndex = 18;
            radioButtonInsufficientQuality.Text = "Insufficient Quality";
            radioButtonInsufficientQuality.UseVisualStyleBackColor = true;
            // 
            // btnCloseDevice
            // 
            btnCloseDevice.Location = new Point(195, 142);
            btnCloseDevice.Name = "btnCloseDevice";
            btnCloseDevice.Size = new Size(114, 34);
            btnCloseDevice.TabIndex = 19;
            btnCloseDevice.Text = "Close Device";
            btnCloseDevice.UseVisualStyleBackColor = true;
            btnCloseDevice.Click += btnCloseDevice_Click;
            // 
            // comboBox_NumObjCapture
            // 
            comboBox_NumObjCapture.FormattingEnabled = true;
            comboBox_NumObjCapture.Items.AddRange(new object[] { "1", "2", "3", "4", "5" });
            comboBox_NumObjCapture.Location = new Point(12, 322);
            comboBox_NumObjCapture.Name = "comboBox_NumObjCapture";
            comboBox_NumObjCapture.Size = new Size(122, 23);
            comboBox_NumObjCapture.TabIndex = 20;
            comboBox_NumObjCapture.Text = "1";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 304);
            label1.Name = "label1";
            label1.Size = new Size(102, 15);
            label1.TabIndex = 21;
            label1.Text = "Object to capture:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 349);
            label2.Name = "label2";
            label2.Size = new Size(53, 15);
            label2.TabIndex = 22;
            label2.Text = "Position:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 393);
            label3.Name = "label3";
            label3.Size = new Size(68, 15);
            label3.TabIndex = 23;
            label3.Text = "Impression:";
            // 
            // btnOpenDevice
            // 
            btnOpenDevice.Location = new Point(12, 142);
            btnOpenDevice.Name = "btnOpenDevice";
            btnOpenDevice.Size = new Size(114, 34);
            btnOpenDevice.TabIndex = 24;
            btnOpenDevice.Text = "Open Device";
            btnOpenDevice.UseVisualStyleBackColor = true;
            btnOpenDevice.Click += btnOpenDevice_Click;
            // 
            // btnCancelAcquire
            // 
            btnCancelAcquire.Location = new Point(159, 623);
            btnCancelAcquire.Name = "btnCancelAcquire";
            btnCancelAcquire.Size = new Size(128, 46);
            btnCancelAcquire.TabIndex = 25;
            btnCancelAcquire.Text = "Cancel Capture";
            btnCancelAcquire.UseVisualStyleBackColor = true;
            btnCancelAcquire.Click += btnCancelAcquire_Click;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(536, 624);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(126, 46);
            btnSave.TabIndex = 26;
            btnSave.Text = "SAVE";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = SystemColors.Window;
            pictureBox1.BorderStyle = BorderStyle.FixedSingle;
            pictureBox1.Location = new Point(683, 53);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(800, 381);
            pictureBox1.TabIndex = 27;
            pictureBox1.TabStop = false;
            // 
            // button2
            // 
            button2.Location = new Point(796, 22);
            button2.Name = "button2";
            button2.Size = new Size(114, 25);
            button2.TabIndex = 29;
            button2.Text = "CloseComDevice";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // button1
            // 
            button1.Location = new Point(683, 22);
            button1.Name = "button1";
            button1.Size = new Size(107, 25);
            button1.TabIndex = 28;
            button1.Text = "OpenComDevice";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // LEDlightPanel
            // 
            LEDlightPanel.BorderStyle = BorderStyle.FixedSingle;
            LEDlightPanel.LedCount = 0;
            LEDlightPanel.Location = new Point(325, 27);
            LEDlightPanel.Name = "LEDlightPanel";
            LEDlightPanel.Size = new Size(337, 27);
            LEDlightPanel.TabIndex = 9;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1495, 681);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(pictureBox1);
            Controls.Add(btnSave);
            Controls.Add(btnCancelAcquire);
            Controls.Add(btnOpenDevice);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(comboBox_NumObjCapture);
            Controls.Add(btnCloseDevice);
            Controls.Add(radioButtonInsufficientQuality);
            Controls.Add(DeviceStatus);
            Controls.Add(radioButtonInsufficientObjectCount);
            Controls.Add(checkBoxAutocontrast);
            Controls.Add(checkBoxAltTrigger);
            Controls.Add(checkBoxFlexRollCapture);
            Controls.Add(checkBoxFlexFlatCapture);
            Controls.Add(btnAcquire);
            Controls.Add(LEDlightPanel);
            Controls.Add(checkBox1000dpi);
            Controls.Add(checkBoxPAD);
            Controls.Add(comboBox_Impression);
            Controls.Add(comboBox_Position);
            Controls.Add(ImageBox);
            Controls.Add(checkBoxVisualization);
            Controls.Add(DeviceInfoBox);
            Controls.Add(textBoxLog);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Form1";
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)ImageBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox textBoxLog;
        private System.Windows.Forms.ListBox DeviceInfoBox;
        private System.Windows.Forms.CheckBox checkBoxVisualization;
        private System.Windows.Forms.PictureBox ImageBox;
        private System.Windows.Forms.ComboBox comboBox_Position;
        private System.Windows.Forms.ComboBox comboBox_Impression;
        private System.Windows.Forms.CheckBox checkBoxPAD;
        private System.Windows.Forms.CheckBox checkBox1000dpi;
        private LightPanel LEDlightPanel;
        private System.Windows.Forms.Button btnAcquire;
        private System.Windows.Forms.CheckBox checkBoxFlexFlatCapture;
        private System.Windows.Forms.CheckBox checkBoxFlexRollCapture;
        private System.Windows.Forms.CheckBox checkBoxAltTrigger;
        private System.Windows.Forms.CheckBox checkBoxAutocontrast;
        private System.Windows.Forms.RadioButton radioButtonInsufficientObjectCount;
        private System.Windows.Forms.Label DeviceStatus;
        private System.Windows.Forms.RadioButton radioButtonInsufficientQuality;
        private System.Windows.Forms.Button btnCloseDevice;
        private System.Windows.Forms.ComboBox comboBox_NumObjCapture;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnOpenDevice;
        private System.Windows.Forms.Button btnCancelAcquire;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
    }
}
