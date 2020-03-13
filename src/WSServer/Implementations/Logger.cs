using WSServer.Abstractions;

namespace WSServer.Implementations
{
    public static class Logger
    {
        public static ILogger Empty { get; }

        static Logger()
        {
            Empty = new EmptyLogger();
        }

        private class EmptyLogger : ILogger
        {
            public void Log(string message) { }
        }
    }
}