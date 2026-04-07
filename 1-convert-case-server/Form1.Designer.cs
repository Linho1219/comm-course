namespace _1_convert_case_server
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
            label1 = new Label();
            numericToUpperPort = new NumericUpDown();
            buttonStartToUpper = new Button();
            buttonStopToUpper = new Button();
            buttonStopToLower = new Button();
            buttonStartToLower = new Button();
            numericToLowerPort = new NumericUpDown();
            label2 = new Label();
            textBoxLog = new TextBox();
            label3 = new Label();
            ((System.ComponentModel.ISupportInitialize)numericToUpperPort).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericToLowerPort).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(24, 25);
            label1.Name = "label1";
            label1.Size = new Size(113, 18);
            label1.TabIndex = 0;
            label1.Text = "监听转大写端口";
            // 
            // numericToUpperPort
            // 
            numericToUpperPort.Location = new Point(157, 21);
            numericToUpperPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numericToUpperPort.Name = "numericToUpperPort";
            numericToUpperPort.Size = new Size(150, 26);
            numericToUpperPort.TabIndex = 1;
            numericToUpperPort.Value = new decimal(new int[] { 5000, 0, 0, 0 });
            // 
            // buttonStartToUpper
            // 
            buttonStartToUpper.Location = new Point(327, 20);
            buttonStartToUpper.Name = "buttonStartToUpper";
            buttonStartToUpper.Size = new Size(94, 29);
            buttonStartToUpper.TabIndex = 2;
            buttonStartToUpper.Text = "启动";
            buttonStartToUpper.UseVisualStyleBackColor = true;
            // 
            // buttonStopToUpper
            // 
            buttonStopToUpper.Enabled = false;
            buttonStopToUpper.Location = new Point(427, 20);
            buttonStopToUpper.Name = "buttonStopToUpper";
            buttonStopToUpper.Size = new Size(94, 29);
            buttonStopToUpper.TabIndex = 3;
            buttonStopToUpper.Text = "停止";
            buttonStopToUpper.UseVisualStyleBackColor = true;
            // 
            // buttonStopToLower
            // 
            buttonStopToLower.Enabled = false;
            buttonStopToLower.Location = new Point(427, 61);
            buttonStopToLower.Name = "buttonStopToLower";
            buttonStopToLower.Size = new Size(94, 29);
            buttonStopToLower.TabIndex = 7;
            buttonStopToLower.Text = "停止";
            buttonStopToLower.UseVisualStyleBackColor = true;
            // 
            // buttonStartToLower
            // 
            buttonStartToLower.Location = new Point(327, 61);
            buttonStartToLower.Name = "buttonStartToLower";
            buttonStartToLower.Size = new Size(94, 29);
            buttonStartToLower.TabIndex = 6;
            buttonStartToLower.Text = "启动";
            buttonStartToLower.UseVisualStyleBackColor = true;
            // 
            // numericToLowerPort
            // 
            numericToLowerPort.Location = new Point(157, 62);
            numericToLowerPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numericToLowerPort.Name = "numericToLowerPort";
            numericToLowerPort.Size = new Size(150, 26);
            numericToLowerPort.TabIndex = 5;
            numericToLowerPort.Value = new decimal(new int[] { 6000, 0, 0, 0 });
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(24, 66);
            label2.Name = "label2";
            label2.Size = new Size(113, 18);
            label2.TabIndex = 4;
            label2.Text = "监听转小写端口";
            // 
            // textBoxLog
            // 
            textBoxLog.Location = new Point(24, 135);
            textBoxLog.Multiline = true;
            textBoxLog.Name = "textBoxLog";
            textBoxLog.ReadOnly = true;
            textBoxLog.ScrollBars = ScrollBars.Vertical;
            textBoxLog.Size = new Size(497, 368);
            textBoxLog.TabIndex = 8;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(24, 107);
            label3.Name = "label3";
            label3.Size = new Size(38, 18);
            label3.TabIndex = 9;
            label3.Text = "日志";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(9F, 18F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(547, 527);
            Controls.Add(label3);
            Controls.Add(textBoxLog);
            Controls.Add(buttonStopToLower);
            Controls.Add(buttonStartToLower);
            Controls.Add(numericToLowerPort);
            Controls.Add(label2);
            Controls.Add(buttonStopToUpper);
            Controls.Add(buttonStartToUpper);
            Controls.Add(numericToUpperPort);
            Controls.Add(label1);
            Name = "Form1";
            Text = "大小写转换服务端";
            ((System.ComponentModel.ISupportInitialize)numericToUpperPort).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericToLowerPort).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private NumericUpDown numericToUpperPort;
        private Button buttonStartToUpper;
        private Button buttonStopToUpper;
        private Button buttonStopToLower;
        private Button buttonStartToLower;
        private NumericUpDown numericToLowerPort;
        private Label label2;
        private TextBox textBoxLog;
        private Label label3;
    }
}
