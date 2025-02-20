using System.IO;
using BoosterBot.Helpers;
using BoosterBot.Models;
using System.Windows.Controls;

namespace BoosterBot
{
    internal static class Logger
    {
        // 声明一个事件或委托，UI 端可以订阅它来更新日志区域
        public static event Action<string> OnLogUpdated;

        public static void Log(LocalizationManager localizer, string key, string logPath, bool directWrite = false, List<FindReplaceValue> replace = null)
        {
            var time = $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] ";
            string logMessage = string.Empty;

            if (directWrite)
            {
                logMessage = key;
                if (logPath != null)
                {
                    using var writer = new StreamWriter(logPath, true);
                    writer.WriteLine(time + key);
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

                logMessage = localized;

                if (logPath != null)
                {
                    using var writer = new StreamWriter(logPath, true);
                    writer.WriteLine(time + neutral);
                }
            }

            // 只发送日志到 UI
            OnLogUpdated?.Invoke(time + logMessage);
        }
    }
}
