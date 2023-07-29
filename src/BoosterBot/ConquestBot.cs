using BoosterBot.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace BoosterBot
{
    internal class ConquestBot
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
            var attempts = 0;
            _matchTimer = new Stopwatch();
            _matchTimer.Start();

            while (true)
            {
                attempts++;

                _config.GetWindowPositions();
                var onMenu = GameUtilities.CanIdentifyMainMenu(_config);

                if (onMenu)
                {
                    Logger.Log("Detected main menu. Navigating to Conquest...");
                    NavigateToConquest();
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
                        var state = GameUtilities.DetermineGameState(_config);
                    }
                }
            }
        }

        private void NavigateToConquest()
        {

        }

        private void RunMatchLoop()
        {
            SelectLobby();
            StartMatch();
            PlayMatch();
            ExitMatch();
        }
    }
}
