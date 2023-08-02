using BoosterBot.Models;
using System.Drawing;
using System.Windows.Forms;

namespace BoosterBot
{
    internal class GameUtilities
    {
        private readonly BotConfig _config;
        private readonly ComponentMappings _mappings;
        public GameUtilities(BotConfig config)
        {
            _config = config;
            _mappings = new ComponentMappings(config);
        }

        public void LogLadderGameState()
        {
            _config.GetWindowPositions();

            Console.WriteLine($"{(CanIdentifyMainMenu() ? "X" : " ")} MAIN_MENU");
            Console.WriteLine($"{(CanIdentifyZeroEnergy() ? "X" : " ")} MID_MATCH");
            Console.WriteLine($"{(CanIdentifyLadderMatchmaking() ? "X" : " ")} LADDER_MATCHMAKING");
            Console.WriteLine($"{(CanIdentifyLadderRetreatBtn() ? "X" : " ")} LADDER_MATCH (Retreat button)");
            Console.WriteLine($"{(CanIdentifyEndTurnBtn() ? "X" : " ")} LADDER_MATCH (End turn button)");
            Console.WriteLine($"{(CanIdentifyMidTurn() ? "X" : " ")} LADDER_MATCH (Mid turn buttons)");
            Console.WriteLine($"{(CanIdentifyLadderMatchEnd() ? "X" : " ")} LADDER_MATCH_END");
        }

        public void LogConquestGameState()
        {
            _config.GetWindowPositions();

            if (CanIdentifyMainMenu()) Console.WriteLine("Found state: MAIN_MENU");
            if (CanIdentifyZeroEnergy()) Console.WriteLine("Found state: MID_MATCH");
            if (CanIdentifyConquestPlayBtn()) Console.WriteLine("Found state: CONQUEST_PREMATCH");
            if (CanIdentifyConquestLobbyPG()) Console.WriteLine("Found state: CONQUEST_LOBBY_PG");
            if (CanIdentifyConquestMatchmaking()) Console.WriteLine("Found state: CONQUEST_MATCHMAKING");
            if (CanIdentifyConquestRetreatBtn()) Console.WriteLine("Found state: CONQUEST_MATCH (Retreat button)");
            if (CanIdentifyEndTurnBtn()) Console.WriteLine("Found state: CONQUEST_MATCH (End turn button)");
            if (CanIdentifyMidTurn()) Console.WriteLine("Found state: CONQUEST_MATCH (Mid turn buttons)");
            if (CanIdentifyConquestConcede()) Console.WriteLine("Found state: CONQUEST_ROUND_END");
            if (CanIdentifyConquestMatchEnd()) Console.WriteLine("Found state: CONQUEST_MATCH_END");
            if (CanIdentifyConquestLossContinue()) Console.WriteLine("Found state: CONQUEST_POSTMATCH_LOSS_SCREEN");
            if (CanIdentifyConquestWinNext()) Console.WriteLine("Found state: CONQUEST_POSTMATCH_WIN_CONTINUE");
            if (CanIdentifyConquestTicketClaim()) Console.WriteLine("Found state: CONQUEST_POSTMATCH_WIN_TICKET");
        }

        public GameState DetermineLadderGameState()
        {
            _config.GetWindowPositions();

            if (CanIdentifyMainMenu()) return GameState.MAIN_MENU;
            if (CanIdentifyLadderMatchmaking()) return GameState.LADDER_MATCHMAKING;
            if (CanIdentifyLadderRetreatBtn()) return GameState.LADDER_MATCH;
            if (CanIdentifyEndTurnBtn()) return GameState.LADDER_MATCH;
            if (CanIdentifyMidTurn()) return GameState.LADDER_MATCH;
            if (CanIdentifyLadderMatchEnd()) return GameState.LADDER_MATCH_END;
            if (CanIdentifyZeroEnergy()) return GameState.MID_MATCH;
            if (CanIdentifyConquestLobbyPG()) return GameState.CONQUEST_LOBBY_PG;

            return GameState.UNKNOWN;
        }

        public GameState DetermineConquestGameState()
        {
            _config.GetWindowPositions();

            if (CanIdentifyMainMenu()) return GameState.MAIN_MENU;
            if (CanIdentifyConquestPlayBtn()) return GameState.CONQUEST_PREMATCH;
            if (CanIdentifyConquestLobbyPG()) return GameState.CONQUEST_LOBBY_PG;
            if (CanIdentifyConquestMatchmaking()) return GameState.CONQUEST_MATCHMAKING;
            if (CanIdentifyConquestRetreatBtn()) return GameState.CONQUEST_MATCH;
            if (CanIdentifyEndTurnBtn()) return GameState.CONQUEST_MATCH;
            if (CanIdentifyMidTurn()) return GameState.CONQUEST_MATCH;
            if (CanIdentifyConquestConcede()) return GameState.CONQUEST_ROUND_END;
            if (CanIdentifyConquestMatchEnd()) return GameState.CONQUEST_MATCH_END;
            if (CanIdentifyConquestLossContinue()) return GameState.CONQUEST_POSTMATCH_LOSS_SCREEN;
            if (CanIdentifyConquestWinNext()) return GameState.CONQUEST_POSTMATCH_WIN_CONTINUE;
            if (CanIdentifyConquestTicketClaim()) return GameState.CONQUEST_POSTMATCH_WIN_TICKET;
            if (CanIdentifyZeroEnergy()) return GameState.MID_MATCH;

            return GameState.UNKNOWN;
        }

