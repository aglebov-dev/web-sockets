using System.Threading.Tasks;

namespace WSServer.Abstractions
{
    public interface IListener
    {
        Task ListenAsync();
        Task SendBroadcastMessage(string message);
        Task SendMessage(long connectionId, string message);
    }
}
