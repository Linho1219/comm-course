namespace _7_udp_receive
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
            labelLocalIp = new Label();
            textBoxLocalIp = new TextBox();
            labelPort = new Label();
            textBoxPort = new TextBox();
            buttonListen = new Button();
            textBoxReceive = new TextBox();
            statusStrip = new StatusStrip();
            toolStripStatusLabel = new ToolStripStatusLabel();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // labelLocalIp
            // 
            labelLocalIp.AutoSize = true;
            labelLocalIp.Location = new Point(18, 25);
            labelLocalIp.Name = "labelLocalIp";
            labelLocalIp.Size = new Size(97, 20);
            labelLocalIp.TabIndex = 0;
            labelLocalIp.Text = "本机的IP地址";
            // 
            // textBoxLocalIp
            // 
            textBoxLocalIp.Location = new Point(124, 22);
            textBoxLocalIp.Name = "textBoxLocalIp";
            textBoxLocalIp.Size = new Size(245, 27);
            textBoxLocalIp.TabIndex = 1;
            // 
            // labelPort
            // 
            labelPort.AutoSize = true;
            labelPort.Location = new Point(385, 25);
            labelPort.Name = "labelPort";
            labelPort.Size = new Size(69, 20);
            labelPort.TabIndex = 2;
            labelPort.Text = "监听端口";
            // 
            // textBoxPort
            // 
            textBoxPort.Location = new Point(462, 22);
            textBoxPort.Name = "textBoxPort";
            textBoxPort.Size = new Size(125, 27);
            textBoxPort.TabIndex = 3;
            // 
            // buttonListen
            // 
            buttonListen.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonListen.Location = new Point(662, 14);
            buttonListen.Name = "buttonListen";
            buttonListen.Size = new Size(130, 42);
            buttonListen.TabIndex = 4;
            buttonListen.Text = "开始监听";
            buttonListen.UseVisualStyleBackColor = true;
            // 
            // textBoxReceive
            // 
            textBoxReceive.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textBoxReceive.Font = new Font("Consolas", 10F);
            textBoxReceive.Location = new Point(18, 84);
            textBoxReceive.Multiline = true;
            textBoxReceive.Name = "textBoxReceive";
            textBoxReceive.ReadOnly = true;
            textBoxReceive.ScrollBars = ScrollBars.Both;
            textBoxReceive.Size = new Size(774, 381);
            textBoxReceive.TabIndex = 5;
            textBoxReceive.WordWrap = false;
            // 
            // statusStrip
            // 
            statusStrip.ImageScalingSize = new Size(20, 20);
            statusStrip.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel });
            statusStrip.Location = new Point(0, 482);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(810, 26);
            statusStrip.TabIndex = 6;
            statusStrip.Text = "statusStrip1";
            // 
            // toolStripStatusLabel
            // 
            toolStripStatusLabel.Name = "toolStripStatusLabel";
            toolStripStatusLabel.Size = new Size(144, 20);
            toolStripStatusLabel.Text = "未监听，等待客户端";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(810, 508);
            Controls.Add(statusStrip);
            Controls.Add(textBoxReceive);
            Controls.Add(buttonListen);
            Controls.Add(textBoxPort);
            Controls.Add(labelPort);
            Controls.Add(textBoxLocalIp);
            Controls.Add(labelLocalIp);
            MinimumSize = new Size(720, 420);
            Name = "Form1";
            Text = "UDP数据监听服务器";
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelLocalIp;
        private TextBox textBoxLocalIp;
        private Label labelPort;
        private TextBox textBoxPort;
        private Button buttonListen;
        private TextBox textBoxReceive;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel toolStripStatusLabel;
    }
}
