namespace FPandSign
{
    partial class LightPanel
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            led1 = new LED();
            led2 = new LED();
            led3 = new LED();
            led4 = new LED();
            led5 = new LED();
            SuspendLayout();
            // 
            // led1
            // 
            led1.LedColor = ActiveColor.gray;
            led1.Location = new Point(3, 2);
            led1.Name = "led1";
            led1.Size = new Size(85, 56);
            led1.TabIndex = 0;
            // 
            // led2
            // 
            led2.LedColor = ActiveColor.gray;
            led2.Location = new Point(94, 3);
            led2.Name = "led2";
            led2.Size = new Size(85, 56);
            led2.TabIndex = 1;
            // 
            // led3
            // 
            led3.LedColor = ActiveColor.gray;
            led3.Location = new Point(185, 3);
            led3.Name = "led3";
            led3.Size = new Size(85, 56);
            led3.TabIndex = 2;
            // 
            // led4
            // 
            led4.LedColor = ActiveColor.gray;
            led4.Location = new Point(276, 3);
            led4.Name = "led4";
            led4.Size = new Size(85, 56);
            led4.TabIndex = 3;
            // 
            // led5
            // 
            led5.LedColor = ActiveColor.gray;
            led5.Location = new Point(367, 3);
            led5.Name = "led5";
            led5.Size = new Size(85, 56);
            led5.TabIndex = 4;
            // 
            // LightPanel
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(led5);
            Controls.Add(led4);
            Controls.Add(led3);
            Controls.Add(led2);
            Controls.Add(led1);
            Name = "LightPanel";
            Size = new Size(463, 62);
            ResumeLayout(false);
        }

        #endregion

        private LED led1;
        private LED led2;
        private LED led3;
        private LED led4;
        private LED led5;
    }
}
