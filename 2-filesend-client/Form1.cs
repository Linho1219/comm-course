using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace _2_filesend_client
{
    public partial class FormSendClient : Form
    {
        private const int PbmSetState = 0x0410;
        private const int PbstNormal = 0x0001;
        private const int PbstPaused = 0x0003;

        private readonly object _queueLock = new();
        private readonly LinkedList<ClientTaskItem> _pendingTasks = [];
        private readonly List<ClientTaskItem> _allTasks = [];

        private Task? _processingTask;
        private CancellationTokenSource? _currentTransferCts;
        private ClientTaskItem? _currentTask;

        private bool _isProcessing;
        private bool _isPaused;
        private bool _pauseRequested;
        private bool _cancelAllRequested;
        private bool _clearHistoryRequested;

        private string _targetIp = "127.0.0.1";
        private int _targetPort = 7000;

        public FormSendClient()
        {
            InitializeComponent();
            InitializeGrid();
            WireEvents();
            SetProgressBarPausedVisual(false);
            UpdateUiState();
        }

        private void InitializeGrid()
        {
            dataGridViewTasks.AllowUserToAddRows = false;
            dataGridViewTasks.AllowUserToDeleteRows = false;
            dataGridViewTasks.AllowUserToResizeRows = false;
            dataGridViewTasks.RowHeadersVisible = false;
            dataGridViewTasks.MultiSelect = false;
            dataGridViewTasks.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewTasks.ReadOnly = true;
            dataGridViewTasks.AutoGenerateColumns = false;

            dataGridViewTasks.Columns.Clear();
            dataGridViewTasks.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colFileName",
                HeaderText = "文件名",
                DataPropertyName = "FileName",
                Width = 260
            });
            dataGridViewTasks.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colSourcePath",
                HeaderText = "源路径",
                DataPropertyName = "SourcePath",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridViewTasks.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colStatus",
                HeaderText = "传输状态",
                DataPropertyName = "Status",
                Width = 180
            });
        }

        private void WireEvents()
        {
            buttonBrowseFile.Click += ButtonBrowseFile_Click;
            buttonBrowseFolder.Click += ButtonBrowseFolder_Click;
            buttonStart.Click += ButtonStart_Click;
            buttonPause.Click += ButtonPause_Click;
            buttonCancel.Click += ButtonCancel_Click;
            buttonClearHistory.Click += ButtonClearHistory_Click;
            textBoxPath.TextChanged += (_, _) => UpdateUiState();
            FormClosing += FormSendClient_FormClosing;
        }

        private void FormSendClient_FormClosing(object? sender, FormClosingEventArgs e)
        {
            _cancelAllRequested = true;
            _pauseRequested = false;
            _isPaused = false;
            _currentTransferCts?.Cancel();
        }

        private void ButtonBrowseFile_Click(object? sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                textBoxPath.Text = openFileDialog1.FileName;
            }
        }

        private void ButtonBrowseFolder_Click(object? sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog(this) == DialogResult.OK)
            {
                textBoxPath.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void ButtonStart_Click(object? sender, EventArgs e)
        {
            var inputPath = textBoxPath.Text.Trim();
            textBoxPath.Clear();

            if (!string.IsNullOrWhiteSpace(inputPath))
            {
                EnqueuePath(inputPath);
            }

            if (_isPaused && GetPendingTaskCount() > 0)
            {
                _isPaused = false;
                _pauseRequested = false;
                SetProgressBarPausedVisual(false);
                AppendLog("继续传输...");
            }

            StartProcessingIfNeeded();
            UpdateUiState();
        }

        private void ButtonPause_Click(object? sender, EventArgs e)
        {
            if (!_isProcessing)
            {
                return;
            }

            _pauseRequested = true;
            _isPaused = true;
            _currentTransferCts?.Cancel();
            SetProgressBarPausedVisual(true);
            AppendLog("已请求暂停当前传输。");
            UpdateUiState();
        }

        private void ButtonCancel_Click(object? sender, EventArgs e)
        {
            _cancelAllRequested = true;
            _pauseRequested = false;
            _isPaused = false;
            SetProgressBarPausedVisual(false);
            _currentTransferCts?.Cancel();

            if (!_isProcessing)
            {
                MarkPendingTasksFailedAndClear();
                UpdateProgress(0, 1);
            }

            AppendLog("已请求取消当前和后续任务。");
            UpdateUiState();
        }

        private void ButtonClearHistory_Click(object? sender, EventArgs e)
        {
            _clearHistoryRequested = true;

            if (_isProcessing)
            {
                _cancelAllRequested = true;
                _pauseRequested = false;
                _isPaused = false;
                _currentTransferCts?.Cancel();
                AppendLog("正在取消并清空历史任务...");
            }
            else
            {
                _isPaused = false;
                _pauseRequested = false;
                _cancelAllRequested = false;
                SetProgressBarPausedVisual(false);
                ClearAllTasksNow();
                _clearHistoryRequested = false;
                AppendLog("历史任务已清空。");
            }

            UpdateUiState();
        }

        private void EnqueuePath(string inputPath)
        {
            var fullPath = Path.GetFullPath(inputPath);
            var newTasks = new List<ClientTaskItem>();

            if (File.Exists(fullPath))
            {
                newTasks.Add(new ClientTaskItem(Path.GetFileName(fullPath), fullPath, Path.GetFileName(fullPath)));
            }
            else if (Directory.Exists(fullPath))
            {
                var rootFolderName = new DirectoryInfo(fullPath).Name;
                var files = Directory.EnumerateFiles(fullPath, "*", SearchOption.AllDirectories)
                    .OrderBy(static p => p, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                foreach (var file in files)
                {
                    var relativeWithinSelected = Path.GetRelativePath(fullPath, file);
                    var relativePath = string.IsNullOrWhiteSpace(rootFolderName)
                        ? relativeWithinSelected
                        : Path.Combine(rootFolderName, relativeWithinSelected);
                    newTasks.Add(new ClientTaskItem(Path.GetFileName(file), file, relativePath));
                }
            }
            else
            {
                AppendLog($"路径不存在: {fullPath}");
                return;
            }

            if (newTasks.Count == 0)
            {
                AppendLog("目录下没有可发送的文件。");
                return;
            }

            foreach (var task in newTasks)
            {
                AddTaskToQueue(task);
            }

            AppendLog($"已加入 {newTasks.Count} 个任务。");
        }

        private void AddTaskToQueue(ClientTaskItem task)
        {
            UiSync(() =>
            {
                var rowIndex = dataGridViewTasks.Rows.Add(task.FileName, task.SourcePath, ClientTaskStatus.Queued.ToDisplayText());
                task.Row = dataGridViewTasks.Rows[rowIndex];
            });

            lock (_queueLock)
            {
                task.Status = ClientTaskStatus.Queued;
                _allTasks.Add(task);
                _pendingTasks.AddLast(task);
            }
        }

        private void StartProcessingIfNeeded()
        {
            if (_isProcessing || GetPendingTaskCount() == 0)
            {
                return;
            }

            _targetIp = textBoxIP.Text.Trim();
            _targetPort = (int)numericPort.Value;
            if (string.IsNullOrWhiteSpace(_targetIp))
            {
                AppendLog("目标 IP 不能为空。");
                return;
            }

            _cancelAllRequested = false;
            _pauseRequested = false;
            _isPaused = false;
            _isProcessing = true;

            _processingTask = Task.Run(ProcessQueueAsync);
        }

        private async Task ProcessQueueAsync()
        {
            try
            {
                while (true)
                {
                    if (_cancelAllRequested)
                    {
                        break;
                    }

                    if (_pauseRequested)
                    {
                        break;
                    }

                    var task = DequeueNextTask();
                    if (task is null)
                    {
                        break;
                    }

                    _currentTask = task;
                    SetTaskStatus(task, ClientTaskStatus.Transferring);
                    var keepProgressOnPause = false;

                    _currentTransferCts?.Dispose();
                    _currentTransferCts = new CancellationTokenSource();

                    try
                    {
                        await SendFileAsync(task, _currentTransferCts.Token);
                        SetTaskStatus(task, ClientTaskStatus.Success);
                        AppendLog($"发送成功: {task.RelativePath}");
                    }
                    catch (OperationCanceledException)
                    {
                        if (_cancelAllRequested)
                        {
                            SetTaskStatus(task, ClientTaskStatus.Failed);
                            MarkPendingTasksFailedAndClear();
                            AppendLog($"已取消: {task.RelativePath}");
                            break;
                        }

                        if (_pauseRequested)
                        {
                            SetTaskStatus(task, ClientTaskStatus.Queued);
                            RequeueToFront(task);
                            keepProgressOnPause = true;
                            AppendLog($"已暂停: {task.RelativePath}");
                            break;
                        }

                        SetTaskStatus(task, ClientTaskStatus.Failed);
                        AppendLog($"任务取消: {task.RelativePath}");
                    }
                    catch (Exception ex)
                    {
                        SetTaskStatus(task, ClientTaskStatus.Failed);
                        AppendLog($"发送失败: {task.RelativePath}，{ex.Message}");
                    }
                    finally
                    {
                        _currentTask = null;
                        _currentTransferCts?.Dispose();
                        _currentTransferCts = null;
                        if (!keepProgressOnPause)
                        {
                            UpdateProgress(0, 1);
                        }
                    }
                }
            }
            finally
            {
                _isProcessing = false;
                _pauseRequested = _pauseRequested && GetPendingTaskCount() > 0;
                _isPaused = _pauseRequested;
                SetProgressBarPausedVisual(_isPaused);

                if (_clearHistoryRequested)
                {
                    ClearAllTasksNow();
                    _clearHistoryRequested = false;
                    AppendLog("历史任务已清空。");
                }

                if (_cancelAllRequested)
                {
                    _cancelAllRequested = false;
                }

                Ui(UpdateUiState);
            }
        }

        private async Task SendFileAsync(ClientTaskItem task, CancellationToken token)
        {
            if (!File.Exists(task.SourcePath))
            {
                throw new FileNotFoundException("源文件不存在。", task.SourcePath);
            }

            var fileInfo = new FileInfo(task.SourcePath);
            var fileSize = fileInfo.Length;
            task.ResumeKey ??= BuildResumeKey(task.SourcePath, fileSize);

            using var client = new TcpClient();
            await client.ConnectAsync(_targetIp, _targetPort, token);

            using var stream = client.GetStream();

            var startRequest = new TransferStartRequest
            {
                FileName = task.FileName,
                RelativePath = task.RelativePath,
                SourcePath = Path.GetFullPath(task.SourcePath),
                FileSize = fileSize,
                ResumeKey = task.ResumeKey
            };

            await TransferProtocol.WriteJsonAsync(stream, startRequest, token);
            var startResponse = await TransferProtocol.ReadJsonAsync<TransferStartResponse>(stream, token);

            if (!startResponse.Accepted)
            {
                throw new InvalidOperationException(startResponse.Message);
            }

            var offset = Math.Clamp(startResponse.ResumeOffset, 0, fileSize);
            UpdateProgress(offset, fileSize <= 0 ? 1 : fileSize);

            if (offset < fileSize)
            {
                var buffer = new byte[64 * 1024];
                long sent = offset;

                using var fileStream = new FileStream(task.SourcePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                fileStream.Seek(offset, SeekOrigin.Begin);

                while (sent < fileSize)
                {
                    var toRead = (int)Math.Min(buffer.Length, fileSize - sent);
                    var read = await fileStream.ReadAsync(buffer.AsMemory(0, toRead), token);
                    if (read <= 0)
                    {
                        throw new IOException("读取源文件失败。");
                    }

                    await stream.WriteAsync(buffer.AsMemory(0, read), token);
                    sent += read;
                    UpdateProgress(sent, fileSize);
                }
            }

            var complete = await TransferProtocol.ReadJsonAsync<TransferCompleteResponse>(stream, token);
            if (!complete.Success)
            {
                throw new InvalidOperationException(complete.Message);
            }
        }

        private ClientTaskItem? DequeueNextTask()
        {
            lock (_queueLock)
            {
                if (_pendingTasks.Count == 0)
                {
                    return null;
                }

                var first = _pendingTasks.First!.Value;
                _pendingTasks.RemoveFirst();
                return first;
            }
        }

        private void RequeueToFront(ClientTaskItem task)
        {
            lock (_queueLock)
            {
                _pendingTasks.AddFirst(task);
            }
        }

        private int GetPendingTaskCount()
        {
            lock (_queueLock)
            {
                return _pendingTasks.Count;
            }
        }

        private void MarkPendingTasksFailedAndClear()
        {
            List<ClientTaskItem> pending;
            lock (_queueLock)
            {
                pending = [.. _pendingTasks];
                _pendingTasks.Clear();
            }

            foreach (var task in pending)
            {
                SetTaskStatus(task, ClientTaskStatus.Failed);
            }
        }

        private void ClearAllTasksNow()
        {
            lock (_queueLock)
            {
                _pendingTasks.Clear();
                _allTasks.Clear();
            }

            Ui(() =>
            {
                dataGridViewTasks.Rows.Clear();
                UpdateProgress(0, 1);
            });
        }

        private void SetTaskStatus(ClientTaskItem task, ClientTaskStatus status)
        {
            task.Status = status;
            Ui(() =>
            {
                if (task.Row?.DataGridView is not null)
                {
                    task.Row.Cells[2].Value = status.ToDisplayText();
                }
            });
        }

        private void UpdateProgress(long value, long max)
        {
            Ui(() =>
            {
                const int scaleMax = 10000;
                var safeMax = Math.Max(1L, max);
                var safeValue = Math.Max(0L, Math.Min(safeMax, value));

                var scaledValue = safeMax == 0
                    ? 0
                    : (int)Math.Min(scaleMax, (safeValue * scaleMax) / safeMax);

                progressBar.Maximum = scaleMax;
                progressBar.Value = scaledValue;
            });
        }

        private static string BuildResumeKey(string sourcePath, long fileSize)
        {
            var fullPath = Path.GetFullPath(sourcePath);
            return $"{fullPath}|{fileSize}";
        }

        private void AppendLog(string message)
        {
            var line = $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}";
            Ui(() => textBoxLog.AppendText(line));
        }

        private void Ui(Action action)
        {
            if (IsDisposed)
            {
                return;
            }

            if (InvokeRequired)
            {
                BeginInvoke(action);
            }
            else
            {
                action();
            }
        }

        private void UiSync(Action action)
        {
            if (IsDisposed)
            {
                return;
            }

            if (InvokeRequired)
            {
                Invoke(action);
            }
            else
            {
                action();
            }
        }

        private void UpdateUiState()
        {
            var hasPending = GetPendingTaskCount() > 0;
            var hasPath = !string.IsNullOrWhiteSpace(textBoxPath.Text);
            var hasTasks = dataGridViewTasks.Rows.Count > 0 || hasPending || _isProcessing;

            var lockInputs = _isProcessing || _isPaused || hasPending;

            textBoxIP.Enabled = !lockInputs;
            numericPort.Enabled = !lockInputs;
            textBoxPath.Enabled = !lockInputs;
            buttonBrowseFile.Enabled = !lockInputs;
            buttonBrowseFolder.Enabled = !lockInputs;

            buttonStart.Enabled = !_isProcessing && (hasPath || hasPending);
            buttonPause.Enabled = _isProcessing;
            buttonCancel.Enabled = _isProcessing || hasPending || _isPaused;
            buttonClearHistory.Enabled = hasTasks;
        }

        private void SetProgressBarPausedVisual(bool paused)
        {
            Ui(() =>
            {
                if (progressBar.IsHandleCreated)
                {
                    SendMessage(progressBar.Handle, PbmSetState, (nint)(paused ? PbstPaused : PbstNormal), 0);
                }
            });
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern nint SendMessage(nint hWnd, int msg, nint wParam, nint lParam);
    }
}
