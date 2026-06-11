namespace _6_serial_send
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
            groupBoxSettings = new GroupBox();
            labelStopBits = new Label();
            comboStopBits = new ComboBox();
            labelDataBits = new Label();
            comboDataBits = new ComboBox();
            labelParity = new Label();
            comboParity = new ComboBox();
            labelBaudRate = new Label();
            comboBaudRate = new ComboBox();
            buttonOpenClose = new Button();
            buttonRefreshPorts = new Button();
            labelPort = new Label();
            comboPort = new ComboBox();
            groupBoxSend = new GroupBox();
            buttonExit = new Button();
            buttonClear = new Button();
            buttonStartStop = new Button();
            buttonSendOnce = new Button();
            numericInterval = new NumericUpDown();
            labelInterval = new Label();
            textBoxLog = new TextBox();
            statusStrip = new StatusStrip();
            toolStripStatusLabel = new ToolStripStatusLabel();
            groupBoxSettings.SuspendLayout();
            groupBoxSend.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericInterval).BeginInit();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // groupBoxSettings
            // 
            groupBoxSettings.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBoxSettings.Controls.Add(labelStopBits);
            groupBoxSettings.Controls.Add(comboStopBits);
            groupBoxSettings.Controls.Add(labelDataBits);
            groupBoxSettings.Controls.Add(comboDataBits);
            groupBoxSettings.Controls.Add(labelParity);
            groupBoxSettings.Controls.Add(comboParity);
            groupBoxSettings.Controls.Add(labelBaudRate);
            groupBoxSettings.Controls.Add(comboBaudRate);
            groupBoxSettings.Controls.Add(buttonOpenClose);
            groupBoxSettings.Controls.Add(buttonRefreshPorts);
            groupBoxSettings.Controls.Add(labelPort);
            groupBoxSettings.Controls.Add(comboPort);
            groupBoxSettings.Location = new Point(12, 12);
            groupBoxSettings.Name = "groupBoxSettings";
            groupBoxSettings.Size = new Size(900, 124);
            groupBoxSettings.TabIndex = 0;
            groupBoxSettings.TabStop = false;
            groupBoxSettings.Text = "串口参数";
            // 
            // labelStopBits
            // 
            labelStopBits.AutoSize = true;
            labelStopBits.Location = new Point(213, 80);
            labelStopBits.Name = "labelStopBits";
            labelStopBits.Size = new Size(53, 20);
            labelStopBits.TabIndex = 11;
            labelStopBits.Text = "停止位";
            // 
            // comboStopBits
            // 
            comboStopBits.DropDownStyle = ComboBoxStyle.DropDownList;
            comboStopBits.FormattingEnabled = true;
            comboStopBits.Items.AddRange(new object[] { "One", "Two", "OnePointFive" });
            comboStopBits.Location = new Point(272, 76);
            comboStopBits.Name = "comboStopBits";
            comboStopBits.Size = new Size(126, 28);
            comboStopBits.TabIndex = 10;
            // 
            // labelDataBits
            // 
            labelDataBits.AutoSize = true;
            labelDataBits.Location = new Point(20, 80);
            labelDataBits.Name = "labelDataBits";
            labelDataBits.Size = new Size(53, 20);
            labelDataBits.TabIndex = 9;
            labelDataBits.Text = "数据位";
            // 
            // comboDataBits
            // 
            comboDataBits.DropDownStyle = ComboBoxStyle.DropDownList;
            comboDataBits.FormattingEnabled = true;
            comboDataBits.Items.AddRange(new object[] { "8", "7" });
            comboDataBits.Location = new Point(79, 76);
            comboDataBits.Name = "comboDataBits";
            comboDataBits.Size = new Size(109, 28);
            comboDataBits.TabIndex = 8;
            // 
            // labelParity
            // 
            labelParity.AutoSize = true;
            labelParity.Location = new Point(595, 35);
            labelParity.Name = "labelParity";
            labelParity.Size = new Size(53, 20);
            labelParity.TabIndex = 7;
            labelParity.Text = "校验位";
            // 
            // comboParity
            // 
            comboParity.DropDownStyle = ComboBoxStyle.DropDownList;
            comboParity.FormattingEnabled = true;
            comboParity.Items.AddRange(new object[] { "None", "Odd", "Even", "Mark", "Space" });
            comboParity.Location = new Point(654, 31);
            comboParity.Name = "comboParity";
            comboParity.Size = new Size(126, 28);
            comboParity.TabIndex = 6;
            // 
            // labelBaudRate
            // 
            labelBaudRate.AutoSize = true;
            labelBaudRate.Location = new Point(404, 35);
            labelBaudRate.Name = "labelBaudRate";
            labelBaudRate.Size = new Size(53, 20);
            labelBaudRate.TabIndex = 5;
            labelBaudRate.Text = "波特率";
            // 
            // comboBaudRate
            // 
            comboBaudRate.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBaudRate.FormattingEnabled = true;
            comboBaudRate.Items.AddRange(new object[] { "9600", "19200", "38400", "57600", "115200" });
            comboBaudRate.Location = new Point(463, 31);
            comboBaudRate.Name = "comboBaudRate";
            comboBaudRate.Size = new Size(109, 28);
            comboBaudRate.TabIndex = 4;
            // 
            // buttonOpenClose
            // 
            buttonOpenClose.Location = new Point(463, 74);
            buttonOpenClose.Name = "buttonOpenClose";
            buttonOpenClose.Size = new Size(117, 32);
            buttonOpenClose.TabIndex = 3;
            buttonOpenClose.Text = "打开串口";
            buttonOpenClose.UseVisualStyleBackColor = true;
            // 
            // buttonRefreshPorts
            // 
            buttonRefreshPorts.Location = new Point(300, 29);
            buttonRefreshPorts.Name = "buttonRefreshPorts";
            buttonRefreshPorts.Size = new Size(82, 32);
            buttonRefreshPorts.TabIndex = 2;
            buttonRefreshPorts.Text = "刷新";
            buttonRefreshPorts.UseVisualStyleBackColor = true;
            // 
            // labelPort
            // 
            labelPort.AutoSize = true;
            labelPort.Location = new Point(20, 35);
            labelPort.Name = "labelPort";
            labelPort.Size = new Size(53, 20);
            labelPort.TabIndex = 1;
            labelPort.Text = "串口号";
            // 
            // comboPort
            // 
            comboPort.DropDownStyle = ComboBoxStyle.DropDownList;
            comboPort.FormattingEnabled = true;
            comboPort.Location = new Point(79, 31);
            comboPort.Name = "comboPort";
            comboPort.Size = new Size(200, 28);
            comboPort.TabIndex = 0;
            // 
            // groupBoxSend
            // 
            groupBoxSend.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBoxSend.Controls.Add(buttonExit);
            groupBoxSend.Controls.Add(buttonClear);
            groupBoxSend.Controls.Add(buttonStartStop);
            groupBoxSend.Controls.Add(buttonSendOnce);
            groupBoxSend.Controls.Add(numericInterval);
            groupBoxSend.Controls.Add(labelInterval);
            groupBoxSend.Location = new Point(12, 142);
            groupBoxSend.Name = "groupBoxSend";
            groupBoxSend.Size = new Size(900, 91);
            groupBoxSend.TabIndex = 1;
            groupBoxSend.TabStop = false;
            groupBoxSend.Text = "数据发送";
            // 
            // buttonExit
            // 
            buttonExit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonExit.Location = new Point(785, 34);
            buttonExit.Name = "buttonExit";
            buttonExit.Size = new Size(95, 32);
            buttonExit.TabIndex = 5;
            buttonExit.Text = "退出";
            buttonExit.UseVisualStyleBackColor = true;
            // 
            // buttonClear
            // 
            buttonClear.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonClear.Location = new Point(674, 34);
            buttonClear.Name = "buttonClear";
            buttonClear.Size = new Size(95, 32);
            buttonClear.TabIndex = 4;
            buttonClear.Text = "清空";
            buttonClear.UseVisualStyleBackColor = true;
            // 
            // buttonStartStop
            // 
            buttonStartStop.Location = new Point(449, 34);
            buttonStartStop.Name = "buttonStartStop";
            buttonStartStop.Size = new Size(132, 32);
            buttonStartStop.TabIndex = 3;
            buttonStartStop.Text = "开始连续发送";
            buttonStartStop.UseVisualStyleBackColor = true;
            // 
            // buttonSendOnce
            // 
            buttonSendOnce.Location = new Point(316, 34);
            buttonSendOnce.Name = "buttonSendOnce";
            buttonSendOnce.Size = new Size(117, 32);
            buttonSendOnce.TabIndex = 2;
            buttonSendOnce.Text = "发送一次";
            buttonSendOnce.UseVisualStyleBackColor = true;
            // 
            // numericInterval
            // 
            numericInterval.Increment = new decimal(new int[] { 100, 0, 0, 0 });
            numericInterval.Location = new Point(144, 36);
            numericInterval.Maximum = new decimal(new int[] { 60000, 0, 0, 0 });
            numericInterval.Minimum = new decimal(new int[] { 100, 0, 0, 0 });
            numericInterval.Name = "numericInterval";
            numericInterval.Size = new Size(135, 27);
            numericInterval.TabIndex = 1;
            numericInterval.Value = new decimal(new int[] { 1000, 0, 0, 0 });
            // 
            // labelInterval
            // 
            labelInterval.AutoSize = true;
            labelInterval.Location = new Point(20, 39);
            labelInterval.Name = "labelInterval";
            labelInterval.Size = new Size(113, 20);
            labelInterval.TabIndex = 0;
            labelInterval.Text = "发送间隔(毫秒)";
            // 
            // textBoxLog
            // 
            textBoxLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textBoxLog.Font = new Font("Consolas", 10F);
            textBoxLog.Location = new Point(12, 249);
            textBoxLog.Multiline = true;
            textBoxLog.Name = "textBoxLog";
            textBoxLog.ReadOnly = true;
            textBoxLog.ScrollBars = ScrollBars.Both;
            textBoxLog.Size = new Size(900, 343);
            textBoxLog.TabIndex = 2;
            textBoxLog.WordWrap = false;
            // 
            // statusStrip
            // 
            statusStrip.ImageScalingSize = new Size(20, 20);
            statusStrip.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel });
            statusStrip.Location = new Point(0, 605);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(924, 26);
            statusStrip.TabIndex = 3;
            statusStrip.Text = "statusStrip1";
            // 
            // toolStripStatusLabel
            // 
            toolStripStatusLabel.Name = "toolStripStatusLabel";
            toolStripStatusLabel.Size = new Size(144, 20);
            toolStripStatusLabel.Text = "未打开串口，等待操作";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(924, 631);
            Controls.Add(statusStrip);
            Controls.Add(textBoxLog);
            Controls.Add(groupBoxSend);
            Controls.Add(groupBoxSettings);
            MinimumSize = new Size(760, 560);
            Name = "Form1";
            Text = "虚拟串口数据发送程序";
            groupBoxSettings.ResumeLayout(false);
            groupBoxSettings.PerformLayout();
            groupBoxSend.ResumeLayout(false);
            groupBoxSend.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericInterval).EndInit();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private GroupBox groupBoxSettings;
        private Label labelStopBits;
        private ComboBox comboStopBits;
        private Label labelDataBits;
        private ComboBox comboDataBits;
        private Label labelParity;
        private ComboBox comboParity;
        private Label labelBaudRate;
        private ComboBox comboBaudRate;
        private Button buttonOpenClose;
        private Button buttonRefreshPorts;
        private Label labelPort;
        private ComboBox comboPort;
        private GroupBox groupBoxSend;
        private Button buttonExit;
        private Button buttonClear;
        private Button buttonStartStop;
        private Button buttonSendOnce;
        private NumericUpDown numericInterval;
        private Label labelInterval;
        private TextBox textBoxLog;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel toolStripStatusLabel;
    }
}
