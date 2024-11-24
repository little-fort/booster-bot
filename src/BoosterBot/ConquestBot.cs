using BoosterBot.Helpers;
using BoosterBot.Models;
using System.Diagnostics;

namespace BoosterBot
{
    internal class ConquestBot : BaseBot
    {
        private readonly LocalizationManager _localizer;
        private readonly GameState _maxTier;

        public ConquestBot(BotConfig config, int retreat, GameState maxTier) : base(config, retreat)
        {
            _localizer = config.Localizer;
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
                    Console.WriteLine($"{_localizer.GetString("Log_Error")} {ex.Message}");
                    Thread.Sleep(5000);
                }
            }
        }

        public override void Run()
        {
            Log(_localizer.GetString("Conquest_Log_Start"), 9999);
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
                    Log(_localizer.GetString("Conquest_Log_Menu_Main"), 9999);
                    NavigateToGameModes();
                    NavigateToConquestMenu();
                    RunMatchLoop();
                }
                else
                {
                    if (attempts <= 2)
                    {
                        Log(_localizer.GetString("Log_FailedMenuDetection"), 9999);
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
            Log(_localizer.GetString("Conquest_Log_Menu_GameModes"), 9999);
            SystemUtilities.Click(_config.GameModesPoint);
            Thread.Sleep(1000);
            SystemUtilities.Click(_config.GameModesPoint);
            Thread.Sleep(2500);
        }

        private void NavigateToConquestMenu()
        {
            Log(_localizer.GetString("Conquest_Log_Menu"), 9999);

            for (int x = 0; x < 3; x++)
            {
                SystemUtilities.Click(_config.ConquestBannerPoint);
                Thread.Sleep(1000);
            }
        }

        private bool DetermineLoopEntryPoint(int attempts = 0)
        {
            Log(_localizer.GetString("Log_LoopEntryPoint"), 9999);
            var state = _game.DetermineConquestGameState();

            switch (state)
            {
                case GameState.MAIN_MENU:
                    Log(_localizer.GetString("Conquest_Log_DetectedMain"), 9999);
                    Run();
                    return true;
                case GameState.RECONNECT_TO_GAME:
                    Log(_localizer.GetString("Log_DetectedReconnect"), 9999);
                    _game.ClickPlay();
                    Thread.Sleep(4000);
                    return PlayMatch();
                case GameState.MID_MATCH:
                    Log(_localizer.GetString("Log_DetectedActiveMatch"), 9999);
                    return PlayMatch();
                case GameState.CONQUEST_LOBBY_PG:
                case GameState.CONQUEST_LOBBY_SILVER:
                case GameState.CONQUEST_LOBBY_GOLD:
                case GameState.CONQUEST_LOBBY_INFINITE:
                    Log(_localizer.GetString("Conquest_Log_DetectedLobby"), 9999);
                    SelectLobby();
                    return StartMatch();
                case GameState.CONQUEST_PREMATCH:
                    Log(_localizer.GetString("Conquest_Log_DetectedPrematch"), 9999);
                    return StartMatch();
                case GameState.CONQUEST_MATCHMAKING:
                    Log(_localizer.GetString("Log_DetectedMatchmaking"), 9999);
                    return WaitForMatchmaking();
                case GameState.CONQUEST_MATCH:
                    Log(_localizer.GetString("Conquest_Log_DetectedActiveMatch"), 9999);
                    return PlayMatch();
                case GameState.CONQUEST_ROUND_END:
                    Log(_localizer.GetString("Conquest_Log_DetectedRoundEnd"), 9999);
                    return ProgressRound();
                case GameState.CONQUEST_MATCH_END:
                case GameState.CONQUEST_MATCH_END_REWARDS:
                    Log(_localizer.GetString("Conquest_Log_DetectedMatchEnd"), 9999);
                    return ExitMatch();
                case GameState.CONQUEST_POSTMATCH_LOSS_SCREEN:
                case GameState.CONQUEST_POSTMATCH_WIN_CONTINUE:
                case GameState.CONQUEST_POSTMATCH_WIN_TICKET:
                    Log(_localizer.GetString("Conquest_Log_PostMatch"), 9999);
                    return AcceptResult();
                default:
                    if (attempts < 5)
                    {
                        _game.BlindReset();
                        return DetermineLoopEntryPoint(attempts + 1);
                    }

                    Log(_localizer.GetString("Log_LostBot"), 9999);
                    Log(_localizer.GetString("Log_LostBot_Restart"), 9999);
                    Console.WriteLine();
                    Log(_localizer.GetString("Menu_PressKeyToExit"), 9999);
                    Console.ReadKey();
                    Environment.Exit(0);
                    return false;
            }
        }

        private void RunMatchLoop()
        {
            Log(_localizer.GetString("Log_Match_StartingLoop"), 9999);
            var run = true;
            while (run)
            {
                if (!SelectLobby())
                {
                    DetermineLoopEntryPoint();
                    return;
                }

                // There is a bug where the Enter button can disappear. Verify it exists before proceeding. If not, reset the menu and try again.
                Log(_localizer.GetString("Conquest_Log_VerifyEntryButton"), 9999);
                if (Check(_game.CanIdentifyConquestEntranceFee))
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

            Log(_localizer.GetString("Conquest_Log_Menu_LobbyChoice").Replace("%VALUE%", _maxTier.ToString()), 9999);
            for (int x = 0; x < 6 && !lobbyConfirmed; x++)
            {
                var selectedTier = _game.DetermineConquestLobbyTier();
                Log(_localizer.GetString("Conquest_Log_Menu_SelectedTier").Replace("%VALUE%", _maxTier.ToString()), 9999);
                Log(_localizer.GetString("Conquest_Log_Check_Tickets"), 9999, true);
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
            Log(_localizer.GetString("Conquest_Log_EnteringLobby"), 9999);
            _game.ClickPlay();
            Thread.Sleep(5000);

            Log(_localizer.GetString("Log_Match_StartNew"), 9999);
            _game.ClickPlay();
            Thread.Sleep(1000);
            _game.ClickPlay(); // Press a second time just to be sure
            Thread.Sleep(1000);

            Log(_localizer.GetString("Conquest_Log_ConfirmDeck"), 9999);
            SystemUtilities.Click(_config.Window.Left + _config.Center + _config.Scale(100), _config.Window.Bottom - _config.Scale(345));
            Thread.Sleep(2000);

            return WaitForMatchmaking();
        }

        private bool WaitForMatchmaking()
        {
            _config.GetWindowPositions();

            var mmTimer = new Stopwatch();
            mmTimer.Start();

            Log(_localizer.GetString("Log_Check_Matchmaking"), 9999, true);
            while (Check(_game.CanIdentifyConquestMatchmaking))
            {
                if (mmTimer.Elapsed.TotalSeconds > 600)
                {
                    Log(_localizer.GetString("Log_Check_Matchmaking_Hanged"), 9999);
                    _game.ClickCancel();
                    return true;
                }

                Log(_localizer.GetString("Log_Matchmaking_Waiting").Replace("%ELAPSED%", mmTimer.Elapsed.ToString()), 9999);
                Thread.Sleep(5000);
                _config.GetWindowPositions();
            }

            return PlayMatch();
        }

        private bool PlayMatch()
        {
            Log(_localizer.GetString("Log_Match_Playing"), 9999);
            Thread.Sleep(1000);
            var active = true;
            _game.ClickSnap();

            _matchTimer = new Stopwatch();
            _matchTimer.Start();

            var currentTurn = 0;

            while (active && _matchTimer.Elapsed.Minutes < 30)
            {
                _config.GetWindowPositions();

                Log(_localizer.GetString("Log_Check_ActiveMatch"), 9999, true);
                if (!Check(_game.CanIdentifyActiveConquestMatch))
                {
                    var check = false;
                    for (int x = 1; x < 3 && !check; x++)
                    {
                        Log(_localizer.GetString("Log_Check_ActiveMatch_Failed"), 9999);
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
                        Log(_localizer.GetString("Log_Match_ReachedTurnLimit").Replace("%VALUE%", _retreatAfterTurn.ToString()), 9999);
                        _game.ClickRetreat();
                        Thread.Sleep(5000);

						Log(_localizer.GetString("Conquest_Log_Match_Concede"), 9999);
						_game.ClickConcede();
						Thread.Sleep(5000);
					}
					else
                    {
                        Log(_localizer.GetString("Log_Match_PlayingCards").Replace("%VALUE%", currentTurn.ToString()), 9999);
                        _game.PlayHand();
                        Thread.Sleep(1000);

                        Log(_localizer.GetString("Log_Check_EnergyState"), 9999, true);
                        if (!Check(_game.CanIdentifyZeroEnergy))
                        {
                            Log(_localizer.GetString("Log_Match_LeftoverEnergy"), 9999);
                            _game.PlayHand();
                        }

                        Log(_localizer.GetString("Log_Match_EndTurn"), 9999);
                        _game.ClickNext();
                        Thread.Sleep(1000);

                        _config.GetWindowPositions();

                        Log(_localizer.GetString("Log_Check_TurnState"), 9999, true);
                        while (Check(_game.CanIdentifyMidTurn))
                        {
                            Log(_localizer.GetString("Log_Match_WaitingForTurn"), 9999);
                            Thread.Sleep(4000);
                            _config.GetWindowPositions();
                        }
                    }
                }
            }

            _config.GetWindowPositions();

            Log(_localizer.GetString("Log_Check_RetreatButton"), 9999, true);
            if (_matchTimer.Elapsed.Minutes > 15 && Check(_game.CanIdentifyConquestRetreatBtn))
            {
                Log(_localizer.GetString("Conquest_Log_Match_Concede"), 9999);
                _game.ClickRetreat();
                Thread.Sleep(5000);
            }

            Log(_localizer.GetString("Log_Check_Concede"), 9999, true);
            if (Check(_game.CanIdentifyConquestConcede))
                return ProgressRound();

            Log(_localizer.GetString("Log_Check_MatchEnd"), 9999, true);
            if (Check(_game.CanIdentifyConquestMatchEnd))
                return ExitMatch();

            return false;
        }

        private bool ProgressRound()
        {
            Log(_localizer.GetString("Conquest_Log_DetectedRoundEnd"), 9999);
            _game.ClickNext();

            _config.GetWindowPositions();
            var waitTime = 0;
            Log(_localizer.GetString("Log_Check_MatchState"), 9999, true);
            while (!Check(_game.CanIdentifyActiveConquestMatch) && !Check(_game.CanIdentifyConquestMatchEnd))
            {
                if (waitTime >= 90000)
                {
                    Log(_localizer.GetString("Conquest_Log_MaxWaitTimeReached").Replace("%VALUE%", "90"), 9999);
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
            Log(_localizer.GetString("Log_Match_Exiting"), 9999);
            _config.GetWindowPositions();

            Log(_localizer.GetString("Log_Check_PostMatchScreen"), 9999, true);
            while (Check(_game.CanIdentifyConquestMatchEndNext1) || Check(_game.CanIdentifyConquestMatchEndNext2))
            {
                _game.ClickNext();
                Thread.Sleep(4000);
                _config.GetWindowPositions();
            }

            Log(_localizer.GetString("Conquest_Log_Menu_WaitingPostMatch"), 9999);
            Thread.Sleep(15000);

            var totalSleep = 0;
            Log(_localizer.GetString("Log_Check_PostMatchScreen"), 9999, true);
            while (!Check(_game.CanIdentifyConquestLossContinue) && !Check(_game.CanIdentifyConquestWinNext) && !Check(_game.CanIdentifyConquestPlayBtn))
            {
                Thread.Sleep(2000);
                totalSleep += 2000;
                _config.GetWindowPositions();

                Log(_localizer.GetString("Conquest_Log_Check_AnyLobby"), 9999, true);
                if (totalSleep > 4000 && Check(_game.CanIdentifyAnyConquestLobby))
                {
                    Log(_localizer.GetString("Conquest_Log_Menu_DetectedLobby"), 9999);
                    return true;
                }

                if (totalSleep > 60000)
                {
                    Log(_localizer.GetString("Conquest_Log_MaxWaitTimeReached").Replace("%VALUE%", "60"), 9999);
                    return true;
                }
            }

            return AcceptResult();
        }

        private bool AcceptResult()
        {
            Log(_localizer.GetString("Conquest_Log_Match_ProcessingPostMatch"), 9999);

            _config.GetWindowPositions();
            Log(_localizer.GetString("Conquest_Log_Check_Screens"), 9999);
            if (Check(_game.CanIdentifyConquestLossContinue) || Check(_game.CanIdentifyConquestWinNext))
            {
                Log(_localizer.GetString("Log_ClickNext"), 9999);
                _game.ClickPlay();
                Thread.Sleep(5000);
                _config.GetWindowPositions();
                return AcceptResult();
            }
            else if (Check(_game.CanIdentifyConquestTicketClaim))
            {
                Log(_localizer.GetString("Conquest_Log_Match_ProcessingPostMatch"), 9999);
                _game.ClickClaim();
            }

            return true;
        }
    }
}
