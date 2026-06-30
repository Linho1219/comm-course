namespace _7_udp_send
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
            labelServerIp = new Label();
            textBoxServerIp = new TextBox();
            labelPort = new Label();
            textBoxPort = new TextBox();
            comboDataType = new ComboBox();
            buttonSend = new Button();
            textBoxSendData = new TextBox();
            statusStrip = new StatusStrip();
            toolStripStatusLabel = new ToolStripStatusLabel();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // labelServerIp
            // 
            labelServerIp.AutoSize = true;
            labelServerIp.Location = new Point(18, 25);
            labelServerIp.Name = "labelServerIp";
            labelServerIp.Size = new Size(99, 20);
            labelServerIp.TabIndex = 0;
            labelServerIp.Text = "服务器的IP地址";
            // 
            // textBoxServerIp
            // 
            textBoxServerIp.Location = new Point(124, 22);
            textBoxServerIp.Name = "textBoxServerIp";
            textBoxServerIp.Size = new Size(220, 27);
            textBoxServerIp.TabIndex = 1;
            // 
            // labelPort
            // 
            labelPort.AutoSize = true;
            labelPort.Location = new Point(18, 64);
            labelPort.Name = "labelPort";
            labelPort.Size = new Size(99, 20);
            labelPort.TabIndex = 2;
            labelPort.Text = "数据发送端口";
            // 
            // textBoxPort
            // 
            textBoxPort.Location = new Point(124, 61);
            textBoxPort.Name = "textBoxPort";
            textBoxPort.Size = new Size(105, 27);
            textBoxPort.TabIndex = 3;
            // 
            // comboDataType
            // 
            comboDataType.DropDownStyle = ComboBoxStyle.DropDownList;
            comboDataType.FormattingEnabled = true;
            comboDataType.Items.AddRange(new object[] { "GPS", "测距仪" });
            comboDataType.Location = new Point(244, 61);
            comboDataType.Name = "comboDataType";
            comboDataType.Size = new Size(100, 28);
            comboDataType.TabIndex = 4;
            // 
            // buttonSend
            // 
            buttonSend.Location = new Point(361, 21);
            buttonSend.Name = "buttonSend";
            buttonSend.Size = new Size(125, 68);
            buttonSend.TabIndex = 5;
            buttonSend.Text = "数据发送";
            buttonSend.UseVisualStyleBackColor = true;
            // 
            // textBoxSendData
            // 
            textBoxSendData.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textBoxSendData.Font = new Font("Consolas", 10F);
            textBoxSendData.Location = new Point(18, 104);
            textBoxSendData.Multiline = true;
            textBoxSendData.Name = "textBoxSendData";
            textBoxSendData.ScrollBars = ScrollBars.Vertical;
            textBoxSendData.Size = new Size(468, 426);
            textBoxSendData.TabIndex = 6;
            // 
            // statusStrip
            // 
            statusStrip.ImageScalingSize = new Size(20, 20);
            statusStrip.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel });
            statusStrip.Location = new Point(0, 547);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(504, 26);
            statusStrip.TabIndex = 7;
            statusStrip.Text = "statusStrip1";
            // 
            // toolStripStatusLabel
            // 
            toolStripStatusLabel.Name = "toolStripStatusLabel";
            toolStripStatusLabel.Size = new Size(197, 20);
            toolStripStatusLabel.Text = "未发送，点击按钮开始每秒发送";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(504, 573);
            Controls.Add(statusStrip);
            Controls.Add(textBoxSendData);
            Controls.Add(buttonSend);
            Controls.Add(comboDataType);
            Controls.Add(textBoxPort);
            Controls.Add(labelPort);
            Controls.Add(textBoxServerIp);
            Controls.Add(labelServerIp);
            MinimumSize = new Size(520, 520);
            Name = "Form1";
            Text = "UDP数据发送客户端";
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelServerIp;
        private TextBox textBoxServerIp;
        private Label labelPort;
        private TextBox textBoxPort;
        private ComboBox comboDataType;
        private Button buttonSend;
        private TextBox textBoxSendData;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel toolStripStatusLabel;
    }
}
