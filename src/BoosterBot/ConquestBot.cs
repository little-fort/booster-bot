using BoosterBot.Models;
using System.Diagnostics;

namespace BoosterBot
{
    internal class ConquestBot : IBoosterBot
    {
        private readonly string _logPath;
        private readonly BotConfig _config;
        private readonly GameUtilities _game;
        private readonly GameState _maxTier;
        private Stopwatch _matchTimer { get; set; }

        public ConquestBot(double scaling, bool verbose, bool autoplay, bool saveScreens, GameState maxTier)
        {
            _logPath = $"logs\\conquest-log-{DateTime.Now.ToString("yyyyMMddHHmmss")}.txt";
            _config = new BotConfig(scaling, verbose, autoplay, saveScreens, _logPath);
            _game = new GameUtilities(_config);
            _maxTier = maxTier;

            // Debug();
        }

        public void Debug()
        {
            Console.WriteLine("************** DEBUG MODE **************\n");
            while (true)
            {
                try
                {
                    Console.Clear();
                    Console.WriteLine(DateTime.Now);
                    _config.GetWindowPositions();
                    _game.LogConquestGameState();
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
            Logger.Log("Starting Conquest bot...", _logPath);
            var attempts = 0;

            while (true)
            {
                attempts++;

                _config.GetWindowPositions();
                _game.ResetClick();
                _game.ResetMenu();

                var onMenu = _game.CanIdentifyMainMenu();

                if (onMenu)
                {
                    Logger.Log("Detected main menu. Navigating to Conquest...", _logPath);
                    NavigateToGameModes();
                    NavigateToConquestMenu();
                    RunMatchLoop();
                }
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

        private void NavigateToGameModes()
        {
            Logger.Log("Navigating to Game Modes tab...", _logPath);
            SystemUtilities.Click(_config.GameModesPoint);
            Thread.Sleep(1000);
            SystemUtilities.Click(_config.GameModesPoint);
            Thread.Sleep(2500);
        }

        private void NavigateToConquestMenu()
        {
            Logger.Log("Navigating to Conquest menu...", _logPath);

            for (int x = 0; x < 3; x++)
            {
                SystemUtilities.Click(_config.ConquestBannerPoint);
                Thread.Sleep(1000);
            }
        }

        private bool DetermineLoopEntryPoint(int attempts = 0)
        {
            Logger.Log("Attempting to determine loop entry point...", _logPath);
            var state = _game.DetermineConquestGameState();

            switch (state)
            {
                case GameState.MAIN_MENU:
                    Logger.Log("Detected main menu. Returning to start...", _logPath);
                    Run();
                    return true;
                case GameState.MID_MATCH:
                    Logger.Log("Detected mid-match. Resuming match play...", _logPath);
                    return PlayMatch();
                case GameState.CONQUEST_LOBBY_PG:
                    Logger.Log($"Detected Conquest lobby selection. Entering lobby ({_maxTier.ToString().Replace("CONQUEST_LOBBY_", "")} or lower)...", _logPath);
                    return SelectLobby();
                case GameState.CONQUEST_PREMATCH:
                    Logger.Log("Detected Conquest prematch. Starting match...", _logPath);
                    return StartMatch();
                case GameState.CONQUEST_MATCHMAKING:
                    Logger.Log("Detected matchmaking...", _logPath);
                    return WaitForMatchmaking();
                case GameState.CONQUEST_MATCH:
                    Logger.Log("Detected Conquest match. Playing match...", _logPath);
                    return PlayMatch();
                case GameState.CONQUEST_ROUND_END:
                    Logger.Log("Detected Conquest round end. Moving to next round...", _logPath);
                    return ProgressRound();
                case GameState.CONQUEST_MATCH_END:
                case GameState.CONQUEST_MATCH_END_REWARDS:
                    Logger.Log("Detected Conquest match end. Returning to Conquest menu...", _logPath);
                    return ExitMatch();
                case GameState.CONQUEST_POSTMATCH_LOSS_SCREEN:
                case GameState.CONQUEST_POSTMATCH_WIN_CONTINUE:
                case GameState.CONQUEST_POSTMATCH_WIN_TICKET:
                    Logger.Log("Detected Conquest postmatch screen. Returning to Conquest menu...", _logPath);
                    return AcceptResult();
                default:
                    if (attempts < 3)
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

        private void RunMatchLoop()
        {
            Logger.Log("Starting match loop...", _logPath);
            while (true)
            {
                if (!SelectLobby())
                {
                    DetermineLoopEntryPoint();
                    return;
                }

                var success = StartMatch();

                if (!success)
                    success = DetermineLoopEntryPoint();

                if (!success)
                    _game.BlindReset();
            }
        }

        private bool SelectLobby()
        {
            Thread.Sleep(5000);
            var lobbyConfirmed = false;

            Logger.Log("Making sure lobby type is set to maximum tier available...", _logPath);
            for (int x = 0; x < 6 && !lobbyConfirmed; x++)
            {
                var selectedTier = _game.DetermineConquestLobbyTier();
                Logger.Log($"Selected tier: {selectedTier}", _config.LogPath);
                if ((selectedTier <= _maxTier && !_game.CanIdentifyConquestNoTickets()) || selectedTier == GameState.CONQUEST_LOBBY_PG)
                    lobbyConfirmed = true;
                else
                {
                    _config.GetWindowPositions();
                    var vertCenter = (_config.Window.Bottom - _config.Window.Top) / 2;
                    SystemUtilities.Drag(
                        startX: _config.Window.Left + _config.Center - _config.Scale(250),
                        startY: _config.Window.Top + vertCenter,
                        endX: _config.Window.Left + _config.Center + _config.Scale(250),
                        endY: _config.Window.Top + vertCenter
                    );
                    Thread.Sleep(3500);
                }
            }

            return lobbyConfirmed;
        }

        private bool StartMatch()
        {
            Logger.Log("Entering lobby...", _logPath);
            _game.ClickPlay();
            Thread.Sleep(5000);

            Logger.Log("Clicking 'Play'...", _logPath);
            _game.ClickPlay();
            Thread.Sleep(1000);
            _game.ClickPlay(); // Press a second time just to be sure
            Thread.Sleep(1000);

            Logger.Log("Confirming deck...", _logPath);
            SystemUtilities.Click(_config.Window.Left + _config.Center + _config.Scale(100), _config.Window.Bottom - _config.Scale(345));
            Thread.Sleep(2000);

            return WaitForMatchmaking();
        }

        private bool WaitForMatchmaking()
        {
            _config.GetWindowPositions();
            while (_game.CanIdentifyConquestMatchmaking())
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
            Thread.Sleep(1000);
            var active = true;
            _game.ClickSnap();

            _matchTimer = new Stopwatch();
            _matchTimer.Start();

            while (active && _matchTimer.Elapsed.Minutes < 30)
            {
                _config.GetWindowPositions();
                if (!_game.CanIdentifyActiveConquestMatch())
                {
                    var check = false;
                    for (int x = 1; x < 3 && !check; x++)
                    {
                        Logger.Log("Could not detect active match, trying again in 4 seconds...", _logPath);
                        _config.GetWindowPositions();
                        _game.ResetClick();
                        check = _game.CanIdentifyActiveConquestMatch();
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
            }

            _config.GetWindowPositions();
            if (_matchTimer.Elapsed.Minutes > 15 && _game.CanIdentifyConquestRetreatBtn())
            {
                Logger.Log("Match timer has eclipsed 30 minutes. Attempting retreat...", _logPath);
                _game.ClickRetreat();
                Thread.Sleep(5000);
            }

            if (_game.CanIdentifyConquestConcede())
                return ProgressRound();
            else if (_game.CanIdentifyConquestMatchEnd())
                return ExitMatch();

            return false;
        }

        private bool ProgressRound()
        {
            Logger.Log("Identified round end. Proceeding to next round...", _logPath);
            _game.ClickNext();

            _config.GetWindowPositions();
            var waitTime = 0;
            while (!_game.CanIdentifyActiveConquestMatch() && !_game.CanIdentifyConquestMatchEnd())
            {
                if (waitTime >= 90000)
                {
                    Logger.Log("Max wait time of 90 seconds elapsed...", _logPath);
                    _game.BlindReset();
                    return DetermineLoopEntryPoint();
                }

                Thread.Sleep(3000);
                waitTime += 3000;
                _config.GetWindowPositions();
            }

            return PlayMatch();
        }

        private bool ExitMatch()
        {
            Logger.Log("Exiting match...", _logPath);
            _config.GetWindowPositions();

            while (_game.CanIdentifyConquestMatchEndNext1() || _game.CanIdentifyConquestMatchEndNext2())
            {
                _game.ClickNext();
                Thread.Sleep(4000);
                _config.GetWindowPositions();
            }

            Logger.Log("Waiting for post-match screens...", _logPath);
            while (!_game.CanIdentifyConquestLossContinue() && !_game.CanIdentifyConquestWinNext() && !_game.CanIdentifyConquestPlayBtn())
            {
                Thread.Sleep(2000);
                _config.GetWindowPositions();
            }

            return AcceptResult();
        }

        private bool AcceptResult()
        {
            Logger.Log("Processing post-match screens...", _logPath);

            _config.GetWindowPositions();
            if (_game.CanIdentifyConquestLossContinue() || _game.CanIdentifyConquestWinNext() || _game.CanIdentifyConquestTicketClaim())
            {
                _game.ClickPlay();
                Thread.Sleep(5000);
                _config.GetWindowPositions();
                return AcceptResult();
            }

            return true;
        }
    }
}
