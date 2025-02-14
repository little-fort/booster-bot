
namespace BoosterBot
{
    internal interface IBoosterBot
    {
        void Run();

        string GetLogPath();
        void CheckForPause();
    }
}
