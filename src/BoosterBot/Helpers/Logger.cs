
namespace BoosterBot
{
    internal static class Logger
    {
        public static void Log(string text) => Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] {text}");
    }
}
