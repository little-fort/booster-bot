using BoosterBot.Models;
using System.Diagnostics;

namespace BoosterBot
{
    internal class ConquestBot : IBoosterBot
    {
        private readonly BotConfig _config;
        private Random _rand { get; set; }
        private Stopwatch _matchTimer { get; set; }

        public ConquestBot(double scaling, bool verbose, bool autoplay, bool saveScreens)
        {
            _config = new BotConfig(scaling, verbose, autoplay, saveScreens);
            _rand = new Random();
        }

        public void Run()
        {
            Logger.Log("Starting Conquest bot...");
            var attempts = 0;

            /*
            // Used for debugging crop zones
            while (true)
            {
                GameUtilities.LogGameState(_config);
                Console.WriteLine("--------------------------------------------------------");
                Console.WriteLine("--------------------------------------------------------");
                Thread.Sleep(5000);
            }*/

            while (true)
            {
                attempts++;

                _config.GetWindowPositions();
                GameUtilities.ResetClick(_config);
                GameUtilities.ResetMenu(_config);

                var onMenu = GameUtilities.CanIdentifyMainMenu(_config);

                if (onMenu)
                {
                    Logger.Log("Detected main menu. Navigating to Conquest...");
                    NavigateToGameModes();
                    NavigateToConquestMenu();
                    RunMatchLoop();
                }
                else
                {
                    if (attempts <= 2)
                    {
                        Logger.Log($"Could not detect main menu (attempt #{attempts}). Trying again in 5 seconds...");
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

        private void NavigateToGameModes()
        {
            Logger.Log("Navigating to Game Modes tab...");
            SystemUtilities.Click(_config.GameModesPoint);
            Thread.Sleep(1000);
            SystemUtilities.Click(_config.GameModesPoint);
            Thread.Sleep(2500);
        }

        private void NavigateToConquestMenu()
        {
            Logger.Log("Navigating to Conquest menu...");
            SystemUtilities.Click(_config.Window.Left + _config.Center + _rand.Next(-20, 20), 330 + _rand.Next(-20, 20));
        }

        private bool DetermineLoopEntryPoint(bool finalAttempt = false)
        {
            Logger.Log("Attempting to determine loop entry point...");
            var state = GameUtilities.DetermineConquestGameState(_config);

            switch (state)
            {
                case GameState.MAIN_MENU:
                    Logger.Log("Detected main menu. Returning to start...");
                    Run(); 
                    return true;
                case GameState.MID_MATCH: 
                    Logger.Log("Detected mid-match. Resuming match play...");
                    return PlayMatch();
                case GameState.CONQUEST_LOBBY_PG: 
                    Logger.Log("Detected Conquest lobby selection. Entering Proving Grounds...");
                    return SelectLobby();
                case GameState.CONQUEST_PREMATCH:
                    Logger.Log("Detected Conquest prematch. Starting match...");
                    return StartMatch();
                case GameState.CONQUEST_MATCHMAKING:
                    Logger.Log("Detected matchmaking...");
                    return WaitForMatchmaking();
                case GameState.CONQUEST_MATCH:
                    Logger.Log("Detected Conquest match. Playing match...");
                    return PlayMatch();
                case GameState.CONQUEST_ROUND_END:
                    Logger.Log("Detected Conquest round end. Moving to next round...");
                    return ProgressRound();
                case GameState.CONQUEST_MATCH_END:
                case GameState.CONQUEST_MATCH_END_REWARDS:
                    Logger.Log("Detected Conquest match end. Returning to Conquest menu...");
                    return ExitMatch();
                case GameState.CONQUEST_POSTMATCH_LOSS_SCREEN:
                case GameState.CONQUEST_POSTMATCH_WIN_CONTINUE:
                case GameState.CONQUEST_POSTMATCH_WIN_TICKET:
                    Logger.Log("Detected Conquest postmatch screen. Returning to Conquest menu...");
                    return AcceptResult();
                default:
                    if (!finalAttempt)
                    {
                        GameUtilities.BlindReset(_config);
                        return DetermineLoopEntryPoint(true);
                    }

                    Logger.Log("Bot is hopelessly lost... :/");
                    Logger.Log("Return to main menu and restart bot.");
                    Environment.Exit(0);
                    return false;

            }
        }

        private void RunMatchLoop()
        {
            Logger.Log("Starting match loop...");
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
                    GameUtilities.BlindReset(_config);
            }
        }

        private bool SelectLobby()
        {
            Thread.Sleep(1000);
            var lobbyConfirmed = false;

            Logger.Log("Making sure lobby type is set to Proving Grounds...");
            for (int x = 0; x < 3 && !lobbyConfirmed; x++)
            {
                // The carousel defaults to the highest diffulty level that you have tickets for. Need to first swipe back over to Proving Grounds
                for (int i = 0; i < 5; i++)
                {
                    SystemUtilities.Drag(
                        startX: _config.Window.Left + _config.Center - 250,
                        startY: _config.Window.Bottom / 2,
                        endX: _config.Window.Left + _config.Center + 250,
                        endY: _config.Window.Bottom / 2
                    );
                    Thread.Sleep(1000);
                }

                Thread.Sleep(3000);
                _config.GetWindowPositions();
                lobbyConfirmed = GameUtilities.CanIdentifyConquestLobbyPG(_config);
            }

            if (!lobbyConfirmed)
            {
                Logger.Log("Checking for active Conquest lobby...");

                _config.GetWindowPositions();
                var crop = GameUtilities.GetConquestBannerCrop(_config);
                var isSilver = ImageUtilities.ReadArea(crop, expected: "SILVER"); // CalculateSimilarity("SILVER", text) > 60.0;
                var isGold = ImageUtilities.ReadArea(crop, expected: "GOLD"); // CalculateSimilarity("GOLD", text) > 60.0;
                var isInfinite = ImageUtilities.ReadArea(crop, expected: "INFINITE"); // CalculateSimilarity("INFINITE", text) > 60.0;
                if (isSilver || isGold || isInfinite)
                {
                    Logger.Log("\n\n############## WARNING ##############");
                    Logger.Log($"Detected active Conquest lobby in a tier higher than Proving Grounds. BoosterBot will stop running to avoid consuming Conquest tickets.");
                    Logger.Log("Finish your current Conquest matches and restart when Proving Grounds is accessible again.");
                    Logger.Log("\nPress [Enter] to exit...");
                    Console.ReadLine();
                    Environment.Exit(0);
                }
                else
                    Logger.Log("Could not confirm Conquest lobby status. Restarting navigation...");
            }

            return lobbyConfirmed;
        }

        private bool StartMatch()
        {
            Logger.Log("Entering Proving Grounds lobby...");
            GameUtilities.ClickPlay(_config);
            Thread.Sleep(5000);

            Logger.Log("Clicking 'Play'...");
            GameUtilities.ClickPlay(_config);
            Thread.Sleep(1000);
            GameUtilities.ClickPlay(_config); // Press a second time just to be sure
            Thread.Sleep(1000);

            Logger.Log("Confirming deck...");
            SystemUtilities.Click(_config.Window.Left + _config.Center + 100, _config.Window.Bottom - 345);
            Thread.Sleep(2000);

            return WaitForMatchmaking();
        }

        private bool WaitForMatchmaking()
        {
            _config.GetWindowPositions();
            while (GameUtilities.CanIdentifyConquestMatchmaking(_config))
            {
                Logger.Log("Waiting for match start...");
                Thread.Sleep(5000);
                _config.GetWindowPositions();
            }

            return PlayMatch();
        }

        private bool PlayMatch()
        {
            Logger.Log("Playing match...");
            var active = true;
            GameUtilities.ClickSnap(_config);

            _matchTimer = new Stopwatch();
            _matchTimer.Start();

            while (active && _matchTimer.Elapsed.Minutes < 30)
            {
                _config.GetWindowPositions();
                if (!GameUtilities.CanIdentifyActiveConquestMatch(_config))
                {
                    var check = false;
                    for (int x = 1; x < 3 && !check; x++)
                    {
                        Logger.Log("Could not detect active match, trying again in 4 seconds...");
                        _config.GetWindowPositions();
                        GameUtilities.ResetClick(_config);
                        check = GameUtilities.CanIdentifyActiveConquestMatch(_config);
                        Thread.Sleep(4000);
                    }

                    active = check;
                }
                else
                {
                    Logger.Log("Attempting to play cards...");
                    GameUtilities.PlayHand(_config);
                    Thread.Sleep(1000);

                    _config.GetWindowPositions();
                    if (!GameUtilities.CanIdentifyZeroEnergy(_config))
                    {
                        Logger.Log("Detected leftover energy, will attempt to play cards again...");
                        GameUtilities.PlayHand(_config);
                    }

                    Logger.Log("Clicking 'Next Turn'...");
                    GameUtilities.ClickNext(_config);
                    Thread.Sleep(1000);

                    _config.GetWindowPositions();
                    while (GameUtilities.CanIdentifyMidTurn(_config))
                    {
                        Logger.Log("Waiting for turn to progress...");
                        Thread.Sleep(4000);
                        _config.GetWindowPositions();
                    }
                }
            }

            _config.GetWindowPositions();
            if (_matchTimer.Elapsed.Minutes > 15 && GameUtilities.CanIdentifyConquestRetreatBtn(_config))
            {
                Logger.Log("Match timer has eclipsed 30 minutes. Attempting retreat...");
                GameUtilities.ClickRetreat(_config);
                Thread.Sleep(5000);
            }

            if (GameUtilities.CanIdentifyConquestConcede(_config))
                return ProgressRound();
            else if (GameUtilities.CanIdentifyConquestMatchEnd(_config))
                return ExitMatch();

            return false;
        }

        private bool ProgressRound()
        {
            Logger.Log("Identified round end. Proceeding to next round...");
            GameUtilities.ClickNext(_config);

            _config.GetWindowPositions();
            var waitTime = 0;
            while (!GameUtilities.CanIdentifyActiveConquestMatch(_config) && !GameUtilities.CanIdentifyConquestMatchEnd(_config))
            {
                if (waitTime >= 90000)
                {
                    Logger.Log("Max wait time of 90 seconds elapsed...");
                    GameUtilities.BlindReset(_config);
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
            Logger.Log("Exiting match...");
            _config.GetWindowPositions();

            while (GameUtilities.CanIdentifyConquestMatchEndNext1(_config) || GameUtilities.CanIdentifyConquestMatchEndNext2(_config))
            {
                GameUtilities.ClickNext(_config);
                Thread.Sleep(4000);
                _config.GetWindowPositions();
            }

            Logger.Log("Waiting for post-match screens...");
            while (!GameUtilities.CanIdentifyConquestLossContinue(_config) && !GameUtilities.CanIdentifyConquestWinNext(_config))
            {
                Thread.Sleep(2000);
                _config.GetWindowPositions();
            }

            return AcceptResult();
        }

        private bool AcceptResult()
        {
            Logger.Log("Processing post-match screens...");

            _config.GetWindowPositions();
            if (GameUtilities.CanIdentifyConquestLossContinue(_config) || GameUtilities.CanIdentifyConquestWinNext(_config) || GameUtilities.CanIdentifyConquestTicketClaim(_config))
            {
                GameUtilities.ClickPlay(_config);
                Thread.Sleep(5000);
                _config.GetWindowPositions();
                return AcceptResult();
            }

            return true;
        }
    }
}