        public bool CanIdentifyMainMenu()
        {
            // Get coordinates for 'Play' button
            var area = _mappings.GetBtnPlay();

            // Check if 'Play' button is visible
            return ImageUtilities.CheckImageAreaSimilarity(area, ComponentMappings.REF_LADD_BTN_PLAY);
        }

        public bool CanIdentifyZeroEnergy()
            => ImageUtilities.CheckImageAreaSimilarity(_mappings.GetEnergy(), ComponentMappings.REF_ICON_ZERO_ENERGY, 0.925);

        #region Ladder

        public bool CanIdentifyActiveLadderMatch()
            => CanIdentifyLadderRetreatBtn() ||
                CanIdentifyEndTurnBtn() ||
                CanIdentifyMidTurn();

        public bool CanIdentifyLadderMatchmaking()
            => ImageUtilities.CheckImageAreaSimilarity(_mappings.GetLadderMatchmakingCancel(), ComponentMappings.REF_LADD_BTN_MATCHMAKING);

        public bool CanIdentifyLadderRetreatBtn()
            => ImageUtilities.CheckImageAreaSimilarity(_mappings.GetLadderBtnRetreat(), ComponentMappings.REF_LADD_BTN_RETREAT);

        public bool CanIdentifyLadderCollectRewardsBtn()
            => ImageUtilities.CheckImageAreaSimilarity(_mappings.GetConquestBtnCollect(), ComponentMappings.REF_LADD_BTN_COLLECT_REWARDS);

        public bool CanIdentifyLadderMatchEndNextBtn()
            => ImageUtilities.CheckImageAreaSimilarity(_mappings.GetConquestBtnMatchEndNext2(), ComponentMappings.REF_LADD_BTN_MATCH_END_NEXT);

        public bool CanIdentifyLadderMatchEnd()
            => CanIdentifyLadderCollectRewardsBtn() || CanIdentifyLadderMatchEndNextBtn();

        #endregion

        #region Conquest

        public bool CanIdentifyActiveConquestMatch()
            => CanIdentifyConquestRetreatBtn() ||
                CanIdentifyEndTurnBtn() ||
                CanIdentifyMidTurn();

        public bool CanIdentifyConquestPlayBtn()
            => ImageUtilities.CheckImageAreaSimilarity(_mappings.GetBtnPlay(), ComponentMappings.REF_CONQ_BTN_PLAY);

        public bool CanIdentifyConquestLobbyPG()
            => ImageUtilities.CheckImageAreaSimilarity(_mappings.GetConquestLobbySelection(), ComponentMappings.REF_CONQ_LBL_LOBBY_PG);

        public bool CanIdentifyConquestLobbySilver()
            => ImageUtilities.CheckImageAreaSimilarity(_mappings.GetConquestLobbySelection(), ComponentMappings.REF_CONQ_LBL_LOBBY_SILVER);

        public bool CanIdentifyConquestLobbyGold()
            => ImageUtilities.CheckImageAreaSimilarity(_mappings.GetConquestLobbySelection(), ComponentMappings.REF_CONQ_LBL_LOBBY_GOLD);

        public bool CanIdentifyConquestLobbyInfinite()
            => ImageUtilities.CheckImageAreaSimilarity(_mappings.GetConquestLobbySelection(), ComponentMappings.REF_CONQ_LBL_LOBBY_INFINITE);

        public bool CanIdentifyConquestMatchmaking()
            => ImageUtilities.CheckImageAreaSimilarity(_mappings.GetConquestMatchmakingCancel(), ComponentMappings.REF_CONQ_BTN_MATCHMAKING);

        public bool CanIdentifyConquestRetreatBtn()
            => ImageUtilities.CheckImageAreaSimilarity(_mappings.GetConquestBtnRetreat(), ComponentMappings.REF_CONQ_BTN_RETREAT_1) ||
               ImageUtilities.CheckImageAreaSimilarity(_mappings.GetConquestBtnRetreat(), ComponentMappings.REF_CONQ_BTN_RETREAT_2);

        public bool CanIdentifyEndTurnBtn()
            => ImageUtilities.CheckImageAreaSimilarity(_mappings.GetConquestBtnEndTurn(), ComponentMappings.REF_CONQ_BTN_END_TURN);

