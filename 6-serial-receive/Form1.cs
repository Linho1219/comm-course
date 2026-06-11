using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Text;

namespace _6_serial_receive
{
    public partial class Form1 : Form
    {
        private SerialPort? _serialPort;
        private int _receiveBytes;
        private int _receiveLines;

        public Form1()
        {
            InitializeComponent();

            comboBaudRate.SelectedItem = "9600";
            comboParity.SelectedItem = "None";
            comboDataBits.SelectedItem = "8";
            comboStopBits.SelectedItem = "One";

            buttonRefreshPorts.Click += ButtonRefreshPorts_Click;
            buttonOpenClose.Click += ButtonOpenClose_Click;
            buttonClear.Click += ButtonClear_Click;
            buttonExit.Click += ButtonExit_Click;
            FormClosing += Form1_FormClosing;

            LoadPortNames();
            UpdateUiState();
            AppendReceivedText("请选择与发送端配对的端口。\r\n");
        }

        private void ButtonRefreshPorts_Click(object? sender, EventArgs e)
        {
            LoadPortNames();
        }

        private void ButtonOpenClose_Click(object? sender, EventArgs e)
        {
            if (_serialPort?.IsOpen == true)
            {
                CloseSerialPort("串口已关闭。\r\n");
                return;
            }

            OpenSerialPort();
        }

        private void ButtonClear_Click(object? sender, EventArgs e)
        {
            textBoxReceive.Clear();
            _receiveBytes = 0;
            _receiveLines = 0;
            UpdateStatus();
        }

        private void ButtonExit_Click(object? sender, EventArgs e)
        {
            Close();
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            CloseSerialPort();
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
            else if (ports.Contains("COM6", StringComparer.OrdinalIgnoreCase))
            {
                comboPort.SelectedItem = "COM6";
            }
            else
            {
                comboPort.SelectedIndex = 0;
            }

            UpdateStatus();
        }

        private void OpenSerialPort()
        {
            if (string.IsNullOrWhiteSpace(comboPort.Text))
            {
                AppendReceivedText("没有可用串口，请先刷新串口列表。\r\n");
                return;
            }

            try
            {
                _serialPort = CreateConfiguredSerialPort();
                _serialPort.DataReceived += SerialPort_DataReceived;
                _serialPort.Open();

                AppendReceivedText($"已打开 {_serialPort.PortName}: {_serialPort.BaudRate}, {_serialPort.Parity}, {_serialPort.DataBits}, {_serialPort.StopBits}。\r\n");
                UpdateUiState();
            }
            catch (Exception ex)
            {
                AppendReceivedText($"打开串口失败: {ex.Message}\r\n");
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
            var serialPort = _serialPort;
            _serialPort = null;

            if (serialPort is not null)
            {
                serialPort.DataReceived -= SerialPort_DataReceived;

                try
                {
                    if (serialPort.IsOpen)
                    {
                        serialPort.Close();
                    }
                }
                catch (Exception ex)
                {
                    AppendReceivedText($"关闭串口时发生异常: {ex.Message}\r\n");
                }
                finally
                {
                    serialPort.Dispose();
                }
            }

            if (!string.IsNullOrEmpty(message))
            {
                AppendReceivedText(message);
            }

            UpdateUiState();
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                var serialPort = (SerialPort)sender;
                var data = serialPort.ReadExisting();
                if (data.Length == 0)
                {
                    return;
                }

                BeginInvoke(new Action(() => AppendReceivedData(data)));
            }
            catch (IOException ex)
            {
                BeginInvoke(new Action(() => CloseSerialPort($"接收异常: {ex.Message}\r\n")));
            }
            catch (ObjectDisposedException)
            {
            }
            catch (InvalidOperationException)
            {
            }
        }

        private void AppendReceivedData(string data)
        {
            _receiveBytes += (_serialPort?.Encoding ?? Encoding.ASCII).GetByteCount(data);
            _receiveLines += data.Count(character => character == '\n');
            AppendReceivedText(data);
            UpdateStatus();
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

            textBoxReceive.AppendText(text);

            if (checkBoxAutoScroll.Checked)
            {
                textBoxReceive.SelectionStart = textBoxReceive.TextLength;
                textBoxReceive.ScrollToCaret();
            }
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
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            var portText = _serialPort?.IsOpen == true ? _serialPort.PortName : "未打开";
            toolStripStatusLabel.Text = $"串口: {portText} | 已接收: {_receiveBytes} 字节 / {_receiveLines} 行";
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
