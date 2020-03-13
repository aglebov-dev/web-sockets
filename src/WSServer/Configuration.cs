using System;

namespace WSServer
{
    internal class Configuration
    {
        public string Uri { get; set; }
        public ConnectionSettings Connections { get; set; }
        public ProcessesJobSettings ProcessesJob { get; set; }
    }

    internal class ConnectionSettings
    {
        public long BufferSize { get; set; }
    }

    internal class ProcessesJobSettings
    {
        public TimeSpan Interval { get; set; }
    }
}
