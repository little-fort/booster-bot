using BoosterBot.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoosterBot
{
    internal class BaseBot : IBoosterBot
    {
        protected readonly string _logPath;
        protected readonly BotConfig _config;
        protected readonly GameUtilities _game;
        protected readonly int _retreatAfterTurn;
        protected Random _rand { get; set; }
        protected Stopwatch _matchTimer { get; set; }

        public BaseBot(GameMode type, double scaling, bool verbose, bool autoplay, bool saveScreens, int retreatAfterTurn, bool downscaled, bool useEvent = false)
        {
            _logPath = $"logs\\{type.ToString().ToLower()}-log-{DateTime.Now.ToString("yyyyMMddHHmmss")}.txt";
            _config = new BotConfig(scaling, verbose, autoplay, saveScreens, _logPath, useEvent);
            _retreatAfterTurn = retreatAfterTurn;
            _game = new GameUtilities(_config);
            _rand = new Random();

            // If the bot is running on a lower resolution, we need to adjust the confidence level to account for slight variations in image quality
            if (downscaled)
                _game.SetDefaultConfidence(0.9);

            PrintShortcutInfo();
        }

        public virtual void Run() { }

        public string GetLogPath() => _logPath;

        protected static void PrintShortcutInfo()
        {
            Console.WriteLine("Keyboard Shortcuts:");
            Console.WriteLine("   [Ctrl+Alt+P] Pause/resume bot");
            Console.WriteLine("   [Ctrl+Alt+Q] Quit bot");
            Console.WriteLine();
        }

        protected void Log(List<string> messages, bool verboseOnly = false)
        {
            foreach (var message in messages)
                Log(message, verboseOnly);

            CheckForPause();
        }

        protected void Log(string message, bool verboseOnly = false)
        {
            if (!verboseOnly || _config.Verbose)
                Logger.Log(message, _logPath);

            CheckForPause();
        }

        protected void LogCountdown(int seconds, string preface = "Continuing in", bool verboseOnly = false)
        {
            for (int i = seconds; i > 0; i--)
            {
                Logger.Log($"{preface} {i} second{(i == 1 ? "" : "s")}...", _logPath);
                Thread.Sleep(1000);
            }
        }

        protected bool Check(Func<IdentificationResult> funcCheck)
        {
            var result = funcCheck();
            Log(result.Logs, true);
            return result.IsMatch;
        }

        protected void CheckForPause()
        {
            var paused = HotkeyManager.IsPaused;
            if (paused)
                Logger.Log("Bot is paused. Press Ctrl+Alt+P to resume...", _logPath);

            while (HotkeyManager.IsPaused)
                Thread.Sleep(100);

            if (paused)
                Logger.Log("Bot is resuming...", _logPath);
        }
    }
}
