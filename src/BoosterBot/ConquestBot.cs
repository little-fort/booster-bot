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
        private readonly int _retreatAfterTurn;
        private Stopwatch _matchTimer { get; set; }

        public ConquestBot(double scaling, bool verbose, bool autoplay, bool saveScreens, GameState maxTier, int retreatAfterTurn, bool downscaled, bool useEvent = false)
        {
            _logPath = $"logs\\conquest-log-{DateTime.Now.ToString("yyyyMMddHHmmss")}.txt";
            _config = new BotConfig(scaling, verbose, autoplay, saveScreens, _logPath, useEvent);
            _game = new GameUtilities(_config);
            _maxTier = maxTier;
            _retreatAfterTurn = retreatAfterTurn;

            if (downscaled)
                _game.SetDefaultConfidence(0.9);

            // Debug();
        }

        public void Debug()
        {
            Console.WriteLine("************** DEBUG MODE **************\n");
            while (true)
            {
                try
                {
                    _config.GetWindowPositions();

                    var print = _game.LogConquestGameState();
                    Console.Clear();
                    Console.WriteLine(DateTime.Now);

                    foreach (var line in print.Logs) Console.WriteLine(line);
                    foreach (var line in print.Results) Console.WriteLine(line);

                    Console.WriteLine();

                    for (int i = 4; i >= 0; i--)
                        for (int x = 9; x >= 0; x--)
                        {
                            var text = $"Re-scanning window contents in {i}.{x} seconds...";

                            // Move cursor to the beginning of the last line
                            Console.SetCursorPosition(0, Console.CursorTop);

                            // Write the new content (and clear the rest of the line if the new content is shorter)
                            Console.Write(text + new string(' ', Console.WindowWidth - text.Length - 1));

                            Thread.Sleep(100);
                        }

                    var txt = $"Re-scanning window contents...";
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write(txt + new string(' ', Console.WindowWidth - txt.Length - 1));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: {ex.Message}");
                    Thread.Sleep(5000);
                }
            }
        }

        public string GetLogPath() => _logPath;

        public void Log(List<string> messages, bool verboseOnly = false)
        {
            foreach (var message in messages)
                Log(message, verboseOnly);
        }

        public void Log(string message, bool verboseOnly = false)
        {
            if (!verboseOnly || _config.Verbose)
                Logger.Log(message, _logPath);
        }

        private bool Check(Func<IdentificationResult> funcCheck)
        {
            var result = funcCheck();
            Log(result.Logs, true);
            return result.IsMatch;
        }

        public void Run()
        {
            Log("Starting Conquest bot...");
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
                var onMenu = Check(_game.CanIdentifyMainMenu);
                if (onMenu)
                {
                    Log("Detected main menu. Navigating to Conquest...");
                    NavigateToGameModes();
                    NavigateToConquestMenu();
                    RunMatchLoop();
                }
                else
                {
                    if (attempts <= 2)
                    {
                        Log($"Could not detect main menu (attempt #{attempts}). Trying again in 5 seconds...");
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
            Log("Navigating to Game Modes tab...");
            SystemUtilities.Click(_config.GameModesPoint);
            Thread.Sleep(1000);
            SystemUtilities.Click(_config.GameModesPoint);
            Thread.Sleep(2500);
        }

        private void NavigateToConquestMenu()
        {
            Log("Navigating to Conquest menu...");

            for (int x = 0; x < 3; x++)
            {
                SystemUtilities.Click(_config.ConquestBannerPoint);
                Thread.Sleep(1000);
            }
        }

        private bool DetermineLoopEntryPoint(int attempts = 0)
        {
            Log("Attempting to determine loop entry point...");
            var state = _game.DetermineConquestGameState();

            switch (state)
            {
                case GameState.MAIN_MENU:
                    Log("Detected main menu. Returning to start...");
                    Run();
                    return true;
                case GameState.RECONNECT_TO_GAME:
                    Log("Detected 'Reconnect to Game' button. Resuming match play...");
                    _game.ClickPlay();
                    Thread.Sleep(4000);
                    return PlayMatch();
                case GameState.MID_MATCH:
                    Log("Detected mid-match. Resuming match play...");
                    return PlayMatch();
                case GameState.CONQUEST_LOBBY_PG:
                case GameState.CONQUEST_LOBBY_SILVER:
                case GameState.CONQUEST_LOBBY_GOLD:
                case GameState.CONQUEST_LOBBY_INFINITE:
                    Log($"Detected Conquest lobby selection. Entering lobby ({_maxTier.ToString().Replace("CONQUEST_LOBBY_", "")} or lower)...");
                    return SelectLobby();
                case GameState.CONQUEST_PREMATCH:
                    Log("Detected Conquest prematch. Starting match...");
                    return StartMatch();
                case GameState.CONQUEST_MATCHMAKING:
                    Log("Detected matchmaking...");
                    return WaitForMatchmaking();
                case GameState.CONQUEST_MATCH:
                    Log("Detected Conquest match. Playing match...");
                    return PlayMatch();
                case GameState.CONQUEST_ROUND_END:
                    Log("Detected Conquest round end. Moving to next round...");
                    return ProgressRound();
                case GameState.CONQUEST_MATCH_END:
                case GameState.CONQUEST_MATCH_END_REWARDS:
                    Log("Detected Conquest match end. Returning to Conquest menu...");
                    return ExitMatch();
                case GameState.CONQUEST_POSTMATCH_LOSS_SCREEN:
                case GameState.CONQUEST_POSTMATCH_WIN_CONTINUE:
                case GameState.CONQUEST_POSTMATCH_WIN_TICKET:
                    Log("Detected Conquest postmatch screen. Returning to Conquest menu...");
                    return AcceptResult();
                default:
                    if (attempts < 5)
                    {
                        _game.BlindReset();
                        return DetermineLoopEntryPoint(attempts + 1);
                    }

                    Log("Bot is hopelessly lost... :/");
                    Log("Return to main menu and restart bot.");
                    Console.WriteLine();
                    Log("Press any key to exit...");
                    Console.ReadKey();
                    Environment.Exit(0);
                    return false;

            }
        }

        private void RunMatchLoop()
        {
            Log("Starting match loop...");
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

            Log($"Making sure lobby type is set to {_maxTier} or lower...");
            for (int x = 0; x < 6 && !lobbyConfirmed; x++)
            {
                var selectedTier = _game.DetermineConquestLobbyTier();
                Log($"Selected tier: {selectedTier}");
                Log("Checking tickets...", true);
                if ((selectedTier <= _maxTier && !Check(_game.CanIdentifyConquestNoTickets)) || selectedTier == GameState.CONQUEST_LOBBY_PG)
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
            Log("Entering lobby...");
            _game.ClickPlay();
            Thread.Sleep(5000);

            Log("Clicking 'Play'...");
            _game.ClickPlay();
            Thread.Sleep(1000);
            _game.ClickPlay(); // Press a second time just to be sure
            Thread.Sleep(1000);

            Log("Confirming deck...");
            SystemUtilities.Click(_config.Window.Left + _config.Center + _config.Scale(100), _config.Window.Bottom - _config.Scale(345));
            Thread.Sleep(2000);

            return WaitForMatchmaking();
        }

        private bool WaitForMatchmaking()
        {
            _config.GetWindowPositions();

            var mmTimer = new Stopwatch();
            mmTimer.Start();

            Log("Checking for ongoing matchmaking...", true);
            while (Check(_game.CanIdentifyConquestMatchmaking))
            {
                if (mmTimer.Elapsed.TotalSeconds > 600)
                {
                    Log("Matchmaking seems to be hanging. Returning to menu to re-try...");
                    _game.ClickCancel();
                    return true;
                }

                Log("Waiting for match start...");
                Thread.Sleep(5000);
                _config.GetWindowPositions();
            }

            return PlayMatch();
        }

        private bool PlayMatch()
        {
            Log("Playing match...");
            Thread.Sleep(1000);
            var active = true;
            _game.ClickSnap();

            _matchTimer = new Stopwatch();
            _matchTimer.Start();

            var currentTurn = 0;

            while (active && _matchTimer.Elapsed.Minutes < 30)
            {
                _config.GetWindowPositions();

                Log("Checking for active Conquest match...", true);
                if (!Check(_game.CanIdentifyActiveConquestMatch))
                {
                    var check = false;
                    for (int x = 1; x < 3 && !check; x++)
                    {
                        Log("Could not detect active match, trying again in 4 seconds...");
                        _config.GetWindowPositions();
                        _game.ResetClick();
                        check = Check(_game.CanIdentifyActiveConquestMatch);
                        Thread.Sleep(4000);
                    }

                    active = check;
                }
                else
                {
                    if (currentTurn++ >= _retreatAfterTurn)
                    {
                        Log("Retreat after turn reached. Attempting retreat...");
                        _game.ClickRetreat();
                        Thread.Sleep(5000);

						Log("Attempting concede...");
						_game.ClickConcede();
						Thread.Sleep(5000);
					}
					else
                    {
                        Log("Attempting to play cards...");
                        _game.PlayHand();
                        Thread.Sleep(1000);

                        Log("Checking for energy state...", true);
                        if (!Check(_game.CanIdentifyZeroEnergy))
                        {
                            Log("Detected leftover energy, will attempt to play cards again...");
                            _game.PlayHand();
                        }

                        Log("Clicking 'Next Turn'...");
                        _game.ClickNext();
                        Thread.Sleep(1000);

                        _config.GetWindowPositions();

                        Log("Checking for turn state...", true);
                        while (Check(_game.CanIdentifyMidTurn))
                        {
                            Log("Waiting for turn to progress...");
                            Thread.Sleep(4000);
                            _config.GetWindowPositions();
                        }
                    }
                }
            }

            _config.GetWindowPositions();

            Log("Checking for retreat buton...", true);
            if (_matchTimer.Elapsed.Minutes > 15 && Check(_game.CanIdentifyLadderRetreatBtn))
            {
                Log("Match timer has eclipsed 30 minutes. Attempting retreat...");
                _game.ClickRetreat();
                Thread.Sleep(5000);
            }

            Log("Checking for concede button...", true);
            if (Check(_game.CanIdentifyConquestConcede))
                return ProgressRound();

            Log("Checking for end of match...", true);
            if (Check(_game.CanIdentifyConquestMatchEnd))
                return ExitMatch();

            return false;
        }

        private bool ProgressRound()
        {
            Log("Identified round end. Proceeding to next round...");
            _game.ClickNext();

            _config.GetWindowPositions();
            var waitTime = 0;
            Log("Checking for match state...", true);
            while (!Check(_game.CanIdentifyActiveConquestMatch) && !Check(_game.CanIdentifyConquestMatchEnd))
            {
                if (waitTime >= 90000)
                {
                    Log("Max wait time of 90 seconds elapsed...");
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
            Log("Exiting match...");
            _config.GetWindowPositions();

            Log("Checking for post-round screens...", true);
            while (Check(_game.CanIdentifyConquestMatchEndNext1) || Check(_game.CanIdentifyConquestMatchEndNext2))
            {
                _game.ClickNext();
                Thread.Sleep(4000);
                _config.GetWindowPositions();
            }

            Log("Waiting for post-match screens...");
            Thread.Sleep(10000);

            var totalSleep = 0;
            Log("Checking for post-round screens...", true);
            while (!Check(_game.CanIdentifyConquestLossContinue) && !Check(_game.CanIdentifyConquestWinNext) && !Check(_game.CanIdentifyConquestPlayBtn))
            {
                Thread.Sleep(2000);
                totalSleep += 2000;
                _config.GetWindowPositions();

                Log("Checking for any Conquest lobby...", true);
                if (totalSleep > 4000 && Check(_game.CanIdentifyAnyConquestLobby))
                {
                    Log("Identified Conquest lobby...");
                    return true;
                }

                if (totalSleep > 60000)
                    return true;
            }

            return AcceptResult();
        }

        private bool AcceptResult()
        {
            Log("Processing post-match screens...");

            _config.GetWindowPositions();
            Log("Checking for win, loss, or ticket claim screens...");
            if (Check(_game.CanIdentifyConquestLossContinue) || Check(_game.CanIdentifyConquestWinNext) || Check(_game.CanIdentifyConquestTicketClaim))
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
