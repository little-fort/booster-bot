﻿using BoosterBot.Models;
using System;
using System.Drawing;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

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

            Console.WriteLine($"{(CanIdentifyMainMenu() ? "X" : " ")} MAIN_MENU");
            Console.WriteLine($"{(CanIdentifyReconnectToGameBtn() ? "X" : " ")} RECONNECT_TO_GAME");
            Console.WriteLine($"{(CanIdentifyZeroEnergy() ? "X" : " ")} MID_MATCH");
            Console.WriteLine($"{(CanIdentifyConquestNoTickets() ? "X" : " ")} CONQUEST_NO_TICKETS");
            Console.WriteLine($"{(CanIdentifyConquestPlayBtn() ? "X" : " ")} CONQUEST_PREMATCH");
            Console.WriteLine($"{(CanIdentifyConquestLobbyPG() ? "X" : " ")} CONQUEST_LOBBY_PG");
            Console.WriteLine($"{(CanIdentifyConquestLobbySilver() ? "X" : " ")} CONQUEST_LOBBY_SILVER");
            Console.WriteLine($"{(CanIdentifyConquestLobbyGold() ? "X" : " ")} CONQUEST_LOBBY_GOLD");
            Console.WriteLine($"{(CanIdentifyConquestLobbyInfinite() ? "X" : " ")} CONQUEST_LOBBY_INFINITE");
            Console.WriteLine($"{(CanIdentifyConquestMatchmaking() ? "X" : " ")} CONQUEST_MATCHMAKING");
            Console.WriteLine($"{(CanIdentifyConquestRetreatBtn() ? "X" : " ")} CONQUEST_MATCH (Retreat button)");
            Console.WriteLine($"{(CanIdentifyEndTurnBtn() ? "X" : " ")} CONQUEST_MATCH (End turn button)");
            Console.WriteLine($"{(CanIdentifyMidTurn() ? "X" : " ")} CONQUEST_MATCH (Mid turn buttons)");
            Console.WriteLine($"{(CanIdentifyConquestConcede() ? "X" : " ")} CONQUEST_ROUND_END");
            Console.WriteLine($"{(CanIdentifyConquestMatchEnd() ? "X" : " ")} CONQUEST_MATCH_END");
            Console.WriteLine($"{(CanIdentifyConquestLossContinue() ? "X" : " ")} CONQUEST_POSTMATCH_LOSS_SCREEN");
            Console.WriteLine($"{(CanIdentifyConquestWinNext() ? "X" : " ")} CONQUEST_POSTMATCH_WIN_CONTINUE");
            Console.WriteLine($"{(CanIdentifyConquestTicketClaim() ? "X" : " ")} CONQUEST_POSTMATCH_WIN_TICKET");
        }

        public GameState DetermineLadderGameState()
        {
            _config.GetWindowPositions();

            if (CanIdentifyMainMenu()) return GameState.MAIN_MENU;
            if (CanIdentifyReconnectToGameBtn()) return GameState.RECONNECT_TO_GAME;
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
            if (CanIdentifyReconnectToGameBtn()) return GameState.RECONNECT_TO_GAME;
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

        public GameState DetermineConquestLobbyTier()
        {
            _config.GetWindowPositions();

            if (CanIdentifyConquestLobbyPG()) return GameState.CONQUEST_LOBBY_PG;
            if (CanIdentifyConquestLobbySilver()) return GameState.CONQUEST_LOBBY_SILVER;
            if (CanIdentifyConquestLobbyGold()) return GameState.CONQUEST_LOBBY_GOLD;
            if (CanIdentifyConquestLobbyInfinite()) return GameState.CONQUEST_LOBBY_INFINITE;

            return GameState.UNKNOWN;
        }

        public bool CanIdentifyMainMenu()
        {
            // Get coordinates for 'Play' button
            var area = _mappings.GetBtnPlay();

            // Check if 'Play' button is visible
            return ImageUtilities.CheckImageAreaSimilarity(area, ComponentMappings.REF_LADD_BTN_PLAY);
        }

        public bool CanIdentifyReconnectToGameBtn()
            => ImageUtilities.CheckImageAreaSimilarity(_mappings.GetBtnPlay(), ComponentMappings.REF_BTN_RECONNECT_TO_GAME);

        public bool CanIdentifyZeroEnergy()
            => ImageUtilities.CheckImageAreaSimilarity(_mappings.GetEnergy(), ComponentMappings.REF_ICON_ZERO_ENERGY, 0.925);

        #region Ladder

        public bool CanIdentifyActiveLadderMatch()
            => CanIdentifyLadderRetreatBtn() ||
                CanIdentifyEndTurnBtn() ||
                CanIdentifyMidTurn();

        public bool CanIdentifyLadderMatchmaking()
            => ImageUtilities.CheckImageAreaSimilarity(_mappings.GetLadderMatchmakingCancel(), ComponentMappings.REF_LADD_BTN_MATCHMAKING_1) ||
               ImageUtilities.CheckImageAreaSimilarity(_mappings.GetLadderMatchmakingCancel(), ComponentMappings.REF_LADD_BTN_MATCHMAKING_2);

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

        public bool CanIdentifyAnyConquestLobby() => CanIdentifyConquestLobbyPG() ||
                                                     CanIdentifyConquestLobbySilver() ||
                                                     CanIdentifyConquestLobbyGold() ||
                                                     CanIdentifyConquestLobbyInfinite();

        public bool CanIdentifyConquestLobbyPG()
            => ImageUtilities.CheckImageAreaSimilarity(_mappings.GetConquestLobbySelection(), ComponentMappings.REF_CONQ_LBL_LOBBY_PG_1) ||
               ImageUtilities.CheckImageAreaSimilarity(_mappings.GetConquestBannerCrop(), ComponentMappings.REF_CONQ_LBL_LOBBY_PG_2);

        public bool CanIdentifyConquestNoTickets()
            => ImageUtilities.CheckImageAreaSimilarity(_mappings.GetConquestOwnedTicketsIcon(), ComponentMappings.REF_CONQ_LBL_NO_TICKETS);

        public bool CanIdentifyConquestLobbySilver()
            => ImageUtilities.CheckImageAreaSimilarity(_mappings.GetConquestLobbySelection(), ComponentMappings.REF_CONQ_LBL_LOBBY_SILVER_1) ||
               ImageUtilities.CheckImageAreaSimilarity(_mappings.GetConquestLobbySelection(), ComponentMappings.REF_CONQ_LBL_LOBBY_SILVER_2) ||
               ImageUtilities.CheckImageAreaSimilarity(_mappings.GetConquestBannerCrop(), ComponentMappings.REF_CONQ_LBL_LOBBY_SILVER_3);
               

        public bool CanIdentifyConquestLobbyGold()
            => ImageUtilities.CheckImageAreaSimilarity(_mappings.GetConquestLobbySelection(), ComponentMappings.REF_CONQ_LBL_LOBBY_GOLD_1) ||
               ImageUtilities.CheckImageAreaSimilarity(_mappings.GetConquestLobbySelection(), ComponentMappings.REF_CONQ_LBL_LOBBY_GOLD_2) ||
               ImageUtilities.CheckImageAreaSimilarity(_mappings.GetConquestBannerCrop(), ComponentMappings.REF_CONQ_LBL_LOBBY_GOLD_3);

        public bool CanIdentifyConquestLobbyInfinite()
            => ImageUtilities.CheckImageAreaSimilarity(_mappings.GetConquestLobbySelection(), ComponentMappings.REF_CONQ_LBL_LOBBY_INFINITE_1) ||
               ImageUtilities.CheckImageAreaSimilarity(_mappings.GetConquestLobbySelection(), ComponentMappings.REF_CONQ_LBL_LOBBY_INFINITE_2) ||
               ImageUtilities.CheckImageAreaSimilarity(_mappings.GetConquestBannerCrop(), ComponentMappings.REF_CONQ_LBL_LOBBY_INFINITE_3);

        public bool CanIdentifyConquestMatchmaking()
            => ImageUtilities.CheckImageAreaSimilarity(_mappings.GetConquestMatchmakingCancel(), ComponentMappings.REF_CONQ_BTN_MATCHMAKING_1) ||
               ImageUtilities.CheckImageAreaSimilarity(_mappings.GetConquestMatchmakingCancel(), ComponentMappings.REF_CONQ_BTN_MATCHMAKING_2);

        public bool CanIdentifyConquestRetreatBtn()
            => ImageUtilities.CheckImageAreaSimilarity(_mappings.GetConquestBtnRetreat(), ComponentMappings.REF_CONQ_BTN_RETREAT_1) ||
               ImageUtilities.CheckImageAreaSimilarity(_mappings.GetConquestBtnRetreat(), ComponentMappings.REF_CONQ_BTN_RETREAT_2);

        public bool CanIdentifyEndTurnBtn()
            => ImageUtilities.CheckImageAreaSimilarity(_mappings.GetConquestBtnEndTurn(), ComponentMappings.REF_CONQ_BTN_END_TURN);

        public bool CanIdentifyMidTurn()
            => ImageUtilities.CheckImageAreaSimilarity(_mappings.GetConquestBtnWaiting(), ComponentMappings.REF_CONQ_BTN_WAITING, 0.85) ||
               ImageUtilities.CheckImageAreaSimilarity(_mappings.GetConquestBtnWaiting(), ComponentMappings.REF_CONQ_BTN_WAITING_2, 0.85) ||
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

        public void ResetMenu() => SystemUtilities.Click(_config.Window.Left + _config.Center, _config.Window.Bottom - _config.Scale(5));

        public void ClearError() => SystemUtilities.Click(_config.ClearErrorPoint);

        /// <summary>
        /// Simulates attempting to play four cards in your hand to random locations.
        /// </summary>
        public void PlayHand()
        {
            var rand = new Random();

            // Randomize the order in which cards are attempted to be played:
            int[] handPos = { 0, 1, 2, 3 };
            for (int i = handPos.Length - 1; i > 0; i--)
            {
                int j = rand.Next(0, i + 1);
                (handPos[j], handPos[i]) = (handPos[i], handPos[j]);
            }

            // Attempt to play cards from hand to random locations. Will exit early if zero energy is detected.
            MouseUtilities.MoveCard(_config.Cards[handPos[0]], _config.Locations[rand.Next(3)], _config.ResetPoint);
            _config.GetWindowPositions(); if (CanIdentifyZeroEnergy()) goto Exit;
            MouseUtilities.MoveCard(_config.Cards[handPos[1]], _config.Locations[rand.Next(3)], _config.ResetPoint);
            _config.GetWindowPositions(); if (CanIdentifyZeroEnergy()) goto Exit;
            MouseUtilities.MoveCard(_config.Cards[handPos[2]], _config.Locations[rand.Next(3)], _config.ResetPoint);
            _config.GetWindowPositions(); if (CanIdentifyZeroEnergy()) goto Exit;
            MouseUtilities.MoveCard(_config.Cards[handPos[3]], _config.Locations[rand.Next(3)], _config.ResetPoint);

            Exit:
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
        /// Simulates clicking the "Cancel" button during matchmaking.
        /// </summary>
        public void ClickCancel() => SystemUtilities.Click(_config.CancelPoint);

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

		/// <summary>
		/// Simulates clicks to from a match.
		/// </summary>
		public void ClickConcede()
		{
			_config.GetWindowPositions();
			SystemUtilities.Click(_config.ConcedePoint);

			Thread.Sleep(1000);
			SystemUtilities.Click(_config.ConcedeConfirmPoint);

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
            ResetClick();
            Thread.Sleep(1000);
        }

        public LeadStatus GetLeadStatus()
        {
            bool[] lanes = new bool[3];

            // Check color points of three lanes. If orange, lane is leading.
            lanes[0] = IsWinning(_config.LaneColorPoint1);
            lanes[1] = IsWinning(_config.LaneColorPoint2);
            lanes[2] = IsWinning(_config.LaneColorPoint3);

            int winning = 0;
            foreach (var lane in lanes)
                if (lane) 
                    winning++;

            // Return array of lane results
            var status = winning switch
            {
                3 => LeadStatus.WINNING_THREE,
                2 => LeadStatus.WINNING_TWO,
                1 => LeadStatus.LOSING_TWO,
                0 => LeadStatus.LOSING_THREE,
                _ => LeadStatus.LOSING_THREE
            };

            return status;
        }

        public static bool IsWinning(Point point, string imagePath = BotConfig.DefaultImageLocation)
        {
            using (Bitmap image = new Bitmap(imagePath))
            {
                // Ensure the point is within the bounds of the image
                if (point.X < 0 || point.Y < 0 || point.X >= image.Width || point.Y >= image.Height)
                    throw new ArgumentOutOfRangeException("Point is outside the bounds of the image.");

                // Get the color of the pixel at the specified point
                Color pixelColor = image.GetPixel(point.X, point.Y);
                // Console.WriteLine($"[{point.X}, {point.Y}] #{pixelColor.Name}");

                // Convert the color to HSV
                float hue = pixelColor.GetHue();
                float saturation = pixelColor.GetSaturation();
                float brightness = pixelColor.GetBrightness();

                // Define the range for the color orange
                const float orangeHueMin = 1f; // Minimum hue for orange
                const float orangeHueMax = 120f; // Maximum hue for orange

                // Check if the hue falls within the range of orange and has sufficient saturation and brightness
                bool isOrange = (hue >= orangeHueMin && hue <= orangeHueMax) &&
                                (saturation >= 0.5f) && // Adjust saturation threshold as needed
                                (brightness >= 0.3f);  // Adjust brightness threshold as needed

                return isOrange;
            }
        }
    }
}
