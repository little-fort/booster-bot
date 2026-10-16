using BoosterBot.Models;
using BoosterBot.Resources;
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
        protected Stopwatch _startTime { get; set; }

        public BaseBot(BotConfig config, int retreat)
        {
            _config = config;
            _logPath = config.LogPath;
            _retreatAfterTurn = retreat;
            _game = new GameUtilities(_config);
            _rand = new Random();
            _startTime = new Stopwatch();
            _startTime.Start();

            // If the bot is running on a lower resolution, we need to adjust the confidence level to account for slight variations in image quality
            if (_config.Downscaled)
                _game.SetDefaultConfidence(0.9);

            PrintShortcutInfo();
        }

        public virtual void Run() { }

        public string GetLogPath() => _logPath;

        protected void PrintShortcutInfo()
        {
            Console.WriteLine(Strings.Log_Info_Shortcuts);
            Console.WriteLine("   " + Strings.Log_Info_Shortcuts_Pause);
            Console.WriteLine("   " + Strings.Log_Info_Shortcuts_Quit);
            Console.WriteLine();
        }

        protected void Log(List<string> keys, bool verboseOnly = false)
        {
            foreach (var key in keys)
                Log(key, verboseOnly);

            CheckForPause();
        }

        protected void Log(string key, bool verboseOnly = false, List<FindReplaceValue> replace = null)
        {
            if (!verboseOnly || _config.Verbose)
                Logger.Log(_config.Localizer, key, _logPath, replace: replace);

            CheckForPause();
        }

        protected void LogCountdown(int seconds, string preface = "Continuing in", bool verboseOnly = false)
        {
            for (int i = seconds; i > 0; i--)
            {
                Console.WriteLine($"{preface} {i} second{(i == 1 ? "" : "s")}...", _logPath);
                Thread.Sleep(1000);
            }
        }

        protected bool Check(Func<IdentificationResult> funcCheck)
        {
            var result = funcCheck();
            Log(result.Logs, true);
            return result.IsMatch;
        }

        protected void CheckForTerminate()
        {
            if (_config.TerminateAfter > 0 && _startTime.Elapsed.TotalSeconds >= _config.TerminateAfter)
            {
                Log("Log_TerminatingAfter", replace: [new("%SECONDS%", _config.TerminateAfter.ToString())]);
                Console.WriteLine();
                Log("Menu_PressKeyToExit");
                Thread.Sleep(2000);
                Environment.Exit(0);
            }
        }

        protected void CheckForPause()
        {
            var paused = HotkeyManager.IsPaused;
            if (paused)
                Logger.Log(_config.Localizer, "Log_BotPaused", _logPath);

            while (HotkeyManager.IsPaused)
                Thread.Sleep(100);

            if (paused)
                Logger.Log(_config.Localizer, "Log_BotResuming", _logPath);
        }
    }
}
