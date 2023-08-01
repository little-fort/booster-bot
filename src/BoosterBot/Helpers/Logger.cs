
namespace BoosterBot
{
    internal static class Logger
    {
        public static void Log(string text, string logPath)
        {
            var line = $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] {text}";
            Console.WriteLine(line);

            if (logPath != null)
            {
                using var writer = new StreamWriter(logPath, true);
                writer.WriteLine(line);
            }
        }
    }
}
