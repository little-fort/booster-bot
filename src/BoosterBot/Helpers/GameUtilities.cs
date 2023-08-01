using BoosterBot.Models;
using System.Drawing;
using System.Windows.Forms;

namespace BoosterBot
{
    internal class GameUtilities
    {
        public static void LogLadderGameState(BotConfig config)
        {
            config.GetWindowPositions();

            Console.WriteLine($"{(CanIdentifyMainMenu(config) ? "X" : " ")} MAIN_MENU");
            Console.WriteLine($"{(CanIdentifyZeroEnergy(config) ? "X" : " ")} MID_MATCH");
            Console.WriteLine($"{(CanIdentifyLadderMatchmaking(config) ? "X" : " ")} LADDER_MATCHMAKING");
            Console.WriteLine($"{(CanIdentifyLadderRetreatBtn(config) ? "X" : " ")} LADDER_MATCH (Retreat button)");
            Console.WriteLine($"{(CanIdentifyEndTurnBtn(config) ? "X" : " ")} LADDER_MATCH (End turn button)");
            Console.WriteLine($"{(CanIdentifyMidTurn(config) ? "X" : " ")} LADDER_MATCH (Mid turn buttons)");
            Console.WriteLine($"{(CanIdentifyLadderMatchEnd(config) ? "X" : " ")} LADDER_MATCH_END");
        }

        public static void LogConquestGameState(BotConfig config)
        {
            config.GetWindowPositions();

            if (CanIdentifyMainMenu(config)) Console.WriteLine("Found state: MAIN_MENU");
            if (CanIdentifyZeroEnergy(config)) Console.WriteLine("Found state: MID_MATCH");
            if (CanIdentifyConquestPlayBtn(config)) Console.WriteLine("Found state: CONQUEST_PREMATCH");
            if (CanIdentifyConquestLobbyPG(config)) Console.WriteLine("Found state: CONQUEST_LOBBY_PG");
            if (CanIdentifyConquestMatchmaking(config)) Console.WriteLine("Found state: CONQUEST_MATCHMAKING");
            if (CanIdentifyConquestRetreatBtn(config)) Console.WriteLine("Found state: CONQUEST_MATCH (Retreat button)");
            if (CanIdentifyEndTurnBtn(config)) Console.WriteLine("Found state: CONQUEST_MATCH (End turn button)");
            if (CanIdentifyMidTurn(config)) Console.WriteLine("Found state: CONQUEST_MATCH (Mid turn buttons)");
            if (CanIdentifyConquestConcede(config)) Console.WriteLine("Found state: CONQUEST_ROUND_END");
            if (CanIdentifyConquestMatchEnd(config)) Console.WriteLine("Found state: CONQUEST_MATCH_END");
            if (CanIdentifyConquestLossContinue(config)) Console.WriteLine("Found state: CONQUEST_POSTMATCH_LOSS_SCREEN");
            if (CanIdentifyConquestWinNext(config)) Console.WriteLine("Found state: CONQUEST_POSTMATCH_WIN_CONTINUE");
            if (CanIdentifyConquestTicketClaim(config)) Console.WriteLine("Found state: CONQUEST_POSTMATCH_WIN_TICKET");
        }

        public static GameState DetermineLadderGameState(BotConfig config)
        {
            config.GetWindowPositions();

            if (CanIdentifyMainMenu(config)) return GameState.MAIN_MENU;
            if (CanIdentifyLadderMatchmaking(config)) return GameState.LADDER_MATCHMAKING;
            if (CanIdentifyLadderRetreatBtn(config)) return GameState.LADDER_MATCH;
            if (CanIdentifyEndTurnBtn(config)) return GameState.LADDER_MATCH;
            if (CanIdentifyMidTurn(config)) return GameState.LADDER_MATCH;
            if (CanIdentifyLadderMatchEnd(config)) return GameState.LADDER_MATCH_END;
            if (CanIdentifyZeroEnergy(config)) return GameState.MID_MATCH;
            if (CanIdentifyConquestLobbyPG(config)) return GameState.CONQUEST_LOBBY_PG;

            return GameState.UNKNOWN;
        }

