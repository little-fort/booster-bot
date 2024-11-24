
using BoosterBot.Helpers;

namespace BoosterBot
{
    internal static class Logger
    {
        public static void Log(LocalizationManager localizer, string key, string logPath, bool directWrite = false)
        {
            var time = $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] ";
            Console.WriteLine(time + (directWrite ? key : localizer.GetString(key)));

            if (logPath != null)
            {
                using var writer = new StreamWriter(logPath, true);
                writer.WriteLine(time + (directWrite ? key : localizer.GetString(key)));
            }
        }
    }
}
