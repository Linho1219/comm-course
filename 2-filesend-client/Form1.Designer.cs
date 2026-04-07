namespace _2_filesend_client
{
    partial class FormSendClient
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
            labelIP = new Label();
            textBoxIP = new TextBox();
            labelPort = new Label();
            numericPort = new NumericUpDown();
            labelPath = new Label();
            textBoxPath = new TextBox();
            openFileDialog1 = new OpenFileDialog();
            folderBrowserDialog1 = new FolderBrowserDialog();
            buttonBrowseFile = new Button();
            buttonBrowseFolder = new Button();
            progressBar = new ProgressBar();
            buttonStart = new Button();
            buttonPause = new Button();
            buttonCancel = new Button();
            dataGridViewTasks = new DataGridView();
            textBoxLog = new TextBox();
            buttonClearHistory = new Button();
            ((System.ComponentModel.ISupportInitialize)numericPort).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewTasks).BeginInit();
            SuspendLayout();
            // 
            // labelIP
            // 
            labelIP.AutoSize = true;
            labelIP.Location = new Point(36, 38);
            labelIP.Name = "labelIP";
            labelIP.Size = new Size(88, 32);
            labelIP.TabIndex = 0;
            labelIP.Text = "目标 IP";
            // 
            // textBoxIP
            // 
            textBoxIP.Location = new Point(150, 35);
            textBoxIP.Name = "textBoxIP";
            textBoxIP.Size = new Size(266, 39);
            textBoxIP.TabIndex = 1;
            textBoxIP.Text = "127.0.0.1";
            // 
            // labelPort
            // 
            labelPort.AutoSize = true;
            labelPort.Location = new Point(488, 38);
            labelPort.Name = "labelPort";
            labelPort.Size = new Size(110, 32);
            labelPort.TabIndex = 2;
            labelPort.Text = "目标端口";
            // 
            // numericPort
            // 
            numericPort.Location = new Point(630, 35);
            numericPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numericPort.Name = "numericPort";
            numericPort.Size = new Size(225, 39);
            numericPort.TabIndex = 3;
            numericPort.Value = new decimal(new int[] { 7000, 0, 0, 0 });
            // 
            // labelPath
            // 
            labelPath.AutoSize = true;
            labelPath.Location = new Point(36, 109);
            labelPath.Name = "labelPath";
            labelPath.Size = new Size(182, 32);
            labelPath.TabIndex = 4;
            labelPath.Text = "文件或目录路径";
            // 
            // textBoxPath
            // 
            textBoxPath.Location = new Point(241, 106);
            textBoxPath.Name = "textBoxPath";
            textBoxPath.Size = new Size(774, 39);
            textBoxPath.TabIndex = 5;
            // 
            // openFileDialog1
            // 
            openFileDialog1.FileName = "openFileDialog1";
            // 
            // buttonBrowseFile
            // 
            buttonBrowseFile.AccessibleDescription = "";
            buttonBrowseFile.AccessibleName = "";
            buttonBrowseFile.Font = new Font("Segoe Fluent Icons", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            buttonBrowseFile.Location = new Point(1023, 106);
            buttonBrowseFile.Name = "buttonBrowseFile";
            buttonBrowseFile.Size = new Size(70, 46);
            buttonBrowseFile.TabIndex = 6;
            buttonBrowseFile.Text = "";
            buttonBrowseFile.UseVisualStyleBackColor = true;
            // 
            // buttonBrowseFolder
            // 
            buttonBrowseFolder.Font = new Font("Segoe Fluent Icons", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            buttonBrowseFolder.Location = new Point(1101, 106);
            buttonBrowseFolder.Name = "buttonBrowseFolder";
            buttonBrowseFolder.Size = new Size(70, 46);
            buttonBrowseFolder.TabIndex = 7;
            buttonBrowseFolder.Text = "";
            buttonBrowseFolder.UseVisualStyleBackColor = true;
            // 
            // progressBar
            // 
            progressBar.Location = new Point(36, 171);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(819, 46);
            progressBar.TabIndex = 8;
            // 
            // buttonStart
            // 
            buttonStart.Font = new Font("Segoe Fluent Icons", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            buttonStart.Location = new Point(867, 171);
            buttonStart.Name = "buttonStart";
            buttonStart.Size = new Size(70, 46);
            buttonStart.TabIndex = 9;
            buttonStart.Text = "";
            buttonStart.UseVisualStyleBackColor = true;
            // 
            // buttonPause
            // 
            buttonPause.Enabled = false;
            buttonPause.Font = new Font("Segoe Fluent Icons", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            buttonPause.Location = new Point(945, 171);
            buttonPause.Name = "buttonPause";
            buttonPause.Size = new Size(70, 46);
            buttonPause.TabIndex = 10;
            buttonPause.Text = "";
            buttonPause.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            buttonCancel.Enabled = false;
            buttonCancel.Font = new Font("Segoe Fluent Icons", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            buttonCancel.Location = new Point(1023, 171);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(70, 46);
            buttonCancel.TabIndex = 11;
            buttonCancel.Text = "";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // dataGridViewTasks
            // 
            dataGridViewTasks.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewTasks.Location = new Point(36, 252);
            dataGridViewTasks.Name = "dataGridViewTasks";
            dataGridViewTasks.RowHeadersWidth = 82;
            dataGridViewTasks.Size = new Size(1133, 420);
            dataGridViewTasks.TabIndex = 12;
            // 
            // textBoxLog
            // 
            textBoxLog.Location = new Point(36, 699);
            textBoxLog.Multiline = true;
            textBoxLog.Name = "textBoxLog";
            textBoxLog.ReadOnly = true;
            textBoxLog.ScrollBars = ScrollBars.Vertical;
            textBoxLog.Size = new Size(1133, 350);
            textBoxLog.TabIndex = 13;
            // 
            // buttonClearHistory
            // 
            buttonClearHistory.Font = new Font("Segoe Fluent Icons", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            buttonClearHistory.Location = new Point(1099, 171);
            buttonClearHistory.Name = "buttonClearHistory";
            buttonClearHistory.Size = new Size(70, 46);
            buttonClearHistory.TabIndex = 14;
            buttonClearHistory.Text = "";
            buttonClearHistory.UseVisualStyleBackColor = true;
            // 
            // FormSendClient
            // 
            AllowDrop = true;
            AutoScaleDimensions = new SizeF(14F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1207, 1084);
            Controls.Add(buttonClearHistory);
            Controls.Add(textBoxLog);
            Controls.Add(dataGridViewTasks);
            Controls.Add(buttonCancel);
            Controls.Add(buttonPause);
            Controls.Add(buttonStart);
            Controls.Add(progressBar);
            Controls.Add(buttonBrowseFolder);
            Controls.Add(buttonBrowseFile);
            Controls.Add(textBoxPath);
            Controls.Add(labelPath);
            Controls.Add(numericPort);
            Controls.Add(labelPort);
            Controls.Add(textBoxIP);
            Controls.Add(labelIP);
            Name = "FormSendClient";
            Text = "文件发送客户端";
            ((System.ComponentModel.ISupportInitialize)numericPort).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridViewTasks).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelIP;
        private TextBox textBoxIP;
        private Label labelPort;
        private NumericUpDown numericPort;
        private Label labelPath;
        private TextBox textBoxPath;
        private OpenFileDialog openFileDialog1;
        private FolderBrowserDialog folderBrowserDialog1;
        private Button buttonBrowseFile;
        private Button buttonBrowseFolder;
        private ProgressBar progressBar;
        private Button buttonStart;
        private Button buttonPause;
        private Button buttonCancel;
        private DataGridView dataGridViewTasks;
        private TextBox textBoxLog;
        private Button buttonClearHistory;
    }
}
