using BoosterBot.Models;
using System.Diagnostics;

namespace BoosterBot
{
    internal class LadderBot : IBoosterBot
    {
        private readonly string _logPath;
        private readonly BotConfig _config;
        private readonly GameUtilities _game;
        private Random _rand { get; set; }
        private Stopwatch _matchTimer { get; set; }


        public LadderBot(double scaling, bool verbose, bool autoplay, bool saveScreens)
        {
            _logPath = $"logs\\ladder-log-{DateTime.Now.ToString("yyyyMMddHHmmss")}.txt";
            _config = new BotConfig(scaling, verbose, autoplay, saveScreens, _logPath);
            _game = new GameUtilities(_config);
            _rand = new Random();
        }

        public void Debug()
        {
            while (true)
            {
                try
                {
                    Console.WriteLine("--------------------------------------------------------");
                    Console.WriteLine("--------------------------------------------------------");
                    _game.LogLadderGameState();
                    Thread.Sleep(5000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: {ex.Message}");
                    Thread.Sleep(5000);
                }
            }
        }

        public string GetLogPath() => _logPath;

        public void Run()
        {
            Logger.Log("Starting Ladder bot...", _logPath);
            var attempts = 0;

            while (true)
            {
                attempts++;

                _config.GetWindowPositions();
                _game.ResetClick();
                _game.ResetMenu();
                _game.ResetClick();

                Thread.Sleep(500);

                _config.GetWindowPositions();
                var onMenu = _game.CanIdentifyMainMenu();

                if (onMenu)
                    DetermineLoopEntryPoint();
                else
                {
                    if (attempts <= 2)
                    {
                        Logger.Log($"Could not detect main menu (attempt #{attempts}). Trying again in 5 seconds...", _logPath);
                        _game.ResetClick();
                        Thread.Sleep(5000);
                    }
                    else
                    {
                        attempts = 0;
                        DetermineLoopEntryPoint();
                    }
                }
            }
        }

        private bool DetermineLoopEntryPoint(int attempts = 0)
        {
            Logger.Log("Attempting to determine loop entry point...", _logPath);
            var state = _game.DetermineLadderGameState();

            switch (state)
            {
                case GameState.MAIN_MENU:
                    Logger.Log("Detected main menu. Starting new match...", _logPath);
                    StartMatch();
                    return true;
                case GameState.RECONNECT_TO_GAME:
                    Logger.Log("Detected 'Reconnect to Game' button. Resuming match play...", _logPath);
                    _game.ClickPlay();
                    Thread.Sleep(4000);
                    return PlayMatch();
                case GameState.MID_MATCH:
                    Logger.Log("Detected active match. Resuming match play...", _logPath);
                    return StartMatch();
                case GameState.LADDER_MATCHMAKING:
                    Logger.Log("Detected matchmaking...", _logPath);
                    return WaitForMatchmaking();
                case GameState.LADDER_MATCH:
                    Logger.Log("Detected active match. Resuming match play...", _logPath);
                    return PlayMatch();
                case GameState.LADDER_MATCH_END:
                case GameState.LADDER_MATCH_END_REWARDS:
                    Logger.Log("Detected match end. Returning to main menu...", _logPath);
                    return ExitMatch();
                case GameState.CONQUEST_LOBBY_PG:
                    Logger.Log("Detected Conquest lobby. Resetting menu...", _logPath);
                    _game.ResetMenu();
                    return StartMatch();
                default:
                    if (attempts < 5)
                    {
                        _game.BlindReset();
                        return DetermineLoopEntryPoint(attempts + 1);
                    }

                    Logger.Log("Bot is hopelessly lost... :/", _logPath);
                    Logger.Log("Return to main menu and restart bot.", _logPath);
                    Environment.Exit(0);
                    return false;

            }
        }

        private bool StartMatch()
        {
            Logger.Log("Clicking 'Play'...", _logPath);
            _game.ClickPlay();
            Thread.Sleep(1000);
            _game.ClickPlay(); // Press a second time just to be sure
            Thread.Sleep(3000);

            return WaitForMatchmaking();
        }

        private bool WaitForMatchmaking()
        {
            _config.GetWindowPositions();
            while (_game.CanIdentifyLadderMatchmaking())
            {
                Logger.Log("Waiting for match start...", _logPath);
                Thread.Sleep(5000);
                _config.GetWindowPositions();
            }

            return PlayMatch();
        }

        private bool PlayMatch()
        {
            Logger.Log("Playing match...", _logPath);
            var active = true;
            var rolledSnap = false;
            _rand = new Random();

            _matchTimer = new Stopwatch();
            _matchTimer.Start();

            while (active && _matchTimer.Elapsed.Minutes < 15)
            {
                _config.GetWindowPositions();
                if (!_game.CanIdentifyActiveLadderMatch())
                {
                    var check = false;
                    for (int x = 1; x < 3 && !check; x++)
                    {
                        Logger.Log("Could not detect active match, trying again in 4 seconds...", _logPath);
                        _config.GetWindowPositions();
                        _game.ResetClick();
                        check = _game.CanIdentifyActiveLadderMatch();
                        Thread.Sleep(4000);
                    }

                    active = check;
                }
                else
                {
                    Logger.Log("Attempting to play cards...", _logPath);
                    _game.PlayHand();
                    Thread.Sleep(1000);

                    _config.GetWindowPositions();
                    if (!_game.CanIdentifyZeroEnergy())
                    {
                        Logger.Log("Detected leftover energy, will attempt to play cards again...", _logPath);
                        _game.PlayHand();
                    }

                    Logger.Log("Clicking 'Next Turn'...", _logPath);
                    _game.ClickNext();
                    Thread.Sleep(1000);

                    _config.GetWindowPositions();
                    while (_game.CanIdentifyMidTurn())
                    {
                        Logger.Log("Waiting for turn to progress...", _logPath);
                        Thread.Sleep(4000);
                        _config.GetWindowPositions();
                    }
                }

                if (!rolledSnap && _rand.Next(1, 101) > 45) // Add randomness for snaps
                {
                    rolledSnap = true;
                    _game.ClickSnap();
                }
            }

            _config.GetWindowPositions();
            if (_matchTimer.Elapsed.Minutes > 15 && _game.CanIdentifyLadderRetreatBtn())
            {
                Logger.Log("Match timer has eclipsed 15 minutes. Attempting retreat...", _logPath);
                _game.ClickRetreat();
                Thread.Sleep(5000);
            }

            if (_game.CanIdentifyLadderMatchEnd())
                return ExitMatch();

            return false;
        }

        private bool ExitMatch()
        {
            Logger.Log("Exiting match...", _logPath);
            _config.GetWindowPositions();

            while (_game.CanIdentifyLadderCollectRewardsBtn() || _game.CanIdentifyLadderMatchEndNextBtn())
            {
                _game.ClickNext();
                Thread.Sleep(6000);
                _config.GetWindowPositions();
            }

            return true;
        }
    }

}
