namespace _2_filesend_server
{
    partial class FormFileReceiveServer
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
            labelPort = new Label();
            numericPort = new NumericUpDown();
            buttonStartServer = new Button();
            buttonStopServer = new Button();
            labelWorkingDir = new Label();
            textBoxWorkingDir = new TextBox();
            folderBrowserDialog1 = new FolderBrowserDialog();
            buttonBrowseFolder = new Button();
            buttonShowInExpolorer = new Button();
            progressBar = new ProgressBar();
            buttonForceCancel = new Button();
            textBoxLog = new TextBox();
            dataGridViewTasks = new DataGridView();
            buttonClearHistory = new Button();
            ((System.ComponentModel.ISupportInitialize)numericPort).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewTasks).BeginInit();
            SuspendLayout();
            // 
            // labelPort
            // 
            labelPort.AutoSize = true;
            labelPort.Location = new Point(34, 36);
            labelPort.Name = "labelPort";
            labelPort.Size = new Size(110, 32);
            labelPort.TabIndex = 0;
            labelPort.Text = "监听端口";
            // 
            // numericPort
            // 
            numericPort.Location = new Point(171, 33);
            numericPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numericPort.Name = "numericPort";
            numericPort.Size = new Size(678, 39);
            numericPort.TabIndex = 1;
            numericPort.Tag = "";
            numericPort.Value = new decimal(new int[] { 7000, 0, 0, 0 });
            // 
            // buttonStartServer
            // 
            buttonStartServer.Location = new Point(855, 29);
            buttonStartServer.Name = "buttonStartServer";
            buttonStartServer.Size = new Size(150, 46);
            buttonStartServer.TabIndex = 2;
            buttonStartServer.Text = "启动监听";
            buttonStartServer.UseVisualStyleBackColor = true;
            // 
            // buttonStopServer
            // 
            buttonStopServer.Enabled = false;
            buttonStopServer.Location = new Point(1011, 29);
            buttonStopServer.Name = "buttonStopServer";
            buttonStopServer.Size = new Size(150, 46);
            buttonStopServer.TabIndex = 3;
            buttonStopServer.Text = "停止监听";
            buttonStopServer.UseVisualStyleBackColor = true;
            // 
            // labelWorkingDir
            // 
            labelWorkingDir.AutoSize = true;
            labelWorkingDir.Location = new Point(34, 113);
            labelWorkingDir.Name = "labelWorkingDir";
            labelWorkingDir.Size = new Size(110, 32);
            labelWorkingDir.TabIndex = 4;
            labelWorkingDir.Text = "工作目录";
            // 
            // textBoxWorkingDir
            // 
            textBoxWorkingDir.Location = new Point(171, 110);
            textBoxWorkingDir.Name = "textBoxWorkingDir";
            textBoxWorkingDir.Size = new Size(678, 39);
            textBoxWorkingDir.TabIndex = 5;
            textBoxWorkingDir.Text = "D:\\FileReceive";
            // 
            // buttonBrowseFolder
            // 
            buttonBrowseFolder.Location = new Point(855, 106);
            buttonBrowseFolder.Name = "buttonBrowseFolder";
            buttonBrowseFolder.Size = new Size(229, 46);
            buttonBrowseFolder.TabIndex = 6;
            buttonBrowseFolder.Text = "选择目录...";
            buttonBrowseFolder.UseVisualStyleBackColor = true;
            // 
            // buttonShowInExpolorer
            // 
            buttonShowInExpolorer.Font = new Font("Segoe Fluent Icons", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            buttonShowInExpolorer.Location = new Point(1090, 106);
            buttonShowInExpolorer.Name = "buttonShowInExpolorer";
            buttonShowInExpolorer.Size = new Size(71, 46);
            buttonShowInExpolorer.TabIndex = 7;
            buttonShowInExpolorer.Text = "";
            buttonShowInExpolorer.UseVisualStyleBackColor = true;
            // 
            // progressBar
            // 
            progressBar.Location = new Point(34, 183);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(815, 46);
            progressBar.TabIndex = 8;
            // 
            // buttonForceCancel
            // 
            buttonForceCancel.Enabled = false;
            buttonForceCancel.Location = new Point(855, 183);
            buttonForceCancel.Name = "buttonForceCancel";
            buttonForceCancel.Size = new Size(150, 46);
            buttonForceCancel.TabIndex = 9;
            buttonForceCancel.Text = "强制取消";
            buttonForceCancel.UseVisualStyleBackColor = true;
            // 
            // textBoxLog
            // 
            textBoxLog.Location = new Point(34, 688);
            textBoxLog.Multiline = true;
            textBoxLog.Name = "textBoxLog";
            textBoxLog.ReadOnly = true;
            textBoxLog.ScrollBars = ScrollBars.Vertical;
            textBoxLog.Size = new Size(1127, 350);
            textBoxLog.TabIndex = 15;
            // 
            // dataGridViewTasks
            // 
            dataGridViewTasks.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewTasks.Location = new Point(34, 270);
            dataGridViewTasks.Name = "dataGridViewTasks";
            dataGridViewTasks.RowHeadersWidth = 82;
            dataGridViewTasks.Size = new Size(1127, 391);
            dataGridViewTasks.TabIndex = 14;
            // 
            // buttonClearHistory
            // 
            buttonClearHistory.Enabled = false;
            buttonClearHistory.Location = new Point(1011, 183);
            buttonClearHistory.Name = "buttonClearHistory";
            buttonClearHistory.Size = new Size(150, 46);
            buttonClearHistory.TabIndex = 16;
            buttonClearHistory.Text = "清空历史";
            buttonClearHistory.UseVisualStyleBackColor = true;
            // 
            // FormFileReceiveServer
            // 
            AutoScaleDimensions = new SizeF(14F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1201, 1070);
            Controls.Add(buttonClearHistory);
            Controls.Add(textBoxLog);
            Controls.Add(dataGridViewTasks);
            Controls.Add(buttonForceCancel);
            Controls.Add(progressBar);
            Controls.Add(buttonShowInExpolorer);
            Controls.Add(buttonBrowseFolder);
            Controls.Add(textBoxWorkingDir);
            Controls.Add(labelWorkingDir);
            Controls.Add(buttonStopServer);
            Controls.Add(buttonStartServer);
            Controls.Add(numericPort);
            Controls.Add(labelPort);
            Name = "FormFileReceiveServer";
            Text = "文件接收服务器";
            ((System.ComponentModel.ISupportInitialize)numericPort).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewTasks).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelPort;
        private NumericUpDown numericPort;
        private Button buttonStartServer;
        private Button buttonStopServer;
        private Label labelWorkingDir;
        private TextBox textBoxWorkingDir;
        private FolderBrowserDialog folderBrowserDialog1;
        private Button buttonBrowseFolder;
        private Button buttonShowInExpolorer;
        private ProgressBar progressBar;
        private Button buttonForceCancel;
        private TextBox textBoxLog;
        private DataGridView dataGridViewTasks;
        private Button buttonClearHistory;
    }
}
