using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using WSServer.Abstractions;
using WSServer.Extensions;
using WSServer.Implementations;

namespace WSServer.Server
{
    internal class WebListener: IListener
    {
        private readonly string _prefix;
        private readonly ConnectionSettings _connectionSettings;
        private readonly ILogger _logger;
        private readonly CancellationToken _token;
        private readonly object _lock;
        private readonly Dictionary<long, WebSocketConnection> _connections;
        private readonly HttpListener _listener;
        private readonly TaskCompletionSource<HttpListenerContext> _cancelTaskSource;
        private readonly CancellationTokenRegistration _cancellationTokenRegistration;

        private long _connectionCounter;

        public WebListener(
            string prefix,
            ConnectionSettings connectionSettings,
            ILogger logger = default,
            CancellationToken token = default)
        {
            _prefix = prefix.TrimEnd('/') + "/";
            _connectionSettings = connectionSettings;
            _logger = logger ?? Logger.Empty;
            _token = token;
            _lock = new object();
            _connections = new Dictionary<long, WebSocketConnection>();
            _cancelTaskSource = new TaskCompletionSource<HttpListenerContext>();
            _listener = new HttpListener();
            _listener.Prefixes.Add(_prefix);

            _cancellationTokenRegistration = token.Register(CleanUp);
        }

        public async Task SendMessage(long connectionId, string message)
        {
            if (_connections.TryGetValue(connectionId, out var connection))
            {
                await connection.SendAsync(message, _token);
            }
        }

        public Task SendBroadcastMessage(string message)
        {
            var tasks = _connections.Values.Select(x => x.SendAsync(message, _token)).ToArray();

            return Task.WhenAll(tasks);
        }

        public async Task ListenAsync()
        {
            var isCanceled = _token.IsCancellationRequested;
            lock (_lock)
            {
                if (isCanceled)
                {
                    return;
                }
                else if (_listener.IsListening)
                {
                    throw new Exception("Listener already started");
                }
                else
                {
                    _listener.Start();
                }
            }

            if (_listener.IsListening)
            {
                await ReceivingAsync(_listener);
            }
            else
            {
                throw new Exception("Failed to start");
            }
        }

        private async Task ReceivingAsync(HttpListener listener)
        {
            _logger.Log($"Start listening. Uri '{_prefix}'");
            while (!_token.IsCancellationRequested)
            {
                var contextTask = listener.GetContextAsync();
                var cancelTask = _cancelTaskSource.Task;
                await Task.WhenAny(cancelTask, contextTask);

                if (contextTask.Status == TaskStatus.RanToCompletion)
                {
                    var context = await contextTask;
                    if (context.Request.IsWebSocketRequest)
                    {
                        await AcceptWebSocketAsync(context);
                    }
                    else
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        context.Response.Close();
                    }
                }
            }
        }

        private async Task AcceptWebSocketAsync(HttpListenerContext context)
        {
            try
            {
                var wsContext = await context.AcceptWebSocketAsync(default);
                var connectionId = Interlocked.Increment(ref _connectionCounter);
                var connection = new WebSocketConnection(connectionId, _connectionSettings, wsContext, _token);
                _connections.Add(connectionId, connection);
                _ = connection.ReceiveAsync();

                connection.Close += RemoveConnection;
                connection.Receive += Receive;

                if (!connection.IsOpen)
                {
                    RemoveConnection(connectionId);
                }

                _logger.Log($"Connection {connectionId} was started");
            }
            catch (Exception e)
            {
                _logger.Log(e.GetFullMessage());

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.Close();
            }
        }

        private void Receive(long connectionId, byte[] message)
        {
            //TODO
        }

        private void RemoveConnection(long connectionId)
        {
            _logger.Log($"Connection {connectionId} was closed");
            if (_connections.TryGetValue(connectionId, out var connection))
            {
                _connections.Remove(connectionId);
                connection.Close -= RemoveConnection;
                connection.Receive -= Receive;
            }
        }

        private void CleanUp()
        {
            lock (_lock)
            {
                _cancellationTokenRegistration.Unregister();
                _cancelTaskSource.SetResult(default);
                
                foreach (var connectionId in _connections.Keys)
                {
                    RemoveConnection(connectionId);
                }
                
                _listener.Stop();
                _listener.Close();
            }
        }
    }
}
