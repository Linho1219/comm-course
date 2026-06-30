using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace _7_udp_receive
{
    public partial class Form1 : Form
    {
        private UdpClient? _udpClient;
        private CancellationTokenSource? _listenCancellation;
        private int _receiveCount;

        public Form1()
        {
            InitializeComponent();

            textBoxLocalIp.Text = GetDefaultLocalIpAddress();
            textBoxPort.Text = "15000";

            buttonListen.Click += ButtonListen_Click;
            FormClosing += Form1_FormClosing;
            AppendReceivedText("等待来自客户端的UDP连接......");
            UpdateStatus();
        }

        private void ButtonListen_Click(object? sender, EventArgs e)
        {
            if (_udpClient is not null)
            {
                StopListening("监听已停止。");
                return;
            }

            if (!IPAddress.TryParse(textBoxLocalIp.Text.Trim(), out var localIp))
            {
                MessageBox.Show("请输入正确的本机IP地址。", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(textBoxPort.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var port)
                || port is < IPEndPoint.MinPort or > IPEndPoint.MaxPort)
            {
                MessageBox.Show("请输入 0 到 65535 之间的端口号。", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _udpClient = new UdpClient(new IPEndPoint(localIp, port));
                _listenCancellation = new CancellationTokenSource();
                buttonListen.Text = "暂停监听";
                textBoxLocalIp.Enabled = false;
                textBoxPort.Enabled = false;
                AppendReceivedText($"开始监听 {localIp}:{port}");
                UpdateStatus();
                _ = ListenAsync(_listenCancellation.Token);
            }
            catch (Exception ex)
            {
                StopListening($"启动监听失败: {ex.Message}");
            }
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            StopListening();
        }

        private async Task ListenAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = await _udpClient!.ReceiveAsync(cancellationToken);
                    var message = Encoding.UTF8.GetString(result.Buffer);
                    BeginInvoke(new Action(() =>
                    {
                        _receiveCount++;
                        AppendReceivedText(message);
                        UpdateStatus();
                    }));
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
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        BeginInvoke(new Action(() => StopListening($"接收失败: {ex.Message}")));
                    }

                    break;
                }
            }
        }

        private void StopListening(string? message = null)
        {
            _listenCancellation?.Cancel();
            _listenCancellation?.Dispose();
            _listenCancellation = null;

            _udpClient?.Dispose();
            _udpClient = null;

            if (!IsDisposed)
            {
                buttonListen.Text = "开始监听";
                textBoxLocalIp.Enabled = true;
                textBoxPort.Enabled = true;

                if (!string.IsNullOrEmpty(message))
                {
                    AppendReceivedText(message);
                }

                UpdateStatus();
            }
        }

        private void AppendReceivedText(string text)
        {
            if (IsDisposed)
            {
                return;
            }

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => AppendReceivedText(text)));
                return;
            }

            textBoxReceive.AppendText((textBoxReceive.TextLength == 0 ? string.Empty : Environment.NewLine) + text);
            textBoxReceive.SelectionStart = textBoxReceive.TextLength;
            textBoxReceive.ScrollToCaret();
        }

        private void UpdateStatus()
        {
            var state = _udpClient is null ? "未监听" : $"监听中 {textBoxLocalIp.Text}:{textBoxPort.Text}";
            toolStripStatusLabel.Text = $"{state} | 已接收: {_receiveCount} 条";
        }

        private static string GetDefaultLocalIpAddress()
        {
            try
            {
                using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socket.Connect("8.8.8.8", 65530);
                return ((IPEndPoint)socket.LocalEndPoint!).Address.ToString();
            }
            catch
            {
                return "127.0.0.1";
            }
        }
    }
}
