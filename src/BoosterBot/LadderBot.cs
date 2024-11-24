using BoosterBot.Helpers;
using BoosterBot.Models;
using BoosterBot.Resources;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace BoosterBot
{
    internal class LadderBot : BaseBot
    {
        public LadderBot(BotConfig config, int retreat) : base(config, retreat) { }

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
                    Console.WriteLine($"{Strings.Log_Error} {ex.Message}");
                    Thread.Sleep(5000);
                }
            }
        }

        public override void Run()
        {
            Log("Ladder_Log_Start", 9999);
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
                    DetermineLoopEntryPoint();
                else
                {
                    if (attempts <= 2)
                    {
                        Log("Log_FailedMenuDetection", 9999);
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
            Log("Log_LoopEntryPoint", 9999);
            var state = _game.DetermineLadderGameState();

            switch (state)
            {
                case GameState.MAIN_MENU:
                    Log("Log_DetectedMainMenu", 9999);
                    StartMatch();
                    return true;
                case GameState.RECONNECT_TO_GAME:
                    Log("Log_DetectedReconnect", 9999);
                    _game.ClickPlay();
                    Thread.Sleep(4000);
                    return PlayMatch();
                case GameState.MID_MATCH:
                    Log("Log_DetectedActiveMatch", 9999);
                    return StartMatch();
                case GameState.LADDER_MATCHMAKING:
                    Log("Log_DetectedMatchmaking", 9999);
                    return WaitForMatchmaking();
                case GameState.LADDER_MATCH:
                    Log("Log_DetectedActiveMatch", 9999);
                    return PlayMatch();
                case GameState.LADDER_MATCH_END:
                case GameState.LADDER_MATCH_END_REWARDS:
                    Log("Log_DetectedMatchEnd", 9999);
                    return ExitMatch();
                case GameState.CONQUEST_LOBBY_PG:
                    Log("Log_Ladder_DetectedConquest", 9999);
                    _game.ResetMenu();
                    return StartMatch();
                default:
                    if (attempts < 5)
                    {
                        _game.BlindReset();
                        return DetermineLoopEntryPoint(attempts + 1);
                    }

                    Log("Log_LostBot", 9999);
                    Log("Log_LostBot_Restart", 9999);
                    Console.WriteLine();
                    Log("Menu_PressKeyToExit", 9999);
                    Console.ReadKey();
                    Environment.Exit(0);
                    return false;
            }
        }

        private bool StartMatch()
        {
            Log("Log_Match_StartNew", 9999);
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

            Log("Log_Check_Matchmaking", 9999, true);
            while (Check(_game.CanIdentifyLadderMatchmaking))
            {
                if (mmTimer.Elapsed.TotalSeconds > _rand.Next(300, 360))
                {
                    Log("Log_Check_Matchmaking_Hanged", 9999);
                    _game.ClickCancel();
                    return true;
                }

                Log("Log_Matchmaking_Waiting", 9999, replace: [new("%ELAPSED%", mmTimer.Elapsed.ToString())]);
                Thread.Sleep(5000);
                _config.GetWindowPositions();
            }

            return PlayMatch();
        }

        private bool PlayMatch()
        {
            Log("Log_Match_Playing", 9999);
            var active = true;
            var alreadySnapped = false;
            _rand = new Random();

            Log("Log_Match_SnapRoll", 9999);
            var snapLimit = 0.465;
            var snapRoll = Math.Round(_rand.NextDouble(), 3);
            var shouldSnap = snapRoll <= snapLimit;
            Log("Log_Match_SnapRoll_Limit", 9999, true, replace: [new("%VALUE%", snapLimit.ToString())]);
            Log("Log_Match_SnapRoll_Result", 9999, true, replace: [new("%VALUE%", snapRoll.ToString())]);
            Log("Log_Match_SnapRoll_Snap", 9999, true, replace: [new("%VALUE%", shouldSnap ? "YES" : "NO")]);

            _matchTimer = new Stopwatch();
            _matchTimer.Start();

            var currentTurn = 0;

            while (active && _matchTimer.Elapsed.Minutes < 15)
            {
                _config.GetWindowPositions();

                Log("Log_Check_ActiveMatch", 9999, true);
                if (!Check(_game.CanIdentifyActiveLadderMatch))
                {
                    var check = false;
                    for (int x = 1; x < 3 && !check; x++)
                    {
                        Log("Log_Check_ActiveMatch_Failed", 9999);
                        _config.GetWindowPositions();
                        _game.ResetClick();
                        check = Check(_game.CanIdentifyActiveLadderMatch);
                        Thread.Sleep(2500);
                    }

                    active = check;
                }
                else
                {
                    if (currentTurn++ >= _retreatAfterTurn)
                    {
                        Log("Log_Match_ReachedTurnLimit", 9999, replace: [new("%VALUE%", _retreatAfterTurn.ToString())]);
                        _game.ClickRetreat();
                        Thread.Sleep(5000);
					}
					else
                    {
                        Log("Log_Match_PlayingCards", 9999, replace: [new("%VALUE%", currentTurn.ToString())]);
                        _game.PlayHand();
                        Thread.Sleep(1000);

                        _config.GetWindowPositions();

                        Log("Log_Check_EnergyState", 9999, true);
                        if (!Check(_game.CanIdentifyZeroEnergy))
                        {
                            Log("Log_Match_LeftoverEnergy", 9999);
                            _game.PlayHand();
                        }

                        Log("Log_Match_EndTurn", 9999);
                        _game.ClickNext();
                        Thread.Sleep(1000);

                        _config.GetWindowPositions();

                        Log("Log_Check_TurnState", 9999, true);
                        while (Check(_game.CanIdentifyMidTurn))
                        {
                            Log("Log_Match_WaitingForTurn", 9999);
                            Thread.Sleep(4000);
                            _config.GetWindowPositions();
                        }
                    }
                }

                if (shouldSnap && !alreadySnapped)
                {                     
                    Log("Log_Match_Snapping", 9999);
                    _game.ClickSnap();
                    alreadySnapped = true;
                }
            }

            _config.GetWindowPositions();

            if (_matchTimer.Elapsed.Minutes > 15 && Check(_game.CanIdentifyLadderRetreatBtn))
            {
                Log("Log_Match_MaxTimeReached", 9999);
                _game.ClickRetreat();
                Thread.Sleep(5000);
            }

            Log("Log_Check_MatchEnd", 9999, true);
            if (Check(_game.CanIdentifyLadderMatchEnd))
                return ExitMatch();

            return false;
        }

        private bool ExitMatch()
        {
            Log("Log_Match_Exiting", 9999);
            _config.GetWindowPositions();

            Log("Log_Check_PostMatchScreen", 9999, true);
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
