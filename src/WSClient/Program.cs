using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WSClient.UI;
using WSServer.Contracts;

namespace WSClient
{
    class Program
    {
        private static JsonSerializerOptions _jsonOptions;
        private static Rectangle _size;
        private static UI.Table _table;

        static void Main(string[] args)
        {
            Console.Clear();
            Console.CursorVisible = false;
            _jsonOptions = new JsonSerializerOptions();
            _jsonOptions.Converters.Add(new TimeSpanConverter());
            _size = new Rectangle(Console.WindowWidth - 2, Console.WindowHeight - 2);
            _table = new Table();

            var headers = new[]
            {
                new TableHeader(10, nameof(Process.Pid),    UI.Binding.Create<Process>(x => x.Pid.ToString())) { Alignment = Alignment.Center },
                new TableHeader(40, nameof(Process.Name),   UI.Binding.Create<Process>(x => x.Name)),
                new TableHeader(20, nameof(Process.User),   UI.Binding.Create<Process>(x => x.User)),
                new TableHeader(10, nameof(Process.Cpu),    UI.Binding.Create<Process>(x => x.Cpu.ToString("n2"))) { Alignment = Alignment.Right },
                new TableHeader(15, nameof(Process.Memory), UI.Binding.Create<Process>(x => x.Memory.ToString("n0", CultureInfo.CreateSpecificCulture("ru")))) { Alignment = Alignment.Right },
                new TableHeader(11, nameof(Process.Time),   UI.Binding.Create<Process>(x => x.Time.ToString("hh\\:mm\\:ss"))) { Alignment = Alignment.Center }
            };

            foreach (var header in headers)
            {
                _table.Headers.Add(header);
            }

            var uri = args?.FirstOrDefault() ?? "ws://localhost:47055/";
            ConnectAsync(uri)
                .GetAwaiter()
                .GetResult();
        }

        private static async Task ConnectAsync(string uri)
        {
            var client = new ClientWebSocket();
            var token = CancellationToken.None;
            await client.ConnectAsync(new Uri(uri), token);
            if (client.State == WebSocketState.Open)
            {
                var parts = new LinkedList<byte[]>();
                while (!token.IsCancellationRequested)
                {
                    await ReceiveAsync(client, parts, token);
                }
            }
            else
            {
                throw new Exception();
            }
        }

        private static async Task ReceiveAsync(ClientWebSocket client, LinkedList<byte[]> parts, CancellationToken token)
        {
            var buffer = new byte[4 * 1024];
            var response = await client.ReceiveAsync(new Memory<byte>(buffer), token);
            var array = new byte[response.Count];
            Array.Copy(buffer, 0, array, 0, array.Length);

            parts.AddLast(array);

            if (response.EndOfMessage)
            {
                var data = CombineMessageParts(parts);
                parts.Clear();
                ProcessMessage(data);
            }
        }

        private static byte[] CombineMessageParts(LinkedList<byte[]> parts)
        {
            var messageLength = parts.Sum(x => x.Length);
            var result = new byte[messageLength];
            var offset = 0L;

            foreach (var item in parts)
            {
                Array.Copy(item, 0, result, offset, item.Length);
                offset += item.Length;
            }

            return result;
        }

        private static void ProcessMessage(byte[] message)
        {
            var json = Encoding.UTF8.GetString(message);
            var data = JsonSerializer.Deserialize<Process[]>(json, _jsonOptions)
                .OrderByDescending(x => x.Cpu)
                .ToList();

            _table.DataSource = data;

            _table.PopulateData();
            _table.EvaluateSize(_size);
            _table.Render(new Point(0, 0), _size);
        }
    }
}