        public static GameState DetermineConquestGameState(BotConfig config)
        {
            config.GetWindowPositions();

            if (CanIdentifyMainMenu(config)) return GameState.MAIN_MENU;
            if (CanIdentifyConquestPlayBtn(config)) return GameState.CONQUEST_PREMATCH;
            if (CanIdentifyConquestLobbyPG(config)) return GameState.CONQUEST_LOBBY_PG;
            if (CanIdentifyConquestMatchmaking(config)) return GameState.CONQUEST_MATCHMAKING;
            if (CanIdentifyConquestRetreatBtn(config)) return GameState.CONQUEST_MATCH;
            if (CanIdentifyEndTurnBtn(config)) return GameState.CONQUEST_MATCH;
            if (CanIdentifyMidTurn(config)) return GameState.CONQUEST_MATCH;
            if (CanIdentifyConquestConcede(config)) return GameState.CONQUEST_ROUND_END;
            if (CanIdentifyConquestMatchEnd(config)) return GameState.CONQUEST_MATCH_END;
            if (CanIdentifyConquestLossContinue(config)) return GameState.CONQUEST_POSTMATCH_LOSS_SCREEN;
            if (CanIdentifyConquestWinNext(config)) return GameState.CONQUEST_POSTMATCH_WIN_CONTINUE;
            if (CanIdentifyConquestTicketClaim(config)) return GameState.CONQUEST_POSTMATCH_WIN_TICKET;
            if (CanIdentifyZeroEnergy(config)) return GameState.MID_MATCH;

            return GameState.UNKNOWN;
        }

        public static bool CanIdentifyMainMenu(BotConfig config)
        {
            // Get coordinates for 'Play' button
            var area = ComponentMappings.GetBtnPlay(config.Center, config.Screencap);

            // Check if 'Play' button is visible
            return ImageUtilities.CheckImageAreaSimilarity(area, ComponentMappings.REF_LADD_BTN_PLAY);
        }

        public static bool CanIdentifyZeroEnergy(BotConfig config)
            => ImageUtilities.CheckImageAreaSimilarity(ComponentMappings.GetEnergy(config.Center, config.Screencap), ComponentMappings.REF_ICON_ZERO_ENERGY, 0.925);

        #region Ladder

        public static bool CanIdentifyActiveLadderMatch(BotConfig config)
            => CanIdentifyLadderRetreatBtn(config) ||
                CanIdentifyEndTurnBtn(config) ||
                CanIdentifyMidTurn(config);

        public static bool CanIdentifyLadderMatchmaking(BotConfig config)
            => ImageUtilities.CheckImageAreaSimilarity(ComponentMappings.GetLadderMatchmakingCancel(config.Center, config.Screencap), ComponentMappings.REF_LADD_BTN_MATCHMAKING);

        public static bool CanIdentifyLadderRetreatBtn(BotConfig config)
            => ImageUtilities.CheckImageAreaSimilarity(ComponentMappings.GetLadderBtnRetreat(config.Center, config.Screencap), ComponentMappings.REF_LADD_BTN_RETREAT);

        public static bool CanIdentifyLadderCollectRewardsBtn(BotConfig config)
            => ImageUtilities.CheckImageAreaSimilarity(ComponentMappings.GetConquestBtnCollect(config.Center, config.Screencap), ComponentMappings.REF_LADD_BTN_COLLECT_REWARDS);

        public static bool CanIdentifyLadderMatchEndNextBtn(BotConfig config)
            => ImageUtilities.CheckImageAreaSimilarity(ComponentMappings.GetConquestBtnMatchEndNext2(config.Center, config.Screencap), ComponentMappings.REF_LADD_BTN_MATCH_END_NEXT);

        public static bool CanIdentifyLadderMatchEnd(BotConfig config)
            => CanIdentifyLadderCollectRewardsBtn(config) || CanIdentifyLadderMatchEndNextBtn(config);

        #endregion

        #region Conquest

        public static bool CanIdentifyActiveConquestMatch(BotConfig config)
            => CanIdentifyConquestRetreatBtn(config) ||
                CanIdentifyEndTurnBtn(config) ||
                CanIdentifyMidTurn(config);

        public static bool CanIdentifyConquestPlayBtn(BotConfig config)
            => ImageUtilities.CheckImageAreaSimilarity(ComponentMappings.GetBtnPlay(config.Center, config.Screencap), ComponentMappings.REF_CONQ_BTN_PLAY);

