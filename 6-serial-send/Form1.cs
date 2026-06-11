using System.Globalization;
using System.IO.Ports;
using System.Text;

namespace _6_serial_send
{
    public partial class Form1 : Form
    {
        private readonly Random _random = new();
        private readonly System.Windows.Forms.Timer _sendTimer = new();
        private SerialPort? _serialPort;
        private int _sendCount;

        public Form1()
        {
            InitializeComponent();

            comboBaudRate.SelectedItem = "9600";
            comboParity.SelectedItem = "None";
            comboDataBits.SelectedItem = "8";
            comboStopBits.SelectedItem = "One";

            buttonRefreshPorts.Click += ButtonRefreshPorts_Click;
            buttonOpenClose.Click += ButtonOpenClose_Click;
            buttonSendOnce.Click += ButtonSendOnce_Click;
            buttonStartStop.Click += ButtonStartStop_Click;
            buttonClear.Click += ButtonClear_Click;
            buttonExit.Click += ButtonExit_Click;
            numericInterval.ValueChanged += NumericInterval_ValueChanged;
            FormClosing += Form1_FormClosing;

            _sendTimer.Tick += SendTimer_Tick;

            LoadPortNames();
            UpdateUiState();
            AppendLog("请选择与接收端或串口助手配对的端口。");
        }

        private void ButtonRefreshPorts_Click(object? sender, EventArgs e)
        {
            LoadPortNames();
        }

        private void ButtonOpenClose_Click(object? sender, EventArgs e)
        {
            if (_serialPort?.IsOpen == true)
            {
                CloseSerialPort("串口已关闭。");
                return;
            }

            OpenSerialPort();
        }

        private void ButtonSendOnce_Click(object? sender, EventArgs e)
        {
            if (EnsureSerialPortOpen())
            {
                SendSensorData();
            }
        }

        private void ButtonStartStop_Click(object? sender, EventArgs e)
        {
            if (_sendTimer.Enabled)
            {
                _sendTimer.Stop();
                AppendLog("连续发送已停止。");
                UpdateUiState();
                return;
            }

            if (!EnsureSerialPortOpen())
            {
                return;
            }

            _sendTimer.Interval = (int)numericInterval.Value;
            _sendTimer.Start();
            AppendLog($"连续发送已启动，间隔 {_sendTimer.Interval} 毫秒。");
            UpdateUiState();
            SendSensorData();
        }

        private void ButtonClear_Click(object? sender, EventArgs e)
        {
            textBoxLog.Clear();
            _sendCount = 0;
            UpdateStatus();
        }

        private void ButtonExit_Click(object? sender, EventArgs e)
        {
            Close();
        }

        private void NumericInterval_ValueChanged(object? sender, EventArgs e)
        {
            if (_sendTimer.Enabled)
            {
                _sendTimer.Interval = (int)numericInterval.Value;
                UpdateStatus();
            }
        }

        private void SendTimer_Tick(object? sender, EventArgs e)
        {
            SendSensorData();
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            CloseSerialPort();
            _sendTimer.Dispose();
        }

