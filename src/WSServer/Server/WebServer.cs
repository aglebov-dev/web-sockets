using System;
using System.Threading;
using System.Collections.Generic;
using WSServer.Abstractions;
using WSServer.Implementations;

namespace WSServer.Server
{
    internal class WebServer
    {
        private const string DEFAULT_SERVER_PREFIX = "http://localhost:5000";

        private readonly Configuration _configuration;
        private readonly ILogger _logger;
        private readonly CancellationToken _token;

        private readonly object _lock;
        private readonly Dictionary<string, WebListener> _listeners;
        private readonly CancellationTokenRegistration _cancellationTokenRegistration;

        public WebServer(Configuration configuration, CancellationToken token = default, ILogger logger = default)
        {
            _configuration = configuration;
            _token = token;
            _logger = logger ?? Logger.Empty;

            _lock = new object();
            _listeners = new Dictionary<string, WebListener>();
            _cancellationTokenRegistration = token.Register(CleanUp);
        }

        public IListener CreateListener(string uri)
        {
            return CreateListenerInternal(uri ?? DEFAULT_SERVER_PREFIX);
        }

        private WebListener CreateListenerInternal(string uri)
        {
            var isCanceled = _token.IsCancellationRequested;
            lock (_lock)
            {
                if (isCanceled)
                {
                    throw new Exception($"Server was stoped");
                }
                else if (_listeners.ContainsKey(uri))
                {
                    throw new Exception($"Uri '{uri}' allready uses");
                }
                else
                {
                    var listener = new WebListener(uri, _configuration.Connections, _logger, _token);
                    _listeners.Add(uri, listener);

                    return listener;
                }
            }
        }

        private void CleanUp()
        {
            _cancellationTokenRegistration.Dispose();
            lock (_lock)
            {
                _listeners.Clear();
            }
        }
    }
}
