using System;

namespace WSServer.Contracts
{
    public class Process
    {
        public int Pid { get; set; }
        public string User { get; set; }
        public double Cpu { get; set; }
        public long Memory { get; set; }
        public TimeSpan Time { get; set; }
        public string Name { get; set; }
    }
}
