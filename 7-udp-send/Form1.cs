using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace _7_udp_send
{
    public partial class Form1 : Form
    {
        private readonly Random _random = new();
        private readonly System.Windows.Forms.Timer _sendTimer = new();
        private UdpClient? _udpClient;
        private IPEndPoint? _serverEndPoint;
        private int _sendCount;
        private double _latitude = 5257.0122231;
        private double _longitude = 111.0435278;

        public Form1()
        {
            InitializeComponent();

            textBoxServerIp.Text = GetDefaultLocalIpAddress();
            textBoxPort.Text = "15000";
            comboDataType.SelectedIndex = 0;
            textBoxSendData.Text = string.Join(Environment.NewLine, CreatePreviewLines());

            buttonSend.Click += ButtonSend_Click;
            comboDataType.SelectedIndexChanged += ComboDataType_SelectedIndexChanged;
            FormClosing += Form1_FormClosing;

            _sendTimer.Interval = 1000;
            _sendTimer.Tick += SendTimer_Tick;
            UpdateStatus();
        }

        private void ButtonSend_Click(object? sender, EventArgs e)
        {
            if (_sendTimer.Enabled)
            {
                StopSending("数据发送已停止。");
                return;
            }

            if (!IPAddress.TryParse(textBoxServerIp.Text.Trim(), out var serverIp))
            {
                MessageBox.Show("请输入正确的服务器IP地址。", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(textBoxPort.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var port)
                || port is < IPEndPoint.MinPort or > IPEndPoint.MaxPort)
            {
                MessageBox.Show("请输入 0 到 65535 之间的端口号。", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _serverEndPoint = new IPEndPoint(serverIp, port);
            _udpClient = new UdpClient();
            _sendTimer.Start();
            buttonSend.Text = "停止发送";
            textBoxServerIp.Enabled = false;
            textBoxPort.Enabled = false;
            comboDataType.Enabled = false;
            AppendLine($"开始向 {_serverEndPoint} 每秒发送1条{comboDataType.Text}数据。");
            SendOnePacket();
            UpdateStatus();
        }

        private void ComboDataType_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (!_sendTimer.Enabled)
            {
                textBoxSendData.Text = string.Join(Environment.NewLine, CreatePreviewLines());
            }
        }

        private void SendTimer_Tick(object? sender, EventArgs e)
        {
            SendOnePacket();
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            StopSending();
            _sendTimer.Dispose();
        }

        private void SendOnePacket()
        {
            if (_udpClient is null || _serverEndPoint is null)
            {
                return;
            }

            try
            {
                var line = comboDataType.Text == "测距仪" ? CreateRangeLine() : CreateGpsLine();
                var buffer = Encoding.UTF8.GetBytes(line);
                _udpClient.Send(buffer, buffer.Length, _serverEndPoint);
                _sendCount++;
                AppendLine(line);
                UpdateStatus();
            }
            catch (Exception ex)
            {
                StopSending($"发送失败: {ex.Message}");
            }
        }

        private void StopSending(string? message = null)
        {
            _sendTimer.Stop();
            _udpClient?.Dispose();
            _udpClient = null;
            _serverEndPoint = null;

            if (!IsDisposed)
            {
                buttonSend.Text = "数据发送";
                textBoxServerIp.Enabled = true;
                textBoxPort.Enabled = true;
                comboDataType.Enabled = true;

                if (!string.IsNullOrEmpty(message))
                {
                    AppendLine(message);
                }

                UpdateStatus();
            }
        }

        private string CreateGpsLine()
        {
            _latitude += (_random.NextDouble() - 0.5) * 0.00008;
            _longitude += (_random.NextDouble() - 0.5) * 0.00008;
            var seconds = 35.0 + _sendCount + _random.NextDouble();
            var altitude = 130.0 + _random.NextDouble() * 20.0;
            var geoid = 7.0 + _random.NextDouble();

            return string.Format(
                CultureInfo.InvariantCulture,
                "$GPGGA,0050{0:00.000},{1:0000.0000000},N,{2:00000.0000000},E,4,08,1.2,{3:F3},M,{4:F3},M,1.0,0029*4F",
                seconds,
                _latitude,
                _longitude,
                altitude,
                geoid);
        }

        private string CreateRangeLine()
        {
            var distance = 12.0 + _random.NextDouble() * 88.0;
            var angle = _random.NextDouble() * 360.0;

            return string.Format(
                CultureInfo.InvariantCulture,
                "RANGE,NO={0:D5},TIME={1:HHmmss.fff},DIST={2:F2}m,ANGLE={3:F1}deg,QUALITY={4}",
                _sendCount + 1,
                DateTime.Now,
                distance,
                angle,
                _random.Next(80, 100));
        }

        private IEnumerable<string> CreatePreviewLines()
        {
            return comboDataType.Text == "测距仪"
                ? Enumerable.Range(1, 4).Select(_ => CreateRangeLine())
                : Enumerable.Range(1, 4).Select(_ => CreateGpsLine());
        }

        private void AppendLine(string line)
        {
            textBoxSendData.AppendText((textBoxSendData.TextLength == 0 ? string.Empty : Environment.NewLine) + line);
            textBoxSendData.SelectionStart = textBoxSendData.TextLength;
            textBoxSendData.ScrollToCaret();
        }

        private void UpdateStatus()
        {
            var state = _sendTimer.Enabled && _serverEndPoint is not null
                ? $"发送中 -> {_serverEndPoint}"
                : "未发送";
            toolStripStatusLabel.Text = $"{state} | 已发送: {_sendCount} 条 | 周期: 1000 毫秒";
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
