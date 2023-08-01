using BoosterBot.Models;
using System.Diagnostics;

namespace BoosterBot
{
    internal class LadderBot : IBoosterBot
    {
        private readonly string _logPath;
        private readonly BotConfig _config;
        private Random _rand { get; set; }
        private Stopwatch _matchTimer { get; set; }


        public LadderBot(double scaling, bool verbose, bool autoplay, bool saveScreens)
        {
            _logPath = $"logs\\ladder-log-{DateTime.Now.ToString("yyyyMMddHHmmss")}.txt";
            _config = new BotConfig(scaling, verbose, autoplay, saveScreens, _logPath);
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
                    GameUtilities.LogLadderGameState(_config);
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
                GameUtilities.ResetClick(_config);
                GameUtilities.ResetMenu(_config);

                var onMenu = GameUtilities.CanIdentifyMainMenu(_config);

                if (onMenu)
                    DetermineLoopEntryPoint();
                else
                {
                    if (attempts <= 2)
                    {
                        Logger.Log($"Could not detect main menu (attempt #{attempts}). Trying again in 5 seconds...", _logPath);
                        GameUtilities.ResetClick(_config);
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

        private bool DetermineLoopEntryPoint(bool finalAttempt = false)
        {
            Logger.Log("Attempting to determine loop entry point...", _logPath);
            var state = GameUtilities.DetermineLadderGameState(_config);

            switch (state)
            {
                case GameState.MAIN_MENU:
                    Logger.Log("Detected main menu. Starting new match...", _logPath);
                    StartMatch();
                    return true;
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
                    GameUtilities.ResetMenu(_config);
                    return StartMatch();
                default:
                    Logger.Log("Could not find points of reference.", _logPath);
                    if (!finalAttempt)
                    {
                        Logger.Log("Performing blind reset and retrying...", _logPath);
                        GameUtilities.BlindReset(_config);
                        return DetermineLoopEntryPoint(true);
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
            GameUtilities.ClickPlay(_config);
            Thread.Sleep(1000);
            GameUtilities.ClickPlay(_config); // Press a second time just to be sure
            Thread.Sleep(1000);

            return WaitForMatchmaking();
        }

        private bool WaitForMatchmaking()
        {
            _config.GetWindowPositions();
            while (GameUtilities.CanIdentifyLadderMatchmaking(_config))
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
                if (!GameUtilities.CanIdentifyActiveLadderMatch(_config))
                {
                    var check = false;
                    for (int x = 1; x < 3 && !check; x++)
                    {
                        Logger.Log("Could not detect active match, trying again in 4 seconds...", _logPath);
                        _config.GetWindowPositions();
                        GameUtilities.ResetClick(_config);
                        check = GameUtilities.CanIdentifyActiveLadderMatch(_config);
                        Thread.Sleep(4000);
                    }

                    active = check;
                }
                else
                {
                    Logger.Log("Attempting to play cards...", _logPath);
                    GameUtilities.PlayHand(_config);
                    Thread.Sleep(1000);

                    _config.GetWindowPositions();
                    if (!GameUtilities.CanIdentifyZeroEnergy(_config))
                    {
                        Logger.Log("Detected leftover energy, will attempt to play cards again...", _logPath);
                        GameUtilities.PlayHand(_config);
                    }

                    Logger.Log("Clicking 'Next Turn'...", _logPath);
                    GameUtilities.ClickNext(_config);
                    Thread.Sleep(1000);

                    _config.GetWindowPositions();
                    while (GameUtilities.CanIdentifyMidTurn(_config))
                    {
                        Logger.Log("Waiting for turn to progress...", _logPath);
                        Thread.Sleep(4000);
                        _config.GetWindowPositions();
                    }
                }

                if (!rolledSnap && _rand.Next(1, 101) > 45) // Add randomness for snaps
                {
                    rolledSnap = true;
                    GameUtilities.ClickSnap(_config);
                }
            }

            _config.GetWindowPositions();
            if (_matchTimer.Elapsed.Minutes > 15 && GameUtilities.CanIdentifyLadderRetreatBtn(_config))
            {
                Logger.Log("Match timer has eclipsed 15 minutes. Attempting retreat...", _logPath);
                GameUtilities.ClickRetreat(_config);
                Thread.Sleep(5000);
            }

            if (GameUtilities.CanIdentifyLadderMatchEnd(_config))
                return ExitMatch();

            return false;
        }

        private bool ExitMatch()
        {
            Logger.Log("Exiting match...", _logPath);
            _config.GetWindowPositions();

            while (GameUtilities.CanIdentifyLadderCollectRewardsBtn(_config) || GameUtilities.CanIdentifyLadderMatchEndNextBtn(_config))
            {
                GameUtilities.ClickNext(_config);
                Thread.Sleep(4000);
                _config.GetWindowPositions();
            }

            return true;
        }
    }

}
