using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WSServer.Abstractions;
using WSServer.Contracts;

namespace WSServer.Jobs
{
    internal class ProcessesJob : IJob
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
                var processes = (await GetInfo()).Where(x => x != null);
                var json = System.Text.Json.JsonSerializer.Serialize(processes, Common.JsonSerializerOptions);
                await _listener.SendBroadcastMessage(json);
                await Task.Delay(_settings.Interval, token);
            }
        }

        private Task<Process[]> GetInfo()
        {
            var tasks = System.Diagnostics.Process.GetProcesses()
                .Select(X)
                .ToArray();

            return Task.WhenAll(tasks);

        }

        private async Task<Process>  X(System.Diagnostics.Process process)
        {
            try
            {
                var startTime = DateTime.UtcNow;
                var startCpuUsage = process.TotalProcessorTime;
                await Task.Delay(100);

                var endTime = DateTime.UtcNow;
                var endCpuUsage = process.TotalProcessorTime;
                var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
                var totalMsPassed = (endTime - startTime).TotalMilliseconds;
                var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
                var cpu = cpuUsageTotal * 100;

                return new Process
                {
                    Pid = process.Id,
                    Name = process.ProcessName,
                    Cpu = cpu,
                    Memory = process.PagedMemorySize64,
                    Time = DateTime.UtcNow - process.StartTime.ToUniversalTime(),
                    User = "unknown"
                };
            }
            catch
            {
                return default;
            }
        }
    }
}
