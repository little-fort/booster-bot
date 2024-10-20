﻿using BoosterBot.Models;
using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace BoosterBot
{
    internal class EventBot : IBoosterBot
    {
        private readonly string _logPath;
        private readonly BotConfig _config;
        private readonly GameUtilities _game;
        private readonly int _retreatAfterTurn;
        private Random _rand { get; set; }
        private Stopwatch _matchTimer { get; set; }


        public EventBot(double scaling, bool verbose, bool autoplay, bool saveScreens, int retreatAfterTurn, bool downscaled, bool useEvent = false)
        {
            _logPath = $"logs\\ladder-log-{DateTime.Now.ToString("yyyyMMddHHmmss")}.txt";
            _config = new BotConfig(scaling, verbose, autoplay, saveScreens, _logPath, useEvent);
            _retreatAfterTurn = retreatAfterTurn;
            _game = new GameUtilities(_config);
            _rand = new Random();

            // If the bot is running on a lower resolution, we need to adjust the confidence level to account for slight variations in image quality
            if (downscaled)
                _game.SetDefaultConfidence(0.9);

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
            Log("Starting Event bot...");
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
                    Log("Detected main menu. Navigating to event LTM...");
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

        private void NavigateToLtmMenu()
        {
            Log("Navigating to LTM menu...");

            for (int x = 0; x < 3; x++)
            {
                SystemUtilities.Click(_config.EventBannerPoint);
                Thread.Sleep(1000);
            }
        }

        private bool DetermineLoopEntryPoint(int attempts = 0)
        {
            Log("Attempting to determine loop entry point...");
            var state = _game.DetermineLadderGameState();

            switch (state)
            {
                case GameState.MAIN_MENU:
                    Log("Detected main menu. Navigating to event menu...");
                    NavigateToGameModes();
                    NavigateToLtmMenu();
                    Log("Starting new match...");
                    StartMatch();
                    return true;
                case GameState.EVENT_MENU:
                    Log("Detected event menu. Starting new match...");
                    StartMatch();
                    return true;
                case GameState.RECONNECT_TO_GAME:
                    Log("Detected 'Reconnect to Game' button. Resuming match play...");
                    _game.ClickPlay();
                    Thread.Sleep(4000);
                    return PlayMatch();
                case GameState.MID_MATCH:
                    Log("Detected active match. Resuming match play...");
                    return StartMatch();
                case GameState.LADDER_MATCHMAKING:
                    Log("Detected matchmaking...");
                    return WaitForMatchmaking();
                case GameState.LADDER_MATCH:
                    Log("Detected active match. Resuming match play...");
                    return PlayMatch();
                case GameState.LADDER_MATCH_END:
                case GameState.LADDER_MATCH_END_REWARDS:
                    Log("Detected match end. Returning to main menu...");
                    return ExitMatch();
                case GameState.CONQUEST_LOBBY_PG:
                    Log("Detected Conquest lobby. Resetting menu...");
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

                    Log("Bot is hopelessly lost... :/");
                    Log("Return to main menu and restart bot.");
                    Console.WriteLine();
                    Log("Press any key to exit...");
                    Console.ReadKey();
                    Environment.Exit(0);
                    return false;

            }
        }

        private bool StartMatch()
        {
            Log("Clicking 'Play'...");
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

            Log("Checking for ongoing matchmaking...", true);
            while (Check(_game.CanIdentifyLadderMatchmaking))
            {
                if (mmTimer.Elapsed.TotalSeconds > _rand.Next(300, 360))
                {
                    Log("Matchmaking seems to be hanging. Returning to main menu to re-try...");
                    _game.ClickCancel();
                    return true;
                }

                Logger.Log($"Waiting for match start... [Elapsed: {mmTimer.Elapsed}]", _logPath);
                Thread.Sleep(5000);
                _config.GetWindowPositions();
            }

            return PlayMatch();
        }

        private bool PlayMatch()
        {
            Log("Playing match...");
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

                Log("Checking for active ladder match...", true);
                if (!Check(_game.CanIdentifyActiveEventMatch))
                {
                    var check = false;
                    for (int x = 1; x < 3 && !check; x++)
                    {
                        Log("Could not detect active match, trying again in 2 seconds...");
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
                        Log($"Configured turn limit ({_retreatAfterTurn}) reached. Attempting retreat...");
                        _game.ClickRetreat();
                        Thread.Sleep(5000);
					}
					else
                    {
                        Log($"Attempting to play cards... [Turn count: {currentTurn}]");
                        _game.PlayHand();
                        Thread.Sleep(1000);

                        _config.GetWindowPositions();

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
                Log("Match timer has eclipsed 15 minutes. Attempting retreat...");
                _game.ClickRetreat();
                Thread.Sleep(5000);
            }

            Log("Checking for end of match...", true);
            if (Check(_game.CanIdentifyLadderMatchEnd))
                return ExitMatch();

            return false;
        }

        private bool ExitMatch()
        {
            Log("Exiting match...");
            _config.GetWindowPositions();

            Log("Checking for post-match screens...", true);
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
