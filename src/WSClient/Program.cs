using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WSServer.Contracts;

namespace WSClient
{
    class Program
    {
        private static JsonSerializerOptions _jsonOptions;
        private static Dictionary<int, Process> _state;

        static void Main(string[] args)
        {
            _jsonOptions = new System.Text.Json.JsonSerializerOptions();
            _jsonOptions.Converters.Add(new TimeSpanConverter());
            _state = new Dictionary<int, Process>();

            ConnectAsync()
                .GetAwaiter()
                .GetResult();
        }

        private static async Task ConnectAsync()
        {
            var client = new ClientWebSocket();
            var token = CancellationToken.None;
            await client.ConnectAsync(new Uri("ws://localhost:8085/"), token);
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
                .OrderBy(x => x.Pid)
                .ToArray();

            var dictionary = data.ToLookup(x => x.Pid);
            var update = data.Where(x => _state.ContainsKey(x.Pid));
            var remove = _state.Where(x => !dictionary.Contains(x.Key));
            var add = data.Except(update).ToLookup(x => x.Pid);

            foreach (var item in remove)
            {
                _state.Remove(item.Key);
            }

            foreach (var item in add)
            {
                _state.Add(item.Key, item.First());
            }

            foreach (var item in update)
            {
                _state.Remove(item.Pid);
            }

            ReRender();
        }

        private static void ReRender()
        {
            //Console.Clear();
            //foreach (var process in data.OrderBy(x => x.Pid))
            //{
            //    Console.BufferWidth = 200;
            //    Console.CursorLeft = 0;
            //    Console.Write(process.Pid);

            //    Console.CursorLeft = 10;
            //    Console.Write(process.User.Substring(0, Math.Max(process.User.Length, 50)));

            //    Console.CursorLeft = 65;
            //    Console.Write(process.Name.Substring(0, 90));

            //    Console.CursorLeft = 115;
            //    Console.Write(process.Time.ToString("hh\\:mm\\:ss"));

            //    Console.CursorLeft = 140;
            //    Console.Write(process.Cpu.ToString("n2"));

            //    Console.CursorLeft = 180;
            //    Console.Write($"{process.Memory / 1024:n0} Kb");

            //    Console.WriteLine();
            //}
        }

       


       
    }
}
