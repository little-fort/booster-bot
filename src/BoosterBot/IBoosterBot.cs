using System.Threading;
using System.Threading.Tasks;

namespace BoosterBot
{
    internal interface IBoosterBot : IDisposable
    {
        Task RunAsync(CancellationToken token);
        void Run();
        void Pause();
        void Resume();
        void Stop(); 
        void Cancel();
        string GetLogPath();
        bool IsStopped { get; }
        event Action<string> OnLogMessage;
    }
}