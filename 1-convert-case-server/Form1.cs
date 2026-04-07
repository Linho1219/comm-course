using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace _1_convert_case_server
{
    public partial class Form1 : Form
    {
        private readonly ListenerState _upperState;
        private readonly ListenerState _lowerState;

        public Form1()
        {
            InitializeComponent();

            _upperState = new ListenerState(
                "大写",
                s => s.ToUpperInvariant(),
                numericToUpperPort,
                buttonStartToUpper,
                buttonStopToUpper);

            _lowerState = new ListenerState(
                "小写",
                s => s.ToLowerInvariant(),
                numericToLowerPort,
                buttonStartToLower,
                buttonStopToLower);

            buttonStartToUpper.Click += (_, _) => StartListener(_upperState);
            buttonStopToUpper.Click += (_, _) => StopListener(_upperState);
            buttonStartToLower.Click += (_, _) => StartListener(_lowerState);
            buttonStopToLower.Click += (_, _) => StopListener(_lowerState);
            FormClosing += Form1_FormClosing;
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            StopListener(_upperState);
            StopListener(_lowerState);
        }

        private void StartListener(ListenerState state)
        {
            if (state.IsRunning)
            {
                return;
            }

            var port = (int)state.PortControl.Value;
            if (!TryCreateListenSocket(port, out var listenSocket, out var error))
            {
                AppendLog($"[{state.Name}] 启动失败: {error}");
                return;
            }

            state.ListenSocket = listenSocket;
            state.Cts = new CancellationTokenSource();
            state.AcceptLoopTask = Task.Run(() => AcceptLoopAsync(state, state.Cts.Token));

            state.StartButton.Enabled = false;
            state.StopButton.Enabled = true;
            state.PortControl.Enabled = false;

            AppendLog($"[{state.Name}] 监听已启动，端口 {port}。");
        }

        private void StopListener(ListenerState state)
        {
            if (!state.IsRunning)
            {
                return;
            }

            state.Cts?.Cancel();
            var closedClients = ForceCloseAllClients(state);

            try
            {
                state.ListenSocket?.Close();
            }
            catch
            {
            }

            state.ListenSocket = null;
            state.Cts?.Dispose();
            state.Cts = null;
            state.AcceptLoopTask = null;

            state.StartButton.Enabled = true;
            state.StopButton.Enabled = false;
            state.PortControl.Enabled = true;

            AppendLog($"[{state.Name}] 监听已停止，已断开 {closedClients} 个客户端连接。");
        }

        private bool TryCreateListenSocket(int port, out Socket? listenSocket, out string error)
        {
            try
            {
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(new IPEndPoint(IPAddress.Any, port));
                socket.Listen(100);

                listenSocket = socket;
                error = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                listenSocket = null;
                error = ex.Message;
                return false;
            }
        }

        private async Task AcceptLoopAsync(ListenerState state, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                Socket? client = null;
                try
                {
                    if (state.ListenSocket is null)
                    {
                        break;
                    }

                    client = await AcceptAsync(state.ListenSocket);
                    RegisterClient(state, client);
                    _ = Task.Run(() => HandleClientAsync(state, client, token), token);
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (SocketException ex)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    AppendLog($"[{state.Name}] Accept 错误: {ex.Message}");
                    client?.Close();
                }
                catch (Exception ex)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    AppendLog($"[{state.Name}] Accept 异常: {ex.Message}");
                    client?.Close();
                }
            }
        }

        private async Task HandleClientAsync(ListenerState state, Socket client, CancellationToken token)
        {
            var buffer = new byte[4096];
            var remote = client.RemoteEndPoint?.ToString() ?? "unknown";
            AppendLog($"[{state.Name}] 客户端连接: {remote}");

            try
            {
                while (!token.IsCancellationRequested)
                {
                    var receivedCount = await ReceiveAsync(client, buffer);
                    if (receivedCount == 0)
                    {
                        break;
                    }
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    var input = Encoding.ASCII.GetString(buffer, 0, receivedCount);
                    var output = state.Converter(input);

                    AppendLog($"[{state.Name}] 收到: {input}");
                    await SendAsync(client, Encoding.ASCII.GetBytes(output));
                    AppendLog($"[{state.Name}] 回发: {output}");
                }
            }
            catch (SocketException ex)
            {
                if (!token.IsCancellationRequested)
                {
                    AppendLog($"[{state.Name}] 客户端通信错误({remote}): {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested)
                {
                    AppendLog($"[{state.Name}] 客户端通信异常({remote}): {ex.Message}");
                }
            }
            finally
            {
                UnregisterClient(state, client);
                try
                {
                    client.Shutdown(SocketShutdown.Both);
                }
                catch
                {
                }

                client.Close();
                AppendLog($"[{state.Name}] 客户端断开: {remote}");
            }
        }

        private static void RegisterClient(ListenerState state, Socket client)
        {
            state.Clients.TryAdd(client, 0);
        }

        private static void UnregisterClient(ListenerState state, Socket client)
        {
            state.Clients.TryRemove(client, out _);
        }

        private static int ForceCloseAllClients(ListenerState state)
        {
            var closed = 0;
            foreach (var client in state.Clients.Keys)
            {
                if (state.Clients.TryRemove(client, out _))
                {
                    try
                    {
                        // Force an immediate reset so peer detects stop promptly.
                        client.LingerState = new LingerOption(true, 0);
                        client.Close();
                    }
                    catch
                    {
                    }

                    closed++;
                }
            }

            return closed;
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

        private static Task<Socket> AcceptAsync(Socket listenSocket)
        {
            var tcs = new TaskCompletionSource<Socket>(TaskCreationOptions.RunContinuationsAsynchronously);
            var args = new SocketAsyncEventArgs();

            void Completed(object? _, SocketAsyncEventArgs eventArgs)
            {
                eventArgs.Completed -= Completed;
                if (eventArgs.SocketError == SocketError.Success && eventArgs.AcceptSocket is not null)
                {
                    tcs.TrySetResult(eventArgs.AcceptSocket);
                }
                else
                {
                    tcs.TrySetException(new SocketException((int)eventArgs.SocketError));
                }

                eventArgs.Dispose();
            }

            args.Completed += Completed;

            if (!listenSocket.AcceptAsync(args))
            {
                Completed(listenSocket, args);
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

        private sealed class ListenerState
        {
            public ListenerState(
                string name,
                Func<string, string> converter,
                NumericUpDown portControl,
                Button startButton,
                Button stopButton)
            {
                Name = name;
                Converter = converter;
                PortControl = portControl;
                StartButton = startButton;
                StopButton = stopButton;
            }

            public string Name { get; }
            public Func<string, string> Converter { get; }
            public NumericUpDown PortControl { get; }
            public Button StartButton { get; }
            public Button StopButton { get; }
            public Socket? ListenSocket { get; set; }
            public CancellationTokenSource? Cts { get; set; }
            public Task? AcceptLoopTask { get; set; }
            public ConcurrentDictionary<Socket, byte> Clients { get; } = new();
            public bool IsRunning => ListenSocket is not null;
        }
    }
}