        public static bool CanIdentifyConquestLobbyPG(BotConfig config)
            => ImageUtilities.CheckImageAreaSimilarity(ComponentMappings.GetConquestLobbySelection(config.Center), ComponentMappings.REF_CONQ_LBL_LOBBY_PG);

        public static bool CanIdentifyConquestLobbySilver(BotConfig config)
            => ImageUtilities.CheckImageAreaSimilarity(ComponentMappings.GetConquestLobbySelection(config.Center), ComponentMappings.REF_CONQ_LBL_LOBBY_SILVER);

        public static bool CanIdentifyConquestLobbyGold(BotConfig config)
            => ImageUtilities.CheckImageAreaSimilarity(ComponentMappings.GetConquestLobbySelection(config.Center), ComponentMappings.REF_CONQ_LBL_LOBBY_GOLD);

        public static bool CanIdentifyConquestLobbyInfinite(BotConfig config)
            => ImageUtilities.CheckImageAreaSimilarity(ComponentMappings.GetConquestLobbySelection(config.Center), ComponentMappings.REF_CONQ_LBL_LOBBY_INFINITE);

        public static bool CanIdentifyConquestMatchmaking(BotConfig config)
            => ImageUtilities.CheckImageAreaSimilarity(ComponentMappings.GetConquestMatchmakingCancel(config.Center, config.Screencap), ComponentMappings.REF_CONQ_BTN_MATCHMAKING);

        public static bool CanIdentifyConquestRetreatBtn(BotConfig config)
            => ImageUtilities.CheckImageAreaSimilarity(ComponentMappings.GetConquestBtnRetreat(config.Center, config.Screencap), ComponentMappings.REF_CONQ_BTN_RETREAT_1) ||
               ImageUtilities.CheckImageAreaSimilarity(ComponentMappings.GetConquestBtnRetreat(config.Center, config.Screencap), ComponentMappings.REF_CONQ_BTN_RETREAT_2);

        public static bool CanIdentifyEndTurnBtn(BotConfig config)
            => ImageUtilities.CheckImageAreaSimilarity(ComponentMappings.GetConquestBtnEndTurn(config.Center, config.Screencap), ComponentMappings.REF_CONQ_BTN_END_TURN);

        public static bool CanIdentifyMidTurn(BotConfig config)
            => ImageUtilities.CheckImageAreaSimilarity(ComponentMappings.GetConquestBtnWaiting(config.Center, config.Screencap), ComponentMappings.REF_CONQ_BTN_WAITING, 0.85) ||
               ImageUtilities.CheckImageAreaSimilarity(ComponentMappings.GetConquestBtnWaiting(config.Center, config.Screencap), ComponentMappings.REF_CONQ_BTN_PLAYING, 0.85);

        public static bool CanIdentifyConquestConcede(BotConfig config)
            => ImageUtilities.CheckImageAreaSimilarity(ComponentMappings.GetConquestBtnConcede(config.Center, config.Screencap), ComponentMappings.REF_CONQ_BTN_CONCEDE_1) ||
               ImageUtilities.CheckImageAreaSimilarity(ComponentMappings.GetConquestBtnConcede(config.Center, config.Screencap), ComponentMappings.REF_CONQ_BTN_CONCEDE_2);

        public static bool CanIdentifyConquestMatchEnd(BotConfig config)
            => CanIdentifyConquestMatchEndNext1(config) || CanIdentifyConquestMatchEndNext2(config);

        public static bool CanIdentifyConquestMatchEndNext1(BotConfig config)
            => ImageUtilities.CheckImageAreaSimilarity(ComponentMappings.GetConquestBtnMatchEndNext1(config.Center, config.Screencap), ComponentMappings.REF_CONQ_BTN_MATCH_END_1);

        public static bool CanIdentifyConquestMatchEndNext2(BotConfig config)
            => ImageUtilities.CheckImageAreaSimilarity(ComponentMappings.GetConquestBtnMatchEndNext2(config.Center, config.Screencap), ComponentMappings.REF_CONQ_BTN_MATCH_END_2);

        public static bool CanIdentifyConquestLossContinue(BotConfig config)
            => ImageUtilities.CheckImageAreaSimilarity(ComponentMappings.GetConquestBtnContinue(config.Center, config.Screencap), ComponentMappings.REF_CONQ_BTN_CONTINUE);

