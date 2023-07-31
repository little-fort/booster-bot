using BoosterBot.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace BoosterBot
{
    internal class GameUtilities
    {
        public static void LogGameState(BotConfig config)
        {
            config.GetWindowPositions();

            if (CanIdentifyMainMenu(config)) Console.WriteLine("Found state: MAIN_MENU");
            if (CanIdentifyZeroEnergy(config)) Console.WriteLine("Found state: MID_MATCH");
            if (CanIdentifyConquestPlayBtn(config)) Console.WriteLine("Found state: CONQUEST_PREMATCH");
            if (CanIdentifyConquestLobbyPG(config)) Console.WriteLine("Found state: CONQUEST_LOBBY_PG");
            if (CanIdentifyConquestMatchmaking(config)) Console.WriteLine("Found state: CONQUEST_MATCHMAKING");
            if (CanIdentifyConquestRetreatBtn(config)) Console.WriteLine("Found state: CONQUEST_MATCH (Retreat button)");
            if (CanIdentifyConquestEndTurnBtn(config)) Console.WriteLine("Found state: CONQUEST_MATCH (End turn button)");
            if (CanIdentifyConquestMidTurn(config)) Console.WriteLine("Found state: CONQUEST_MATCH (Mid turn buttons)");
            if (CanIdentifyConquestConcede(config)) Console.WriteLine("Found state: CONQUEST_ROUND_END");
            if (CanIdentifyConquestMatchEnd(config)) Console.WriteLine("Found state: CONQUEST_MATCH_END");
            if (CanIdentifyConquestLossContinue(config)) Console.WriteLine("Found state: CONQUEST_POSTMATCH_LOSS_SCREEN");
            if (CanIdentifyConquestWinNext(config)) Console.WriteLine("Found state: CONQUEST_POSTMATCH_WIN_CONTINUE");
            if (CanIdentifyConquestTicketClaim(config)) Console.WriteLine("Found state: CONQUEST_POSTMATCH_WIN_TICKET");
        }

        public static GameState DetermineGameState(BotConfig config)
        {
            config.GetWindowPositions();

            if (CanIdentifyMainMenu(config)) return GameState.MAIN_MENU;
            if (CanIdentifyZeroEnergy(config)) return GameState.MID_MATCH;
            if (CanIdentifyConquestPlayBtn(config)) return GameState.CONQUEST_PREMATCH;
            if (CanIdentifyConquestLobbyPG(config)) return GameState.CONQUEST_LOBBY_PG;
            if (CanIdentifyConquestMatchmaking(config)) return GameState.CONQUEST_MATCHMAKING;
            if (CanIdentifyConquestRetreatBtn(config)) return GameState.CONQUEST_MATCH;
            if (CanIdentifyConquestEndTurnBtn(config)) return GameState.CONQUEST_MATCH;
            if (CanIdentifyConquestMidTurn(config)) return GameState.CONQUEST_MATCH;
            if (CanIdentifyConquestConcede(config)) return GameState.CONQUEST_ROUND_END;
            if (CanIdentifyConquestMatchEnd(config)) return GameState.CONQUEST_MATCH_END;
            if (CanIdentifyConquestLossContinue(config)) return GameState.CONQUEST_POSTMATCH_LOSS_SCREEN;
            if (CanIdentifyConquestWinNext(config)) return GameState.CONQUEST_POSTMATCH_WIN_CONTINUE;
            if (CanIdentifyConquestTicketClaim(config)) return GameState.CONQUEST_POSTMATCH_WIN_TICKET;

            return GameState.UNKNOWN;
        }

        public static bool CanIdentifyMainMenu(BotConfig config)
        {
            // Reset menu to center
            //SystemUtilities.Click(config.Window.Left + config.Center, config.Window.Bottom - 1);

            // Get coordinates for 'Play' button
            var area = ComponentMappings.GetBtnPlay(config.Center, config.Screencap);

            // Check if 'Play' button is visible
            return ImageUtilities.CheckImageAreaSimilarity(area, ComponentMappings.REF_LADD_BTN_PLAY);
        }

        public static bool CanIdentifyActiveConquestMatch(BotConfig config)
            => CanIdentifyConquestRetreatBtn(config) ||
                CanIdentifyConquestEndTurnBtn(config) ||
                CanIdentifyConquestMidTurn(config);

        public static bool CanIdentifyZeroEnergy(BotConfig config)
            => ImageUtilities.CheckImageAreaSimilarity(ComponentMappings.GetEnergy(config.Center, config.Screencap), ComponentMappings.REF_ICON_ZERO_ENERGY, 0.925);

        public static bool CanIdentifyConquestPlayBtn(BotConfig config)
            => ImageUtilities.CheckImageAreaSimilarity(ComponentMappings.GetBtnPlay(config.Center, config.Screencap), ComponentMappings.REF_CONQ_BTN_PLAY);

        public static bool CanIdentifyConquestLobbyPG(BotConfig config)
            => ImageUtilities.CheckImageAreaSimilarity(ComponentMappings.GetConquestLobbySelection(config.Center), ComponentMappings.REF_CONQ_LBL_LOBBY_PG);

        public static bool CanIdentifyConquestMatchmaking(BotConfig config)
            => ImageUtilities.CheckImageAreaSimilarity(ComponentMappings.GetConquestMatchmakingCancel(config.Center, config.Screencap), ComponentMappings.REF_CONQ_BTN_MATCHMAKING);

        public static bool CanIdentifyConquestRetreatBtn(BotConfig config)
            => ImageUtilities.CheckImageAreaSimilarity(ComponentMappings.GetConquestBtnRetreat(config.Center, config.Screencap), ComponentMappings.REF_CONQ_BTN_RETREAT_1) ||
               ImageUtilities.CheckImageAreaSimilarity(ComponentMappings.GetConquestBtnRetreat(config.Center, config.Screencap), ComponentMappings.REF_CONQ_BTN_RETREAT_2);

        public static bool CanIdentifyConquestEndTurnBtn(BotConfig config)
            => ImageUtilities.CheckImageAreaSimilarity(ComponentMappings.GetConquestBtnEndTurn(config.Center, config.Screencap), ComponentMappings.REF_CONQ_BTN_END_TURN);

        public static bool CanIdentifyConquestMidTurn(BotConfig config)
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
            Logger.Log("OH SNAP!");

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

        public static void BlindReset(BotConfig config)
        {
            config.GetWindowPositions();
            Logger.Log("Attempting blind reset clicks...");
            ResetClick(config);
            Thread.Sleep(1000);
            ClearError(config);
            Thread.Sleep(1000);
            ClickNext(config);
            Thread.Sleep(1000);
            ResetMenu(config);
            Thread.Sleep(1000);
            ResetClick(config);
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
