using System;
using WSServer.Abstractions;

namespace WSServer.Implementations
{
    public class DefaultLogger : ILogger
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