        public bool CanIdentifyMidTurn()
            => ImageUtilities.CheckImageAreaSimilarity(_mappings.GetConquestBtnWaiting(), ComponentMappings.REF_CONQ_BTN_WAITING, 0.85) ||
               ImageUtilities.CheckImageAreaSimilarity(_mappings.GetConquestBtnWaiting(), ComponentMappings.REF_CONQ_BTN_PLAYING, 0.85);

        public bool CanIdentifyConquestConcede()
            => ImageUtilities.CheckImageAreaSimilarity(_mappings.GetConquestBtnConcede(), ComponentMappings.REF_CONQ_BTN_CONCEDE_1) ||
               ImageUtilities.CheckImageAreaSimilarity(_mappings.GetConquestBtnConcede(), ComponentMappings.REF_CONQ_BTN_CONCEDE_2);

        public bool CanIdentifyConquestMatchEnd()
            => CanIdentifyConquestMatchEndNext1() || CanIdentifyConquestMatchEndNext2();

        public bool CanIdentifyConquestMatchEndNext1()
            => ImageUtilities.CheckImageAreaSimilarity(_mappings.GetConquestBtnMatchEndNext1(), ComponentMappings.REF_CONQ_BTN_MATCH_END_1);

        public bool CanIdentifyConquestMatchEndNext2()
            => ImageUtilities.CheckImageAreaSimilarity(_mappings.GetConquestBtnMatchEndNext2(), ComponentMappings.REF_CONQ_BTN_MATCH_END_2);

        public bool CanIdentifyConquestLossContinue()
            => ImageUtilities.CheckImageAreaSimilarity(_mappings.GetConquestBtnContinue(), ComponentMappings.REF_CONQ_BTN_CONTINUE);

        public bool CanIdentifyConquestWinNext()
            => ImageUtilities.CheckImageAreaSimilarity(_mappings.GetConquestVictoryNext(), ComponentMappings.REF_CONQ_BTN_WIN_NEXT);

        public bool CanIdentifyConquestTicketClaim()
            => ImageUtilities.CheckImageAreaSimilarity(_mappings.GetConquestTicketClaim(), ComponentMappings.REF_CONQ_BTN_WIN_TICKET);

        #endregion

        public void ResetClick() => SystemUtilities.Click(_config.ResetPoint);

        public void ResetMenu() => SystemUtilities.Click(_config.Window.Left + _config.Center, _config.Window.Bottom - _config.Scale(50));

        public void ClearError() => SystemUtilities.Click(_config.ClearErrorPoint);

        /// <summary>
        /// Simulates attempting to play four cards in your hand to random locations.
        /// </summary>
        public void PlayHand()
        {
            var rand = new Random();
            SystemUtilities.PlayCard(_config.Cards[3], _config.Locations[rand.Next(3)], _config.ResetPoint);
            SystemUtilities.PlayCard(_config.Cards[2], _config.Locations[rand.Next(3)], _config.ResetPoint);
            SystemUtilities.PlayCard(_config.Cards[1], _config.Locations[rand.Next(3)], _config.ResetPoint);
            SystemUtilities.PlayCard(_config.Cards[0], _config.Locations[rand.Next(3)], _config.ResetPoint);

            ResetClick();
        }

        /// <summary>
        /// SNAP
        /// </summary>
        /// <returns></returns>
        public bool ClickSnap()
        {
            SystemUtilities.Click(_config.SnapPoint);
            Logger.Log("OH SNAP!", _config.LogPath);

            return true;
        }

        /// <summary>
        /// Simulates clicking the "Play" button while on the main menu.
        /// </summary>
        public void ClickPlay() => SystemUtilities.Click(_config.PlayPoint);

        /// <summary>
        /// Simulates clicking the "Next"/"Collect Rewards" button while in a match.
        /// </summary>
        public void ClickNext() => SystemUtilities.Click(_config.NextPoint);

        /// <summary>
        /// Simulates clicks to from a match.
        /// </summary>
        public void ClickRetreat()
        {
            _config.GetWindowPositions();
            SystemUtilities.Click(_config.RetreatPoint);

            Thread.Sleep(1000);
            SystemUtilities.Click(_config.RetreatConfirmPoint);

            Thread.Sleep(5000);
        }

        public void PressEscKey() => SendKeys.SendWait("{ESC}");

        public void BlindReset()
        {
            _config.GetWindowPositions();
            Logger.Log("Attempting blind reset clicks...", _config.LogPath);
            ResetClick();
            Thread.Sleep(1000);
            ClearError();
            Thread.Sleep(1000);
            ClickNext();
            Thread.Sleep(1000);
            ResetMenu();
            Thread.Sleep(1000);
            ResetClick();
            Thread.Sleep(1000);
            PressEscKey();
            Thread.Sleep(1000);
        }
    }
}
