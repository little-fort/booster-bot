using BoosterBot.Helpers;
using BoosterBot.Models;
using BoosterBot.Resources;
using System.Diagnostics;

namespace BoosterBot
{
    internal class ConquestBot : BaseBot
    {
        private readonly GameState _maxTier;

        public ConquestBot(BotConfig config, int retreat, GameState maxTier) : base(config, retreat)
        {
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
                    Console.WriteLine($"{Strings.Log_Error} {ex.Message}");
                    Thread.Sleep(5000);
                }
            }
        }

        public override void Run()
        {
            Log("Conquest_Log_Start");
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
                    Log("Conquest_Log_Menu_Main");
                    NavigateToGameModes();
                    NavigateToConquestMenu();
                    RunMatchLoop();
                }
                else
                {
                    if (attempts <= 2)
                    {
                        Log("Log_FailedMenuDetection");
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
            Log("Conquest_Log_Menu_GameModes");
            SystemUtilities.Click(_config.GameModesPoint);
            Thread.Sleep(1000);
            SystemUtilities.Click(_config.GameModesPoint);
            Thread.Sleep(2500);
        }

        private void NavigateToConquestMenu()
        {
            Log("Conquest_Log_Menu");

            for (int x = 0; x < 3; x++)
            {
                SystemUtilities.Click(_config.ConquestBannerPoint);
                Thread.Sleep(1000);
            }
        }

        private bool DetermineLoopEntryPoint(int attempts = 0)
        {
            Log("Log_LoopEntryPoint");
            var state = _game.DetermineConquestGameState();

            switch (state)
            {
                case GameState.MAIN_MENU:
                    Log("Conquest_Log_DetectedMain");
                    Run();
                    return true;
                case GameState.RECONNECT_TO_GAME:
                    Log("Log_DetectedReconnect");
                    _game.ClickPlay();
                    Thread.Sleep(4000);
                    return PlayMatch();
                case GameState.MID_MATCH:
                    Log("Log_DetectedActiveMatch");
                    return PlayMatch();
                case GameState.CONQUEST_LOBBY_PG:
                case GameState.CONQUEST_LOBBY_SILVER:
                case GameState.CONQUEST_LOBBY_GOLD:
                case GameState.CONQUEST_LOBBY_INFINITE:
                    Log("Conquest_Log_DetectedLobby");
                    SelectLobby();
                    return StartMatch();
                case GameState.CONQUEST_PREMATCH:
                    Log("Conquest_Log_DetectedPrematch");
                    return StartMatch();
                case GameState.CONQUEST_MATCHMAKING:
                    Log("Log_DetectedMatchmaking");
                    return WaitForMatchmaking();
                case GameState.CONQUEST_MATCH:
                    Log("Conquest_Log_DetectedActiveMatch");
                    return PlayMatch();
                case GameState.CONQUEST_ROUND_END:
                    Log("Conquest_Log_DetectedRoundEnd");
                    return ProgressRound();
                case GameState.CONQUEST_MATCH_END:
                case GameState.CONQUEST_MATCH_END_REWARDS:
                    Log("Conquest_Log_DetectedMatchEnd");
                    return ExitMatch();
                case GameState.CONQUEST_POSTMATCH_LOSS_SCREEN:
                case GameState.CONQUEST_POSTMATCH_WIN_CONTINUE:
                case GameState.CONQUEST_POSTMATCH_WIN_TICKET:
                    Log("Conquest_Log_PostMatch");
                    return AcceptResult();
                default:
                    if (attempts < 5)
                    {
                        _game.BlindReset();
                        return DetermineLoopEntryPoint(attempts + 1);
                    }

                    Log("Log_LostBot");
                    Log("Log_LostBot_Restart");
                    Console.WriteLine();
                    Log("Menu_PressKeyToExit");
                    Console.ReadKey();
                    Environment.Exit(0);
                    return false;
            }
        }

        private void RunMatchLoop()
        {
            Log("Log_Match_StartingLoop");
            var run = true;
            while (run)
            {
                if (!SelectLobby())
                {
                    DetermineLoopEntryPoint();
                    return;
                }

                // There is a bug where the Enter button can disappear. Verify it exists before proceeding. If not, reset the menu and try again.
                Log("Conquest_Log_VerifyEntryButton");
                if (Check(_game.CanIdentifyConquestPlayBtn) || Check(() => _game.CanIdentifyConquestEntranceFee()))
                {
                    var success = StartMatch();

                    if (!success)
                        success = DetermineLoopEntryPoint();

                    if (!success)
                        _game.BlindReset();
                }
                else
                {
                    _game.BlindReset();
                    run = false;
                }
            }
        }

        private bool SelectLobby()
        {
            Thread.Sleep(5000);
            var lobbyConfirmed = false;

            // Start by scrolling all the way to the right to avoid UI bug that shifts detection points
            Log("Conquest_Log_LobbyReset");
            for (int x = 0; x < 5; x++)
            {
                var vertCenter = (_config.Window.Bottom - _config.Window.Top) / 2;
                SystemUtilities.Drag(
                    startX: _config.Window.Left + _config.Center + _config.Scale(250),
                    startY: _config.Window.Top + vertCenter,
                    endX: _config.Window.Left + _config.Center - _config.Scale(250),
                    endY: _config.Window.Top + vertCenter
                );
                Thread.Sleep(2000);
            }

            Log("Conquest_Log_Menu_LobbyChoice", replace: [new("%VALUE%", _maxTier.ToString())]);
            for (int x = 0; x < 6 && !lobbyConfirmed; x++)
            {
                var selectedTier = _game.DetermineConquestLobbyTier();
                Log("Conquest_Log_Menu_SelectedTier", replace: [new("%VALUE%", selectedTier.ToString())]);
                Log("Conquest_Log_Check_Tickets", true);
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
            Log("Conquest_Log_EnteringLobby");
            _game.ClickPlay();
            Thread.Sleep(5000);

            Log("Log_Match_StartNew");
            _game.ClickPlay();
            Thread.Sleep(1000);
            _game.ClickPlay(); // Press a second time just to be sure
            Thread.Sleep(1000);

            Log("Conquest_Log_ConfirmDeck");
            SystemUtilities.Click(_config.Window.Left + _config.Center + _config.Scale(100), _config.Window.Bottom - _config.Scale(345));
            Thread.Sleep(2000);

            return WaitForMatchmaking();
        }

        private bool WaitForMatchmaking()
        {
            _config.GetWindowPositions();

            var mmTimer = new Stopwatch();
            mmTimer.Start();

            Log("Log_Check_Matchmaking", true);
            while (Check(() => _game.CanIdentifyConquestMatchmaking()))
            {
                if (mmTimer.Elapsed.TotalSeconds > 600)
                {
                    Log("Log_Check_Matchmaking_Hanged");
                    _game.ClickCancel();
                    return true;
                }

                Log("Log_Matchmaking_Waiting", replace: [new("%ELAPSED%", mmTimer.Elapsed.ToString())]);
                Thread.Sleep(5000);
                _config.GetWindowPositions();
            }

            return PlayMatch();
        }

        private bool PlayMatch()
        {
            Log("Log_Match_Playing");
            Thread.Sleep(2000);
            var active = true;
            _game.ClickSnap();

            _matchTimer = new Stopwatch();
            _matchTimer.Start();

            var currentTurn = 0;

            while (active && _matchTimer.Elapsed.Minutes < 30)
            {
                _config.GetWindowPositions();

                Log("Log_Check_ActiveMatch", true);
                if (!Check(_game.CanIdentifyActiveConquestMatch))
                {
                    var check = false;
                    for (int x = 1; x < 3 && !check; x++)
                    {
                        Log("Log_Check_ActiveMatch_Failed");
                        _config.GetWindowPositions();
                        _game.ResetClick();
                        check = Check(_game.CanIdentifyActiveConquestMatch);
                        Thread.Sleep(2500);
                    }

                    active = check;
                }
                else
                {
                    if (currentTurn++ >= _retreatAfterTurn)
                    {
                        Log("Log_Match_ReachedTurnLimit", replace: [new("%VALUE%", _retreatAfterTurn.ToString())]);
                        _game.ClickRetreat();
                        Thread.Sleep(5000);

						Log("Conquest_Log_Match_Concede");
						_game.ClickConcede();
						Thread.Sleep(5000);
					}
					else
                    {
                        Log("Log_Match_PlayingCards", replace: [new("%VALUE%", currentTurn.ToString())]);
                        _game.PlayHand();
                        Thread.Sleep(1000);

                        Log("Log_Check_EnergyState", true);
                        if (!Check(_game.CanIdentifyZeroEnergy))
                        {
                            Log("Log_Match_LeftoverEnergy");
                            _game.PlayHand();
                        }

                        Log("Log_Match_EndTurn");
                        _game.ClickNext();
                        Thread.Sleep(1000);

                        _config.GetWindowPositions();

                        Log("Log_Check_TurnState", true);
                        while (Check(() => _game.CanIdentifyMidTurn()))
                        {
                            Log("Log_Match_WaitingForTurn");
                            Thread.Sleep(4000);
                            _config.GetWindowPositions();
                        }
                    }
                }
            }

            _config.GetWindowPositions();

            Log("Log_Check_RetreatButton", true);
            if (_matchTimer.Elapsed.Minutes > 15 && Check(() => _game.CanIdentifyConquestRetreatBtn()))
            {
                Log("Conquest_Log_Match_Concede");
                _game.ClickRetreat();
                Thread.Sleep(5000);
            }

            Log("Log_Check_Concede", true);
            if (Check(() => _game.CanIdentifyConquestConcede()))
                return ProgressRound();

            Log("Log_Check_MatchEnd", true);
            if (Check(() => _game.CanIdentifyConquestMatchEnd()))
                return ExitMatch();

            return false;
        }

        private bool ProgressRound()
        {
            Log("Conquest_Log_DetectedRoundEnd");
            _game.ClickNext();

            _config.GetWindowPositions();
            var waitTime = 0;
            Log("Log_Check_MatchState", true);
            while (!Check(_game.CanIdentifyActiveConquestMatch) && !Check(() => _game.CanIdentifyConquestMatchEnd()))
            {
                if (waitTime >= 90000)
                {
                    Log("Conquest_Log_MaxWaitTimeReached", replace: [new("%VALUE%", "90")]);
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
            Log("Log_Match_Exiting");
            _config.GetWindowPositions();

            Log("Log_Check_PostMatchScreen", true);
            while (Check(() => _game.CanIdentifyConquestMatchEndNext1()) || Check(() => _game.CanIdentifyConquestMatchEndNext2()))
            {
                _game.ClickNext();
                Thread.Sleep(4000);
                _config.GetWindowPositions();
            }

            Log("Conquest_Log_Menu_WaitingPostMatch");
            Thread.Sleep(15000);

            var totalSleep = 0;
            Log("Log_Check_PostMatchScreen", true);
            while (!Check(_game.CanIdentifyConquestLossContinue) && !Check(_game.CanIdentifyConquestWinNext) && !Check(_game.CanIdentifyConquestPlayBtn))
            {
                Thread.Sleep(2000);
                totalSleep += 2000;
                _config.GetWindowPositions();

                Log("Conquest_Log_Check_AnyLobby", true);
                if (totalSleep > 4000 && Check(_game.CanIdentifyAnyConquestLobby))
                {
                    Log("Conquest_Log_Menu_DetectedLobby");
                    return true;
                }

                if (totalSleep > 60000)
                {
                    Log("Conquest_Log_MaxWaitTimeReached", replace: [new("%VALUE%", "60")]);
                    return DetermineLoopEntryPoint();
                }
            }

            return AcceptResult();
        }

        private bool AcceptResult()
        {
            Log("Conquest_Log_Match_ProcessingPostMatch");

            _config.GetWindowPositions();
            Log("Conquest_Log_Check_Screens");
            if (Check(_game.CanIdentifyConquestLossContinue) || Check(_game.CanIdentifyConquestWinNext))
            {
                Log("Log_ClickNext");
                _game.ClickPlay();
                Thread.Sleep(5000);
                _config.GetWindowPositions();
                return AcceptResult();
            }
            else if (Check(() => _game.CanIdentifyConquestTicketClaim()))
            {
                Log("Conquest_Log_Match_ProcessingPostMatch");
                _game.ClickClaim();
            }

            return true;
        }
    }
}
