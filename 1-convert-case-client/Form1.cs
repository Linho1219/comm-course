using System.Net;
using System.Net.Sockets;
using System.Text;

namespace _1_convert_case_client
{
    public partial class Form1 : Form
    {
        private readonly byte[] _receiveBuffer = new byte[4096];
        private Socket? _clientSocket;
        private bool _isConnected;

        public Form1()
        {
            InitializeComponent();

            buttonConnect.Click += ButtonConnect_Click;
            buttonTerminate.Click += ButtonTerminate_Click;
            buttonSend.Click += ButtonSend_Click;
            FormClosing += Form1_FormClosing;
        }

        private async void ButtonConnect_Click(object? sender, EventArgs e)
        {
            if (_isConnected)
            {
                return;
            }

            try
            {
                if (!IPAddress.TryParse(textBoxIP.Text.Trim(), out var ip))
                {
                    AppendLog("IP 地址格式不正确。");
                    return;
                }

                var endPoint = new IPEndPoint(ip, (int)numericPort.Value);
                var socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                await ConnectAsync(socket, endPoint);

                _clientSocket = socket;
                _isConnected = true;
                UpdateClientUiState();
                AppendLog($"已连接到 {endPoint}。");
            }
            catch (Exception ex)
            {
                AppendLog($"连接失败: {ex.Message}");
                SafeCloseClientSocket();
            }
        }

        private void ButtonTerminate_Click(object? sender, EventArgs e)
        {
            TerminateClient("连接已中断。");
        }

        private async void ButtonSend_Click(object? sender, EventArgs e)
        {
            if (!_isConnected || _clientSocket is null)
            {
                return;
            }

            var text = textBoxMsg.Text;
            if (string.IsNullOrEmpty(text))
            {
                AppendLog("发送内容为空，已忽略。");
                return;
            }

            try
            {
                var sendBytes = Encoding.ASCII.GetBytes(text);
                await SendAsync(_clientSocket, sendBytes);
                AppendLog($"发送: {text}");

                var receivedCount = await ReceiveAsync(_clientSocket, _receiveBuffer);
                if (receivedCount == 0)
                {
                    TerminateClient("服务器已关闭连接。");
                    return;
                }

                var response = Encoding.ASCII.GetString(_receiveBuffer, 0, receivedCount);
                AppendLog($"接收: {response}");
            }
            catch (Exception ex)
            {
                TerminateClient($"通信异常: {ex.Message}");
            }
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            SafeCloseClientSocket();
        }

        private void TerminateClient(string reason)
        {
            SafeCloseClientSocket();
            AppendLog(reason);
        }

        private void SafeCloseClientSocket()
        {
            var socket = _clientSocket;
            _clientSocket = null;

            if (socket is not null)
            {
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
                catch
                {
                }

                try
                {
                    socket.Close();
                }
                catch
                {
                }
            }

            _isConnected = false;
            UpdateClientUiState();
        }

        private void UpdateClientUiState()
        {
            buttonConnect.Enabled = !_isConnected;
            buttonTerminate.Enabled = _isConnected;
            buttonSend.Enabled = _isConnected;
            textBoxIP.Enabled = !_isConnected;
            numericPort.Enabled = !_isConnected;
        }

        private void AppendLog(string message)
        {
            var line = $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}";

            if (InvokeRequired)
            {
                BeginInvoke(() => textBoxLog.AppendText(line));
                return;
            }

            textBoxLog.AppendText(line);
        }

        private static Task ConnectAsync(Socket socket, EndPoint endPoint)
        {
            var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var args = new SocketAsyncEventArgs { RemoteEndPoint = endPoint };

            void Completed(object? _, SocketAsyncEventArgs eventArgs)
            {
                eventArgs.Completed -= Completed;
                if (eventArgs.SocketError == SocketError.Success)
                {
                    tcs.TrySetResult();
                }
                else
                {
                    tcs.TrySetException(new SocketException((int)eventArgs.SocketError));
                }

                eventArgs.Dispose();
            }

            args.Completed += Completed;

            if (!socket.ConnectAsync(args))
            {
                Completed(socket, args);
            }

            return tcs.Task;
        }

        private static Task<int> SendAsync(Socket socket, byte[] data)
        {
            var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            var args = new SocketAsyncEventArgs();
            args.SetBuffer(data, 0, data.Length);

            void Completed(object? _, SocketAsyncEventArgs eventArgs)
            {
                eventArgs.Completed -= Completed;
                if (eventArgs.SocketError == SocketError.Success)
                {
                    tcs.TrySetResult(eventArgs.BytesTransferred);
                }
                else
                {
                    tcs.TrySetException(new SocketException((int)eventArgs.SocketError));
                }

                eventArgs.Dispose();
            }

            args.Completed += Completed;

            if (!socket.SendAsync(args))
            {
                Completed(socket, args);
            }

            return tcs.Task;
        }

        private static Task<int> ReceiveAsync(Socket socket, byte[] buffer)
        {
            var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            var args = new SocketAsyncEventArgs();
            args.SetBuffer(buffer, 0, buffer.Length);

            void Completed(object? _, SocketAsyncEventArgs eventArgs)
            {
                eventArgs.Completed -= Completed;
                if (eventArgs.SocketError == SocketError.Success)
                {
                    tcs.TrySetResult(eventArgs.BytesTransferred);
                }
                else
                {
                    tcs.TrySetException(new SocketException((int)eventArgs.SocketError));
                }

                eventArgs.Dispose();
            }

            args.Completed += Completed;

            if (!socket.ReceiveAsync(args))
            {
                Completed(socket, args);
            }

            return tcs.Task;
        }
    }
}
