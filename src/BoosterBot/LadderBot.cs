using System.Diagnostics;
using BoosterBot.Models;
using BoosterBot.Resources;
namespace BoosterBot
{
    internal class LadderBot(BotConfig config, int retreat) : BaseBot(config, retreat)
    {
        protected override async Task ExecuteCycleAsync(CancellationToken token)
        {
            CheckForStop();
            if (_isStopped) return;
            _config.GetWindowPositions();
            _game.ResetClick();
            _game.ResetMenu();
            _game.ResetClick();
            await SafeDelay(500, token);
            _config.GetWindowPositions();
            var onMenu = Check(_game.CanIdentifyMainMenu);
            if (onMenu)
            {
                await DetermineLoopEntryPointAsync();
            }
            else
            {
                Log("Log_FailedMenuDetection");
                _game.ResetClick();
                await SafeDelay(5000, token);
                await DetermineLoopEntryPointAsync();
            }
        }
        private async Task<bool> DetermineLoopEntryPointAsync(int attempts = 0)
        {
            Log("Log_LoopEntryPoint");
            var state = _game.DetermineLadderGameState();
            Log("Log_Match_Snapping");
            switch (state)
            {
                case GameState.MAIN_MENU:
                    Log("Log_DetectedMainMenu");
                    return await StartMatchAsync();
                case GameState.RECONNECT_TO_GAME:
                    Log("Log_DetectedReconnect");
                    _game.ClickPlay();
                    await SafeDelay(4000, _cts.Token);
                    return await PlayMatchAsync();
                case GameState.MID_MATCH:
                    Log("Log_DetectedActiveMatch");
                    return await StartMatchAsync();
                case GameState.LADDER_MATCHMAKING:
                    Log("Log_DetectedMatchmaking");
                    return await WaitForMatchmakingAsync();
                case GameState.LADDER_MATCH:
                    Log("Log_DetectedActiveMatch");
                    return await PlayMatchAsync();
                case GameState.LADDER_MATCH_END:
                case GameState.LADDER_MATCH_END_REWARDS:
                    Log("Log_DetectedMatchEnd");
                    return await ExitMatchAsync();
                case GameState.CONQUEST_LOBBY_PG:
                    Log("Log_Ladder_DetectedConquest");
                    _game.ResetMenu();
                    return await StartMatchAsync();
                default:
                    if (attempts < 5)
                    {
                        _game.BlindReset();
                        return await DetermineLoopEntryPointAsync(attempts + 1);
                    }
                    Log("Log_LostBot");
                    Log("Log_LostBot_Restart");
                    Console.WriteLine();
                    Log("Menu_PressKeyToExit");
                    Environment.Exit(0);
                    return false;
            }
        }
        private async Task<bool> StartMatchAsync()
        {
            Log("Log_Match_StartNew");
            _game.ClickPlay();
            await SafeDelay(1000, _cts.Token);
            _game.ClickPlay();
            await SafeDelay(3000, _cts.Token);
            return await WaitForMatchmakingAsync();
        }
        private async Task<bool> WaitForMatchmakingAsync()
        {
            _config.GetWindowPositions();
            var mmTimer = new Stopwatch();
            mmTimer.Start();
            Log("Log_Check_Matchmaking", true);
            while (Check(() => _game.CanIdentifyLadderMatchmaking()))
            {
                if (mmTimer.Elapsed.TotalSeconds > _rand.Next(300, 360))
                {
                    Log("Log_Check_Matchmaking_Hanged");
                    _game.ClickCancel();
                    return true;
                }
                Log("Log_Matchmaking_Waiting", replace: new List<FindReplaceValue> { new("%ELAPSED%", mmTimer.Elapsed.ToString(@"mm\:ss")) });
                await SafeDelay(5000, _cts.Token);
                _config.GetWindowPositions();
            }
            return await PlayMatchAsync();
        }
        private async Task<bool> PlayMatchAsync()
        {
            Log("Log_Match_Playing");
            var active = true;
            Thread.Sleep(5000);
            _rand = new Random();
            Log("Log_Match_SnapRoll");
            var snapLimit = 0.465;
            var snapRoll = Math.Round(_rand.NextDouble(), 3);
            var shouldSnap = snapRoll <= snapLimit;
            Log("Log_Match_SnapRoll_Limit", true, new List<FindReplaceValue> { new("%VALUE%", snapLimit.ToString()) });
            Log("Log_Match_SnapRoll_Result", true, new List<FindReplaceValue> { new("%VALUE%", snapRoll.ToString()) });
            Log("Log_Match_SnapRoll_Snap", true, new List<FindReplaceValue> { new("%VALUE%", shouldSnap ? "YES" : "NO") });
            _matchTimer = Stopwatch.StartNew();
            var currentTurn = 0;
            while (active && _matchTimer.Elapsed.TotalMinutes < 15 && !_cts.IsCancellationRequested)
            {
                _config.GetWindowPositions();
                Log("Log_Check_ActiveMatch", true);
                if (!Check(() => _game.CanIdentifyActiveLadderMatch()))
                {

                    var check = true;
                    for (int x = 1; x < 3 && !check; x++)
                    {
                        Log("Log_Check_ActiveMatch_Failed");
                        _config.GetWindowPositions();
                        _game.ResetClick();
                        check = Check(() => _game.CanIdentifyActiveLadderMatch());

                        _game.ClickSnap();
                        await SafeDelay(4000, _cts.Token);
                    }
                    active = check;
                }
                else
                {
                    currentTurn++;
                    if (_retreatAfterTurn > 0 && currentTurn >= _retreatAfterTurn)
                    {
                        Log("Log_Match_ReachedTurnLimit", replace: new List<FindReplaceValue> { new("%VALUE%", _retreatAfterTurn.ToString()) });
                        _game.ClickRetreat();
                        await SafeDelay(5000, _cts.Token);
                        active = false;
                        break; 
                    }
                    else
                    {
                        Log("Log_Match_PlayingCards", replace: new List<FindReplaceValue> { new("%VALUE%", currentTurn.ToString()) });
                        _game.PlayHand();
                        await SafeDelay(1000, _cts.Token);
                        _config.GetWindowPositions();
                        Log("Log_Check_EnergyState", true);
                        if (!Check(_game.CanIdentifyZeroEnergy))
                        {
                            Log("Log_Match_LeftoverEnergy");
                            _game.PlayHand();
                        }
                        Log("Log_Match_EndTurn");
                        _game.ClickNext();
                        await SafeDelay(1000, _cts.Token);
                        _config.GetWindowPositions();
                        Log("Log_Check_TurnState", true);
                        while (Check(() => _game.CanIdentifyMidTurn()))
                        {
                            Log("Log_Match_WaitingForTurn");
                            await SafeDelay(4000, _cts.Token);
                            _config.GetWindowPositions();
                        }
                    }
                }

            }
            _config.GetWindowPositions();
            if (_matchTimer.Elapsed.TotalMinutes > 15 && Check(() => _game.CanIdentifyLadderRetreatBtn()))
            {
                Log("Log_Match_MaxTimeReached");
                _game.ClickRetreat();
                await SafeDelay(5000, _cts.Token);
            }
            Log("Log_Check_MatchEnd", true);
            if (Check(() => _game.CanIdentifyLadderMatchEnd()))
                return await ExitMatchAsync();
            return false;
        }
        private async Task<bool> ExitMatchAsync()
        {
            Log("Log_Match_Exiting");
            _config.GetWindowPositions();
            Log("Log_Check_PostMatchScreen", true);
            while ((Check(() => _game.CanIdentifyLadderCollectRewardsBtn()) ||
                   Check(() => _game.CanIdentifyLadderMatchEndNextBtn())) &&
                   !_cts.IsCancellationRequested)
            {
                _game.ClickNext();
                await SafeDelay(6000, _cts.Token);
                _config.GetWindowPositions();
            }
            return true;
        }
    }
}