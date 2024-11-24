using BoosterBot.Helpers;
using BoosterBot.Models;
using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace BoosterBot
{
    internal class EventBot : BaseBot
    {
        private readonly LocalizationManager _localizer;

        public EventBot(BotConfig config, int retreat) : base(config, retreat)
        {
            _localizer = config.Localizer;
            // Debug();
        }

        public void Debug()
        {
            while (true)
            {
                try
                {
                    var print = _game.LogLadderGameState();
                    foreach (var line in print.Logs) Console.WriteLine(line);
                    foreach (var line in print.Results) Console.WriteLine(line);
                    Console.WriteLine("--------------------------------------------------------");
                    Console.WriteLine("--------------------------------------------------------");

                    Thread.Sleep(5000);
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
            Log(_localizer.GetString("Event_Log_Start"), 9999);
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
                    Log(_localizer.GetString("Event_Log_Menu"), 9999);
                    NavigateToGameModes();
                    NavigateToLtmMenu();
                    StartMatch();
                }
                else if (Check(_game.CanIdentifyEventMenu))
                    StartMatch();
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

        private void NavigateToLtmMenu()
        {
            Log(_localizer.GetString("Event_Log_Menu_ModeTab"), 9999);

            for (int x = 0; x < 3; x++)
            {
                SystemUtilities.Click(_config.EventBannerPoint);
                Thread.Sleep(1000);
            }
        }

        private bool DetermineLoopEntryPoint(int attempts = 0)
        {
            Log(_localizer.GetString("Log_LoopEntryPoint"), 9999);
            var state = _game.DetermineLadderGameState();

            switch (state)
            {
                case GameState.MAIN_MENU:
                    Log(_localizer.GetString("Event_Log_Menu"), 9999);
                    NavigateToGameModes();
                    NavigateToLtmMenu();
                    Log(_localizer.GetString("Log_Match_StartNew"), 9999);
                    StartMatch();
                    return true;
                case GameState.EVENT_MENU:
                    Log(_localizer.GetString("Event_Log_DetectedEventMenu"), 9999);
                    StartMatch();
                    return true;
                case GameState.RECONNECT_TO_GAME:
                    Log(_localizer.GetString("Log_DetectedReconnect"), 9999);
                    _game.ClickPlay();
                    Thread.Sleep(4000);
                    return PlayMatch();
                case GameState.MID_MATCH:
                    Log(_localizer.GetString("Log_DetectedActiveMatch"), 9999);
                    return StartMatch();
                case GameState.LADDER_MATCHMAKING:
                    Log(_localizer.GetString("Log_DetectedMatchmaking"), 9999);
                    return WaitForMatchmaking();
                case GameState.LADDER_MATCH:
                    Log(_localizer.GetString("Log_DetectedActiveMatch"), 9999);
                    return PlayMatch();
                case GameState.LADDER_MATCH_END:
                case GameState.LADDER_MATCH_END_REWARDS:
                    Log(_localizer.GetString("Log_DetectedMatchEnd"), 9999);
                    return ExitMatch();
                case GameState.CONQUEST_LOBBY_PG:
                    Log(_localizer.GetString("Log_Ladder_DetectedConquest"), 9999);
                    _game.ResetMenu();
                    NavigateToGameModes();
                    NavigateToLtmMenu();
                    return StartMatch();
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

        private bool StartMatch()
        {
            Log(_localizer.GetString("Log_Match_StartNew"), 9999);
            _game.ClickPlay();
            Thread.Sleep(1000);
            _game.ClickPlay(); // Press a second time just to be sure
            Thread.Sleep(3000);

            return WaitForMatchmaking();
        }

        private bool WaitForMatchmaking()
        {
            _config.GetWindowPositions();

            var mmTimer = new Stopwatch();
            mmTimer.Start();

            Log(_localizer.GetString("Log_Check_Matchmaking"), 9999, true);
            while (Check(_game.CanIdentifyLadderMatchmaking))
            {
                if (mmTimer.Elapsed.TotalSeconds > _rand.Next(300, 360))
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
            var active = true;
            // var alreadySnapped = false;
            _rand = new Random();

            /*
            Log("Rolling for snap decision...");
            var snapLimit = 0.465;
            var snapRoll = Math.Round(_rand.NextDouble(), 3);
            var shouldSnap = snapRoll <= snapLimit;
            Log("Limit:  " + snapLimit.ToString(), true);
            Log("Rolled: " + snapRoll.ToString(), true);
            Log("Snap:   " + (shouldSnap ? "YES" : "NO"));
            */

            _matchTimer = new Stopwatch();
            _matchTimer.Start();

            var currentTurn = 0;

            while (active && _matchTimer.Elapsed.Minutes < 15)
            {
                _config.GetWindowPositions();

                Log(_localizer.GetString("Log_Check_ActiveMatch"), 9999, true);
                if (!Check(_game.CanIdentifyActiveEventMatch))
                {
                    var check = false;
                    for (int x = 1; x < 3 && !check; x++)
                    {
                        Log(_localizer.GetString("Log_Check_ActiveMatch_Failed"), 9999);
                        _config.GetWindowPositions();
                        _game.ResetClick();
                        check = Check(_game.CanIdentifyActiveEventMatch);
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
					}
					else
                    {
                        Log(_localizer.GetString("Log_Match_PlayingCards").Replace("%VALUE%", currentTurn.ToString()), 9999);
                        _game.PlayHand();
                        Thread.Sleep(1000);

                        _config.GetWindowPositions();

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

                /*if (shouldSnap && !alreadySnapped)
                {                     
                    Log("Attempting to snap...");
                    _game.ClickSnap();
                    alreadySnapped = true;
                }*/
            }

            _config.GetWindowPositions();

            if (_matchTimer.Elapsed.Minutes > 15 && Check(_game.CanIdentifyLadderRetreatBtn))
            {
                Log(_localizer.GetString("Log_Match_MaxTimeReached"), 9999);
                _game.ClickRetreat();
                Thread.Sleep(5000);
            }

            Log(_localizer.GetString("Log_Check_MatchEnd"), 9999, true);
            if (Check(_game.CanIdentifyLadderMatchEnd))
                return ExitMatch();

            return false;
        }

        private bool ExitMatch()
        {
            Log(_localizer.GetString("Log_Match_Exiting"), 9999);
            _config.GetWindowPositions();

            Log(_localizer.GetString("Log_Check_PostMatchScreen"), 9999, true);
            while (Check(_game.CanIdentifyLadderCollectRewardsBtn) || Check(_game.CanIdentifyLadderMatchEndNextBtn))
            {
                _game.ClickNext();
                Thread.Sleep(6000);
                _config.GetWindowPositions();
            }

            return true;
        }
    }

}
