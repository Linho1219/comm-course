using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace _2_filesend_server
{
    public partial class FormFileReceiveServer : Form
    {
        private readonly object _resumeLock = new();
        private readonly object _taskLock = new();
        private readonly Dictionary<string, ResumeRecord> _resumeRecords = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, ServerTaskItem> _taskByKey = new(StringComparer.OrdinalIgnoreCase);
        private readonly SemaphoreSlim _singleReceiveLock = new(1, 1);

        private TcpListener? _listener;
        private CancellationTokenSource? _listenerCts;
        private Task? _acceptLoopTask;

        private CancellationTokenSource? _currentReceiveCts;
        private ServerTaskItem? _currentTask;

        private string? _workingDirectory;

        public FormFileReceiveServer()
        {
            InitializeComponent();
            InitializeGrid();
            WireEvents();
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
                Width = 260
            });
            dataGridViewTasks.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colTargetRelPath",
                HeaderText = "目标相对路径",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridViewTasks.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colStatus",
                HeaderText = "传输状态",
                Width = 180
            });
        }

        private void WireEvents()
        {
            buttonStartServer.Click += ButtonStartServer_Click;
            buttonStopServer.Click += ButtonStopServer_Click;
            buttonBrowseFolder.Click += ButtonBrowseFolder_Click;
            buttonShowInExpolorer.Click += ButtonShowInExpolorer_Click;
            buttonForceCancel.Click += ButtonForceCancel_Click;
            buttonClearHistory.Click += ButtonClearHistory_Click;
            FormClosing += FormFileReceiveServer_FormClosing;
        }

        private void FormFileReceiveServer_FormClosing(object? sender, FormClosingEventArgs e)
        {
            StopServer();
        }

        private void ButtonBrowseFolder_Click(object? sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog(this) == DialogResult.OK)
            {
                textBoxWorkingDir.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void ButtonShowInExpolorer_Click(object? sender, EventArgs e)
        {
            try
            {
                var dir = _workingDirectory ?? textBoxWorkingDir.Text.Trim();
                if (string.IsNullOrWhiteSpace(dir))
                {
                    return;
                }

                Directory.CreateDirectory(dir);
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"\"{dir}\"",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                AppendLog($"打开目录失败: {ex.Message}");
            }
        }

        private void ButtonStartServer_Click(object? sender, EventArgs e)
        {
            if (_listener is not null)
            {
                return;
            }

            var dir = textBoxWorkingDir.Text.Trim();
            if (string.IsNullOrWhiteSpace(dir))
            {
                AppendLog("工作目录不能为空。");
                return;
            }

            try
            {
                var fullDir = Path.GetFullPath(dir);
                Directory.CreateDirectory(fullDir);
                _workingDirectory = fullDir;
            }
            catch (Exception ex)
            {
                AppendLog($"创建工作目录失败: {ex.Message}");
                return;
            }

            var port = (int)numericPort.Value;
            try
            {
                _listener = new TcpListener(IPAddress.Any, port);
                _listener.Start();
                _listenerCts = new CancellationTokenSource();
                _acceptLoopTask = Task.Run(() => AcceptLoopAsync(_listener, _listenerCts.Token));
                AppendLog($"监听已启动，端口 {port}，工作目录 {_workingDirectory}");
            }
            catch (Exception ex)
            {
                AppendLog($"启动监听失败: {ex.Message}");
                _listener = null;
                _listenerCts?.Dispose();
                _listenerCts = null;
                _acceptLoopTask = null;
            }

            UpdateUiState();
        }

        private void ButtonStopServer_Click(object? sender, EventArgs e)
        {
            StopServer();
        }

        private void ButtonForceCancel_Click(object? sender, EventArgs e)
        {
            _currentReceiveCts?.Cancel();
            AppendLog("已请求取消当前接收文件。");
        }

        private void ButtonClearHistory_Click(object? sender, EventArgs e)
        {
            _currentReceiveCts?.Cancel();

            lock (_taskLock)
            {
                _taskByKey.Clear();
            }
            Ui(() =>
            {
                dataGridViewTasks.Rows.Clear();
                UpdateProgress(0, 1);
            });

            AppendLog("历史任务已清空。");
            UpdateUiState();
        }

        private void StopServer()
        {
            if (_listener is null)
            {
                return;
            }

            try
            {
                _listenerCts?.Cancel();
                _currentReceiveCts?.Cancel();
                _listener.Stop();
            }
            catch
            {
            }

            _listener = null;
            _listenerCts?.Dispose();
            _listenerCts = null;
            _acceptLoopTask = null;

            _currentReceiveCts?.Dispose();
            _currentReceiveCts = null;
            _currentTask = null;

            lock (_resumeLock)
            {
                _resumeRecords.Clear();
            }

            UpdateProgress(0, 1);
            AppendLog("监听已停止。");
            UpdateUiState();
        }

        private async Task AcceptLoopAsync(TcpListener listener, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                TcpClient? client = null;
                try
                {
                    client = await listener.AcceptTcpClientAsync(token);
                    _ = Task.Run(() => HandleClientAsync(client, token), token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    if (!token.IsCancellationRequested)
                    {
                        AppendLog($"接收连接失败: {ex.Message}");
                    }

                    client?.Dispose();
                }
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken listenerToken)
        {
            await _singleReceiveLock.WaitAsync(listenerToken);
            try
            {
                using (client)
                {
                    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(listenerToken);
                    _currentReceiveCts = linkedCts;
                    await ProcessClientAsync(client, linkedCts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                if (!listenerToken.IsCancellationRequested)
                {
                    AppendLog("当前文件接收已取消。");
                }
            }
            catch (Exception ex)
            {
                AppendLog($"处理客户端失败: {ex.Message}");
            }
            finally
            {
                _currentReceiveCts?.Dispose();
                _currentReceiveCts = null;
                _currentTask = null;
                UpdateProgress(0, 1);
                UpdateUiState();
                _singleReceiveLock.Release();
            }
        }

        private async Task ProcessClientAsync(TcpClient client, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(_workingDirectory))
            {
                throw new InvalidOperationException("工作目录未配置。");
            }

            using var stream = client.GetStream();
            var request = await TransferProtocol.ReadJsonAsync<TransferStartRequest>(stream, token);
            var resumeKey = BuildResumeKey(request.SourcePath, request.FileSize);

            if (!TryResolveTargetPath(_workingDirectory, request.RelativePath, out var relativePath, out var targetPath, out var pathError))
            {
                await TransferProtocol.WriteJsonAsync(stream, new TransferStartResponse
                {
                    Accepted = false,
                    ResumeOffset = 0,
                    Message = pathError
                }, token);
                return;
            }

            var task = GetOrCreateTask(resumeKey, request.FileName, relativePath);
            _currentTask = task;
            SetTaskStatus(task, ServerTaskStatus.Transferring);
            UpdateUiState();

            var tmpDir = Path.Combine(_workingDirectory, ".tmp");
            Directory.CreateDirectory(tmpDir);
            var tmpPath = Path.Combine(tmpDir, $"{GetResumeTempFileName(resumeKey)}.tmp");

            ResumeRecord record;
            long resumeOffset;

            lock (_resumeLock)
            {
                _resumeRecords.TryGetValue(resumeKey, out var existingRecord);
                var canResume = existingRecord is not null
                                && existingRecord.SourcePath.Equals(request.SourcePath, StringComparison.OrdinalIgnoreCase)
                                && existingRecord.FileSize == request.FileSize
                                && File.Exists(existingRecord.TempPath);

                if (canResume)
                {
                    AppendLog("发现可续传记录，尝试续传。");
                    record = existingRecord!;
                    record.RelativePath = relativePath;
                    resumeOffset = Math.Clamp(new FileInfo(record.TempPath).Length, 0, request.FileSize);
                    tmpPath = record.TempPath;
                }
                else
                {
                    if (File.Exists(tmpPath))
                    {
                        File.Delete(tmpPath);
                    }

                    record = new ResumeRecord
                    {
                        ResumeKey = resumeKey,
                        SourcePath = request.SourcePath,
                        RelativePath = relativePath,
                        FileSize = request.FileSize,
                        TempPath = tmpPath
                    };
                    _resumeRecords[resumeKey] = record;
                    resumeOffset = 0;
                }
            }

            await TransferProtocol.WriteJsonAsync(stream, new TransferStartResponse
            {
                Accepted = true,
                ResumeOffset = resumeOffset,
                Message = resumeOffset > 0 ? $"从 {resumeOffset} 字节继续。" : "开始传输。"
            }, token);

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);

                var fileMode = resumeOffset > 0 ? FileMode.OpenOrCreate : FileMode.Create;
                {
                    await using var tempStream = new FileStream(tmpPath, fileMode, FileAccess.ReadWrite, FileShare.None);
                    if (resumeOffset > 0)
                    {
                        tempStream.Seek(resumeOffset, SeekOrigin.Begin);
                    }
                    else
                    {
                        tempStream.SetLength(0);
                    }

                    long received = resumeOffset;
                    UpdateProgress(received, request.FileSize <= 0 ? 1 : request.FileSize);

                    var buffer = new byte[64 * 1024];
                    while (received < request.FileSize)
                    {
                        var toRead = (int)Math.Min(buffer.Length, request.FileSize - received);
                        var read = await stream.ReadAsync(buffer.AsMemory(0, toRead), token);
                        if (read <= 0)
                        {
                            throw new IOException("客户端连接中断。");
                        }

                        await tempStream.WriteAsync(buffer.AsMemory(0, read), token);
                        received += read;
                        UpdateProgress(received, request.FileSize);
                    }

                    await tempStream.FlushAsync(token);
                }

                if (File.Exists(targetPath))
                {
                    File.Delete(targetPath);
                }

                File.Move(tmpPath, targetPath);

                lock (_resumeLock)
                {
                    _resumeRecords.Remove(resumeKey);
                }

                SetTaskStatus(task, ServerTaskStatus.Success);

                await TransferProtocol.WriteJsonAsync(stream, new TransferCompleteResponse
                {
                    Success = true,
                    Message = "传输完成。"
                }, token);

                AppendLog($"接收成功: {relativePath}");
            }
            catch (OperationCanceledException)
            {
                SetTaskStatus(task, ServerTaskStatus.Failed);
                AppendLog($"接收已取消: {relativePath}");

                if (!token.IsCancellationRequested)
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                SetTaskStatus(task, ServerTaskStatus.Failed);
                AppendLog($"接收失败: {relativePath}，{ex.Message}");

                try
                {
                    await TransferProtocol.WriteJsonAsync(stream, new TransferCompleteResponse
                    {
                        Success = false,
                        Message = ex.Message
                    }, CancellationToken.None);
                }
                catch
                {
                }
            }
        }

        private ServerTaskItem GetOrCreateTask(string resumeKey, string fileName, string relativePath)
        {
            lock (_taskLock)
            {
                if (_taskByKey.TryGetValue(resumeKey, out var existing))
                {
                    existing.FileName = fileName;
                    existing.RelativePath = relativePath;
                    Ui(() =>
                    {
                        if (existing.Row?.DataGridView is not null)
                        {
                            existing.Row.Cells[0].Value = existing.FileName;
                            existing.Row.Cells[1].Value = existing.RelativePath;
                        }
                    });
                    return existing;
                }

                var item = new ServerTaskItem
                {
                    ResumeKey = resumeKey,
                    FileName = fileName,
                    RelativePath = relativePath
                };

                Ui(() =>
                {
                    var rowIndex = dataGridViewTasks.Rows.Add(item.FileName, item.RelativePath, ServerTaskStatus.Transferring.ToDisplayText());
                    item.Row = dataGridViewTasks.Rows[rowIndex];
                });

                _taskByKey[resumeKey] = item;
                return item;
            }
        }

        private void SetTaskStatus(ServerTaskItem task, ServerTaskStatus status)
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

        private static bool TryResolveTargetPath(
            string workingDirectory,
            string inputRelativePath,
            out string normalizedRelativePath,
            out string targetPath,
            out string error)
        {
            normalizedRelativePath = string.Empty;
            targetPath = string.Empty;
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(inputRelativePath))
            {
                error = "目标相对路径不能为空。";
                return false;
            }

            normalizedRelativePath = inputRelativePath.Replace('/', Path.DirectorySeparatorChar)
                .TrimStart(Path.DirectorySeparatorChar);

            if (string.IsNullOrWhiteSpace(normalizedRelativePath))
            {
                error = "目标路径非法。";
                return false;
            }

            var fullPath = Path.GetFullPath(Path.Combine(workingDirectory, normalizedRelativePath));
            var fullWorkdir = Path.GetFullPath(workingDirectory)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!fullPath.StartsWith(fullWorkdir, StringComparison.OrdinalIgnoreCase))
            {
                error = "目标路径越界。";
                return false;
            }

            targetPath = fullPath;
            return true;
        }

        private static string BuildResumeKey(string sourcePath, long fileSize)
        {
            return $"{sourcePath}|{fileSize}";
        }

        private static string GetResumeTempFileName(string resumeKey)
        {
            var bytes = Encoding.UTF8.GetBytes(resumeKey);
            var hash = SHA256.HashData(bytes);
            return Convert.ToHexString(hash);
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

        private void UpdateUiState()
        {
            if (InvokeRequired)
            {
                BeginInvoke(UpdateUiState);
                return;
            }

            var running = _listener is not null;
            var hasTasks = dataGridViewTasks.Rows.Count > 0;
            var receiving = _currentReceiveCts is not null;

            buttonStartServer.Enabled = !running;
            buttonStopServer.Enabled = running;

            numericPort.Enabled = !running;
            textBoxWorkingDir.Enabled = !running;
            buttonBrowseFolder.Enabled = !running;

            buttonForceCancel.Enabled = running && receiving;
            buttonClearHistory.Enabled = hasTasks || receiving;
            buttonShowInExpolorer.Enabled = !string.IsNullOrWhiteSpace(_workingDirectory ?? textBoxWorkingDir.Text.Trim());
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
    }
}
