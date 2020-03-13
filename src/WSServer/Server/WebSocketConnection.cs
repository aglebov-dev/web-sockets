using System;
using System.Text;
using System.Threading;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace WSServer.Server
{
    internal class WebSocketConnection
    {
        private readonly long _connectionId;
        private readonly ConnectionSettings _connectionSettings;
        private readonly HttpListenerWebSocketContext _context;
        private readonly WebSocket _socket;
        private readonly CancellationToken _token;
        private readonly CancellationTokenRegistration _cancellationTokenRegistration;

        public event Action<long> Close;
        public event Action<long, byte[]> Receive;

        public bool IsOpen { get; private set; }

        public WebSocketConnection(
            long connectionId,
            ConnectionSettings connectionSettings,
            HttpListenerWebSocketContext context,
            CancellationToken token)
        {
            _connectionId = connectionId;
            _connectionSettings = connectionSettings;
            _context = context;
            _socket = _context.WebSocket;
            _token = token;

            _cancellationTokenRegistration = token.Register(CleanUp);
        }

        public async Task ReceiveAsync()
        {
            var buffer = new byte[_connectionSettings.BufferSize];
            var messageParts = new LinkedList<byte[]>();
            var messageLength = 0L;

            try
            {
                while (_socket.State == WebSocketState.Open && !_token.IsCancellationRequested)
                {
                    IsOpen = true;
                    var receiveResult = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), _token);
                    if (receiveResult.MessageType == WebSocketMessageType.Text)
                    {
                        var messagePart = CreateMessagePart(buffer, receiveResult);
                        messageLength += messagePart.Length;
                        messageParts.AddLast(messagePart);

                        if (receiveResult.EndOfMessage)
                        {
                            var result = CombineMessageParts(messageLength, messageParts);
                            messageParts.Clear();
                            messageLength = 0;

                            Receive?.Invoke(_connectionId, result);
                        }
                    }
                    else
                    {
                        var description = receiveResult.MessageType == WebSocketMessageType.Close
                            ? string.Empty
                            : "Type of data isn't supported";

                        var type = receiveResult.MessageType == WebSocketMessageType.Close
                            ? WebSocketCloseStatus.NormalClosure
                            : WebSocketCloseStatus.InvalidMessageType;

                        await _socket.CloseAsync(type, description, _token);
                        CleanUp();
                    }
                }
            }
            catch (Exception e)
            {
                CleanUp();
                if (e is OperationCanceledException)
                {
                    return;
                }

                throw;
            }
        }

        public async Task SendAsync(string message, CancellationToken token)
        {
            if (!_token.IsCancellationRequested && _socket.State == WebSocketState.Open)
            {
                var array = Encoding.UTF8.GetBytes(message);
                var segment = new ArraySegment<byte>(array, 0, array.Length);
                await _socket.SendAsync(segment, WebSocketMessageType.Text, true, token);
            }
        }

        private byte[] CreateMessagePart(byte[] buffer, WebSocketReceiveResult receiveResult)
        {
            var length = receiveResult.Count;
            var messagePart = new byte[length];
            Array.Copy(buffer, 0, messagePart, 0, length);

            return messagePart;
        }

        private byte[] CombineMessageParts(long messageLength, LinkedList<byte[]> parts)
        {
            var result = new byte[messageLength];
            var offset = 0L;

            foreach (var item in parts)
            {
                Array.Copy(item, 0, result, offset, item.Length);
                offset += item.Length;
            }

            return result;
        }

        private void CleanUp()
        {
            IsOpen = false;
            Close?.Invoke(_connectionId);
            _cancellationTokenRegistration.Unregister();
            _socket.Dispose();
        }
    }
}