        public static bool CanIdentifyConquestWinNext(BotConfig config)
            => ImageUtilities.CheckImageAreaSimilarity(ComponentMappings.GetConquestVictoryNext(config.Center, config.Screencap), ComponentMappings.REF_CONQ_BTN_WIN_NEXT);

        public static bool CanIdentifyConquestTicketClaim(BotConfig config)
            => ImageUtilities.CheckImageAreaSimilarity(ComponentMappings.GetConquestTicketClaim(config.Center, config.Screencap), ComponentMappings.REF_CONQ_BTN_WIN_TICKET);

        #endregion

        public static void ResetClick(BotConfig config) => SystemUtilities.Click(config.ResetPoint);

        public static void ResetMenu(BotConfig config) => SystemUtilities.Click(config.Window.Left + config.Center, config.Window.Bottom - 1);

        public static void ClearError(BotConfig config) => SystemUtilities.Click(config.ClearErrorPoint);

        /// <summary>
        /// Simulates attempting to play four cards in your hand to random locations.
        /// </summary>
        public static void PlayHand(BotConfig config)
        {
            var rand = new Random();
            SystemUtilities.PlayCard(config.Cards[3], config.Locations[rand.Next(3)], config.ResetPoint);
            SystemUtilities.PlayCard(config.Cards[2], config.Locations[rand.Next(3)], config.ResetPoint);
            SystemUtilities.PlayCard(config.Cards[1], config.Locations[rand.Next(3)], config.ResetPoint);
            SystemUtilities.PlayCard(config.Cards[0], config.Locations[rand.Next(3)], config.ResetPoint);

            ResetClick(config);
        }

        /// <summary>
        /// SNAP
        /// </summary>
        /// <returns></returns>
        public static bool ClickSnap(BotConfig config)
        {
            var rand = new Random();
            SystemUtilities.Click(config.Window.Left + config.Center + rand.Next(-20, 20), 115 + rand.Next(-20, 20));
            Logger.Log("OH SNAP!", config.LogPath);

            return true;
        }

        /// <summary>
        /// Simulates clicking the "Play" button while on the main menu.
        /// </summary>
        public static void ClickPlay(BotConfig config)
        {
            var rand = new Random();
            SystemUtilities.Click(config.Window.Left + config.Center + rand.Next(-20, 20), config.Window.Bottom - 200 + rand.Next(-20, 20));
        }

        /// <summary>
        /// Simulates clicking the "Next"/"Collect Rewards" button while in a match.
        /// </summary>
        public static void ClickNext(BotConfig config)
        {
            var rand = new Random();
            SystemUtilities.Click(config.Window.Left + config.Center + 300 + rand.Next(-20, 20), config.Window.Bottom - 60 + rand.Next(-10, 10));
        }

        /// <summary>
        /// Simulates clicks to from a match.
        /// </summary>
        public static void ClickRetreat(BotConfig config)
        {
            config.GetWindowPositions();

            var pnt = new Point // Retreat
            {
                X = config.Window.Left + config.Center - 300,
                Y = config.Window.Bottom - 70
            };
            SystemUtilities.Click(pnt);

            pnt = new Point // Retreat Now
            {
                X = config.Window.Left + config.Center - 100,
                Y = config.Window.Bottom - 280
            };
            SystemUtilities.Click(pnt);

            Thread.Sleep(10000);
        }

        public static void PressEscKey() => SendKeys.SendWait("{ESC}");

        public static void BlindReset(BotConfig config)
        {
            config.GetWindowPositions();
            Logger.Log("Attempting blind reset clicks...", config.LogPath);
            ResetClick(config);
            Thread.Sleep(1000);
            ClearError(config);
            Thread.Sleep(1000);
            ClickNext(config);
            Thread.Sleep(1000);
            ResetMenu(config);
            Thread.Sleep(1000);
            ResetClick(config);
            Thread.Sleep(1000);
            PressEscKey();
            Thread.Sleep(1000);
        }

        public static Rect GetConquestBannerCrop(BotConfig config) => new Rect
        {
            Left = config.Center - 110,
            Right = config.Center + 100,
            Top = 15,
            Bottom = 60
        };
    }
}