        private void LoadPortNames()
        {
            var previousPort = comboPort.Text;
            comboPort.Items.Clear();

            var ports = SerialPort.GetPortNames()
                .OrderBy(GetPortNumber)
                .ThenBy(port => port, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            comboPort.Items.AddRange(ports);

            if (ports.Length == 0)
            {
                toolStripStatusLabel.Text = "未发现串口，请检查虚拟串口软件是否已创建 COM5/COM6。";
                return;
            }

            if (ports.Contains(previousPort, StringComparer.OrdinalIgnoreCase))
            {
                comboPort.SelectedItem = previousPort;
            }
            else if (ports.Contains("COM5", StringComparer.OrdinalIgnoreCase))
            {
                comboPort.SelectedItem = "COM5";
            }
            else
            {
                comboPort.SelectedIndex = 0;
            }

            UpdateStatus();
        }

        private bool EnsureSerialPortOpen()
        {
            if (_serialPort?.IsOpen == true)
            {
                return true;
            }

            OpenSerialPort();
            return _serialPort?.IsOpen == true;
        }

        private void OpenSerialPort()
        {
            if (string.IsNullOrWhiteSpace(comboPort.Text))
            {
                AppendLog("没有可用串口，请先刷新串口列表。");
                return;
            }

            try
            {
                _serialPort = CreateConfiguredSerialPort();
                _serialPort.Open();

                AppendLog($"已打开 {_serialPort.PortName}: {_serialPort.BaudRate}, {_serialPort.Parity}, {_serialPort.DataBits}, {_serialPort.StopBits}。");
                UpdateUiState();
            }
            catch (Exception ex)
            {
                AppendLog($"打开串口失败: {ex.Message}");
                CloseSerialPort();
            }
        }

        private SerialPort CreateConfiguredSerialPort()
        {
            return new SerialPort(
                comboPort.Text,
                int.Parse(comboBaudRate.Text, CultureInfo.InvariantCulture),
                GetSelectedParity(),
                int.Parse(comboDataBits.Text, CultureInfo.InvariantCulture),
                GetSelectedStopBits())
            {
                Encoding = Encoding.ASCII,
                Handshake = Handshake.None,
                NewLine = "\r\n",
                ReadTimeout = 1000,
                WriteTimeout = 1000
            };
        }

        private void CloseSerialPort(string? message = null)
        {
            _sendTimer.Stop();

            var serialPort = _serialPort;
            _serialPort = null;

            if (serialPort is not null)
            {
                try
                {
                    if (serialPort.IsOpen)
                    {
                        serialPort.Close();
                    }
                }
                catch (Exception ex)
                {
                    AppendLog($"关闭串口时发生异常: {ex.Message}");
                }
                finally
                {
                    serialPort.Dispose();
                }
            }

            if (!string.IsNullOrEmpty(message))
            {
                AppendLog(message);
            }

            UpdateUiState();
        }

        private void SendSensorData()
        {
            var serialPort = _serialPort;
            if (serialPort?.IsOpen != true)
            {
                AppendLog("串口未打开，无法发送。");
                UpdateUiState();
                return;
            }

            try
            {
                var line = CreateSensorLine(_sendCount + 1);
                serialPort.WriteLine(line);
                _sendCount++;
                AppendLog($"发送: {line}");
                UpdateStatus();
            }
            catch (Exception ex)
            {
                AppendLog($"发送失败: {ex.Message}");
                CloseSerialPort("串口已关闭，请检查接收端或虚拟串口连接。");
            }
        }

        private string CreateSensorLine(int index)
        {
            var temperature = 18.0 + _random.NextDouble() * 16.0;
            var humidity = 35.0 + _random.NextDouble() * 45.0;
            var pressure = 99.0 + _random.NextDouble() * 5.0;
            var light = 100 + _random.Next(0, 900);

            return string.Format(
                CultureInfo.InvariantCulture,
                "SENSOR,NO={0:D5},TIME={1:yyyy-MM-dd HH:mm:ss.fff},TEMP={2:F1}C,HUM={3:F1}%,PRESS={4:F2}kPa,LIGHT={5}lx",
                index,
                DateTime.Now,
                temperature,
                humidity,
                pressure,
                light);
        }

        private void UpdateUiState()
        {
            var isOpen = _serialPort?.IsOpen == true;

            comboPort.Enabled = !isOpen;
            comboBaudRate.Enabled = !isOpen;
            comboParity.Enabled = !isOpen;
            comboDataBits.Enabled = !isOpen;
            comboStopBits.Enabled = !isOpen;
            buttonRefreshPorts.Enabled = !isOpen;

            buttonOpenClose.Text = isOpen ? "关闭串口" : "打开串口";
            buttonSendOnce.Enabled = isOpen;
            buttonStartStop.Enabled = isOpen;
            buttonStartStop.Text = _sendTimer.Enabled ? "停止连续发送" : "开始连续发送";

            UpdateStatus();
        }

        private void UpdateStatus()
        {
            var portText = _serialPort?.IsOpen == true ? _serialPort.PortName : "未打开";
            var modeText = _sendTimer.Enabled ? $"连续发送中，间隔 {_sendTimer.Interval} 毫秒" : "空闲";
            toolStripStatusLabel.Text = $"串口: {portText} | 状态: {modeText} | 已发送: {_sendCount} 条";
        }

        private void AppendLog(string message)
        {
            if (IsDisposed)
            {
                return;
            }

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => AppendLog(message)));
                return;
            }

            textBoxLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
            textBoxLog.SelectionStart = textBoxLog.TextLength;
            textBoxLog.ScrollToCaret();
        }

        private Parity GetSelectedParity()
        {
            return Enum.TryParse(comboParity.Text, out Parity parity) ? parity : Parity.None;
        }

        private StopBits GetSelectedStopBits()
        {
            return Enum.TryParse(comboStopBits.Text, out StopBits stopBits) ? stopBits : StopBits.One;
        }

        private static int GetPortNumber(string portName)
        {
            return portName.StartsWith("COM", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(portName.AsSpan(3), NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)
                    ? number
                    : int.MaxValue;
        }
    }
}
