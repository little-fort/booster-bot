using System;
using System.Collections.Generic;
using System.IO;
using BoosterBot.Helpers;
using BoosterBot.Models;

namespace BoosterBot
{
    internal static class Logger
    {
        private static LocalizationManager? _globalLocalizer;

        public static void Log(LocalizationManager localizer, string key, string logPath, bool directWrite = false, List<FindReplaceValue>? replace = null)
        {
            var time = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ";

            if (localizer == null)
            {
                if (_globalLocalizer == null)
                {
                    Console.WriteLine($"{time}ERROR: LocalizationManager is not initialized. Logging key directly: {key}");
                    if (logPath != null)
                    {
                        using var writer = new StreamWriter(logPath, true);
                        writer.WriteLine(time + key);
                    }
                    return;
                }
                localizer = _globalLocalizer;
            }

            if (directWrite)
            {
                Console.WriteLine(key);
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
                {
                    foreach (var replacement in replace)
                    {
                        localized = localized.Replace(replacement.Token, replacement.Value);
                        neutral = neutral.Replace(replacement.Token, replacement.Value);
                    }
                }

                Console.WriteLine(time + localized);

                if (logPath != null)
                {
                    using var writer = new StreamWriter(logPath, true);
                    writer.WriteLine(time + neutral);
                }
            }
        }

        public static void SetGlobalLocalizer(LocalizationManager localizer)
        {
            _globalLocalizer = localizer;
        }

        internal static void LogError(string v)
        {
            throw new NotImplementedException();
        }
    }
}