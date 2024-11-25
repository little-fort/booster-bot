
using BoosterBot.Helpers;
using BoosterBot.Models;

namespace BoosterBot
{
    internal static class Logger
    {
        public static void Log(LocalizationManager localizer, string key, string logPath, bool directWrite = false, List<FindReplaceValue> replace = null)
        {
            var time = $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] ";
            if (directWrite)
            {
                Console.WriteLine(key);
                if (logPath != null)
                {
                    using var writer = new StreamWriter(logPath, true);
                    writer.WriteLine(time + (directWrite ? key : localizer.GetNeutralString(key)));
                }
            }
            else
            {
                var localized = localizer.GetString(key);
                var neutral = localizer.GetNeutralString(key);

                if (replace != null)
                    foreach (var replacement in replace)
                    {
                        localized = localized.Replace(replacement.Token, replacement.Value);
                        neutral = neutral.Replace(replacement.Token, replacement.Value);
                    }

                Console.WriteLine(time + localized);

                if (logPath != null)
                {
                    using var writer = new StreamWriter(logPath, true);
                    writer.WriteLine(time + neutral);
                }
            }
            
        }
    }
}
