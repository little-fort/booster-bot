using BoosterBot.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BoosterBot
{
    internal abstract class BaseBot : IBoosterBot, IDisposable
    {
        // 添加接口要求的属性
        public void Dispose()
        {
            CleanupResources();
            GC.SuppressFinalize(this);
        }
        public bool IsStopped => _isStopped;
        protected readonly string _logPath;
        protected readonly BotConfig _config;
        protected readonly GameUtilities _game;
        protected readonly int _retreatAfterTurn;
        protected CancellationTokenSource _cts = new();
        protected bool _isPaused;
        protected bool _isStopped;
        protected Random _rand { get; set; }
        protected Stopwatch _matchTimer { get; set; }

        public BaseBot(BotConfig config, int retreat)
        {
            _config = config;
            _logPath = config.LogPath;
            _retreatAfterTurn = retreat;
            _game = new GameUtilities(_config);
            _rand = new Random();
            _matchTimer = new Stopwatch();

            if (_config.Downscaled)
                _game.SetDefaultConfidence(0.9);

            PrintShortcutInfo();
        }
        protected async Task SafeDelay(int milliseconds, CancellationToken token)
        {
            try
            {
                var delayTask = Task.Delay(milliseconds, token);
                var stopCheck = Task.Run(async () =>
                {
                    while (!_isStopped) await Task.Delay(10);
                });

                await Task.WhenAny(delayTask, stopCheck);
            }
            catch (TaskCanceledException)
            {
                // 正常取消处理
            }
        }
        public async Task RunAsync(CancellationToken externalToken)
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                externalToken,
                _cts.Token
            );

            try
            {
                while (!linkedCts.IsCancellationRequested && !_isStopped)
                {
                    await CheckPauseStateAsync(linkedCts.Token);
                    if (linkedCts.IsCancellationRequested || _isStopped)
                        break;

                    await ExecuteCycleAsync(linkedCts.Token);
                    await SafeDelay(500, linkedCts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // 处理取消
            }
            finally
            {
                CleanupResources();
            }
        }
        private void CleanupResources()
        {
            try
            {
                _cts?.Dispose();
                _matchTimer?.Stop();  

                _isStopped = true;
                _isPaused = false;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Cleanup error: {ex.Message}");
            }
        }
        private async Task CheckStopStateAsync()
        {
            while (_isStopped)
            {
                await Task.Delay(100); // 异步等待退出
                break;
            }
        }
        protected abstract Task ExecuteCycleAsync(CancellationToken token);
        public virtual void Run() { }
        public void Pause() => _isPaused = true;
        public void Resume() => _isPaused = false;
        public async Task CheckPauseStateAsync(CancellationToken token)
        {
            while (_isPaused && !token.IsCancellationRequested)
            {
                await Task.Delay(250, token);

            }
        }

        public void Cancel() => _cts.Cancel();
        public string GetLogPath() => _logPath;
        protected void PrintShortcutInfo()
        {

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

        protected void CheckForPause()
        {
            while (HotkeyManager.IsPaused && !_isPaused && !_isStopped)
            {
                Logger.Log(_config.Localizer, "Log_BotPausing", _logPath);
                Thread.Sleep(100);
            }
            if (!HotkeyManager.IsPaused && _isPaused)
            {
                Logger.Log(_config.Localizer, "Log_BotResuming", _logPath);
                _isPaused = false;
            }
        }
        public void Stop()
        {
            _isStopped = true;
            _cts.Cancel();

            // 强制释放可能被阻塞的资源
            _game?.Dispose();
        }
        protected void CheckForStop()
        {
            if (HotkeyManager.IsStopped)
            {
                _isStopped = true;
                _cts.Cancel();
            }
        }
    }
}