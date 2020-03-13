using System.Threading;
using System.Threading.Tasks;

namespace WSServer.Abstractions
{
    public interface IJob
    {
        Task RunAsync(CancellationToken token);
    }
}
