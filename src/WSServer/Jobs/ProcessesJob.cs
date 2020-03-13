using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WSServer.Abstractions;
using WSServer.Contracts;

namespace WSServer.Jobs
{
    internal class ProcessesJob: IJob
    {
        private readonly ProcessesJobSettings _settings;
        private readonly IListener _listener;

        public ProcessesJob(IListener listener, ProcessesJobSettings settings)
        {
            _settings = settings;
            _listener = listener;
        }

        public async Task RunAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var processes = GetInfo().ToArray();
                var json = System.Text.Json.JsonSerializer.Serialize(processes, Common.JsonSerializerOptions);
                await _listener.SendBroadcastMessage(json);
                await Task.Delay(_settings.Interval, token);
            }
        }

        private IEnumerable<Process> GetInfo()
        {
            foreach (var process in System.Diagnostics.Process.GetProcesses())
            {
                var data = default(Process);
                try
                {
                    var time = DateTime.UtcNow - process.StartTime.ToUniversalTime();
                    var cpu = 100 * process.TotalProcessorTime / time;
                    data = new Process
                    {
                        Pid = process.Id,
                        Name = process.ProcessName,
                        Cpu = cpu,
                        Memory = process.PagedMemorySize64,
                        Time = time,
                        User = "process.StartInfo.FileName"
                    };
                }
                catch { }

                if (data != null)
                {
                    yield return data;
                }
            }
        }
    }
}
