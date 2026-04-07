namespace _1_convert_case_client
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
            textBoxIP = new TextBox();
            numericPort = new NumericUpDown();
            buttonConnect = new Button();
            buttonTerminate = new Button();
            textBoxMsg = new TextBox();
            buttonSend = new Button();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            textBoxLog = new TextBox();
            ((System.ComponentModel.ISupportInitialize)numericPort).BeginInit();
            SuspendLayout();
            // 
            // textBoxIP
            // 
            textBoxIP.Location = new Point(96, 16);
            textBoxIP.Name = "textBoxIP";
            textBoxIP.Size = new Size(189, 26);
            textBoxIP.TabIndex = 0;
            textBoxIP.Text = "127.0.0.1";
            // 
            // numericPort
            // 
            numericPort.Location = new Point(96, 53);
            numericPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numericPort.Name = "numericPort";
            numericPort.Size = new Size(189, 26);
            numericPort.TabIndex = 1;
            numericPort.Value = new decimal(new int[] { 5000, 0, 0, 0 });
            // 
            // buttonConnect
            // 
            buttonConnect.Location = new Point(336, 15);
            buttonConnect.Name = "buttonConnect";
            buttonConnect.Size = new Size(94, 29);
            buttonConnect.TabIndex = 2;
            buttonConnect.Text = "连接";
            buttonConnect.UseVisualStyleBackColor = true;
            // 
            // buttonTerminate
            // 
            buttonTerminate.Enabled = false;
            buttonTerminate.Location = new Point(336, 52);
            buttonTerminate.Name = "buttonTerminate";
            buttonTerminate.Size = new Size(94, 29);
            buttonTerminate.TabIndex = 3;
            buttonTerminate.Text = "中断";
            buttonTerminate.UseVisualStyleBackColor = true;
            // 
            // textBoxMsg
            // 
            textBoxMsg.Location = new Point(20, 102);
            textBoxMsg.Name = "textBoxMsg";
            textBoxMsg.Size = new Size(298, 26);
            textBoxMsg.TabIndex = 4;
            // 
            // buttonSend
            // 
            buttonSend.Enabled = false;
            buttonSend.Location = new Point(336, 101);
            buttonSend.Name = "buttonSend";
            buttonSend.Size = new Size(94, 29);
            buttonSend.TabIndex = 5;
            buttonSend.Text = "发送";
            buttonSend.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(20, 20);
            label1.Name = "label1";
            label1.Size = new Size(21, 18);
            label1.TabIndex = 6;
            label1.Text = "IP";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(20, 57);
            label2.Name = "label2";
            label2.Size = new Size(38, 18);
            label2.TabIndex = 7;
            label2.Text = "端口";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(20, 150);
            label3.Name = "label3";
            label3.Size = new Size(38, 18);
            label3.TabIndex = 11;
            label3.Text = "日志";
            // 
            // textBoxLog
            // 
            textBoxLog.Location = new Point(20, 178);
            textBoxLog.Multiline = true;
            textBoxLog.Name = "textBoxLog";
            textBoxLog.ReadOnly = true;
            textBoxLog.ScrollBars = ScrollBars.Vertical;
            textBoxLog.Size = new Size(410, 368);
            textBoxLog.TabIndex = 10;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(9F, 18F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(450, 565);
            Controls.Add(label3);
            Controls.Add(textBoxLog);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(buttonSend);
            Controls.Add(textBoxMsg);
            Controls.Add(buttonTerminate);
            Controls.Add(buttonConnect);
            Controls.Add(numericPort);
            Controls.Add(textBoxIP);
            Name = "Form1";
            Text = "大小写转换客户端";
            ((System.ComponentModel.ISupportInitialize)numericPort).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBoxIP;
        private NumericUpDown numericPort;
        private Button buttonConnect;
        private Button buttonTerminate;
        private TextBox textBoxMsg;
        private Button buttonSend;
        private Label label1;
        private Label label2;
        private Label label3;
        private TextBox textBoxLog;
    }
}
