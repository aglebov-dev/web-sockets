using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WSServer.Abstractions;
using WSServer.Implementations;
using WSServer.Jobs;
using WSServer.Server;

namespace WSServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += delegate { cts.Cancel(); };

            var logger = new DefaultLogger();
            var configuration = GetConfiguration();
            var server = new WebServer(configuration, cts.Token, logger);
            
            var listener = server.CreateListener(configuration.Uri);
            var jobs = new IJob[]
            {
                new ProcessesJob(listener, configuration.ProcessesJob)
            };

            var tasks = jobs
                .Select(x => x.RunAsync(cts.Token))
                .Append(listener.ListenAsync())
                .ToArray();

            Task.WhenAll(tasks)
                .GetAwaiter()
                .GetResult();
        }

        private static Configuration GetConfiguration()
        {
            var settingspath = Path.Combine(Directory.GetCurrentDirectory(), "application.json");
            var json = File.Exists(settingspath)
                ? File.ReadAllText(settingspath)
                : "{}";

            return JsonSerializer.Deserialize<Configuration>(json, Common.JsonSerializerOptions);
        }
    }
}
