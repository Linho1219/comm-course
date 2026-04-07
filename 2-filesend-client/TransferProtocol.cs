using System.Net.Sockets;
using System.Text.Json;

namespace _2_filesend_client
{
    internal static class TransferProtocol
    {
        private const int MaxHeaderSize = 1024 * 1024;

        public static async Task WriteJsonAsync<T>(NetworkStream stream, T payload, CancellationToken token)
        {
            var body = JsonSerializer.SerializeToUtf8Bytes(payload);
            var header = BitConverter.GetBytes(body.Length);

            await stream.WriteAsync(header.AsMemory(0, header.Length), token);
            await stream.WriteAsync(body.AsMemory(0, body.Length), token);
            await stream.FlushAsync(token);
        }

        public static async Task<T> ReadJsonAsync<T>(NetworkStream stream, CancellationToken token)
        {
            var header = new byte[4];
            await ReadExactlyAsync(stream, header, token);

            var length = BitConverter.ToInt32(header, 0);
            if (length <= 0 || length > MaxHeaderSize)
            {
                throw new InvalidDataException("协议头长度非法。");
            }

            var body = new byte[length];
            await ReadExactlyAsync(stream, body, token);

            var value = JsonSerializer.Deserialize<T>(body);
            if (value is null)
            {
                throw new InvalidDataException("协议消息反序列化失败。");
            }

            return value;
        }

        private static async Task ReadExactlyAsync(NetworkStream stream, byte[] buffer, CancellationToken token)
        {
            var offset = 0;
            while (offset < buffer.Length)
            {
                var read = await stream.ReadAsync(buffer.AsMemory(offset, buffer.Length - offset), token);
                if (read <= 0)
                {
                    throw new IOException("连接已断开。");
                }

                offset += read;
            }
        }
    }
}
