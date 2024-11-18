using BoosterBot.Models;
using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace BoosterBot
{
    internal class GameUtilities
    {
        private readonly BotConfig _config;
        private readonly ComponentMappings _mappings;
        private double _defaultConfidence = 0.95;

        public GameUtilities(BotConfig config)
        {
            _config = config;
            _mappings = new ComponentMappings(config);
        }

        public void SetDefaultConfidence(double confidence) => _defaultConfidence = confidence;

        private (List<string> Logs, List<string> Results) ProcessChecks(List<(string Description, Func<IdentificationResult> IdentifyMethod)> checks)
        {
            var logs = new List<string>();
            var results = new List<string>();

            foreach (var (description, identifyMethod) in checks)
            {
                var id = identifyMethod();
                logs.AddRange(id.Logs);
                logs.Add("------------");
                results.Add($"{(id.IsMatch ? "X" : " ")} {description}");
            }

            return (logs, results);
        }

        public (List<string> Logs, List<string> Results) LogLadderGameState()
        {
            var logs = new List<string>();
            var results = new List<string>();
            _config.GetWindowPositions();

            var checks = new List<(string Description, Func<IdentificationResult> IdentifyMethod)>
            {
                ("MAIN_MENU", CanIdentifyMainMenu),
                ("MID_MATCH", CanIdentifyZeroEnergy),
                ("LADDER_MATCHMAKING", CanIdentifyLadderMatchmaking),
                ("LADDER_MATCH (Retreat button)", CanIdentifyLadderRetreatBtn),
                ("LADDER_MATCH (End turn button)", CanIdentifyEndTurnBtn),
                ("LADDER_MATCH (Mid turn buttons)", CanIdentifyMidTurn),
                ("LADDER_MATCH_END", CanIdentifyLadderMatchEnd),
                ("EVENT_MENU", CanIdentifyEventMenu)
            };

            return ProcessChecks(checks);
        }

        public (List<string> Logs, List<string> Results) LogConquestGameState()
        {
            var logs = new List<string>();
            var results = new List<string>();
            _config.GetWindowPositions();

            var checks = new List<(string Description, Func<IdentificationResult> IdentifyMethod)>
            {
                ("MAIN_MENU", CanIdentifyMainMenu),
                ("RECONNECT_TO_GAME", CanIdentifyReconnectToGameBtn),
                ("MID_MATCH", CanIdentifyZeroEnergy),
                ("CONQUEST_NO_TICKETS", CanIdentifyConquestNoTickets),
                ("CONQUEST_PREMATCH", CanIdentifyConquestPlayBtn),
                ("CONQUEST_LOBBY_PG", CanIdentifyConquestLobbyPG),
                ("CONQUEST_LOBBY_SILVER", CanIdentifyConquestLobbySilver),
                ("CONQUEST_LOBBY_GOLD", CanIdentifyConquestLobbyGold),
                ("CONQUEST_LOBBY_INFINITE", CanIdentifyConquestLobbyInfinite),
                ("CONQUEST_MATCHMAKING", CanIdentifyConquestMatchmaking),
                ("CONQUEST_MATCH (Retreat button)", CanIdentifyConquestRetreatBtn),
                ("CONQUEST_MATCH (End turn button)", CanIdentifyEndTurnBtn),
                ("CONQUEST_MATCH (Mid turn buttons)", CanIdentifyMidTurn),
                ("CONQUEST_ROUND_END", CanIdentifyConquestConcede),
                ("CONQUEST_MATCH_END", CanIdentifyConquestMatchEnd),
                ("CONQUEST_POSTMATCH_LOSS_SCREEN", CanIdentifyConquestLossContinue),
                ("CONQUEST_POSTMATCH_WIN_CONTINUE", CanIdentifyConquestWinNext),
                ("CONQUEST_POSTMATCH_WIN_TICKET", CanIdentifyConquestTicketClaim)
            };

            return ProcessChecks(checks);
        }

        public GameState DetermineLadderGameState(bool checkEvent = false)
        {
            _config.GetWindowPositions();

            if (CanIdentifyMainMenu().IsMatch) return GameState.MAIN_MENU;
            if (checkEvent && CanIdentifyEventMenu().IsMatch) return GameState.EVENT_MENU;
            if (CanIdentifyReconnectToGameBtn().IsMatch) return GameState.RECONNECT_TO_GAME;
            if (CanIdentifyLadderMatchmaking().IsMatch) return GameState.LADDER_MATCHMAKING;
            if (CanIdentifyLadderRetreatBtn().IsMatch) return GameState.LADDER_MATCH;
            if (checkEvent && CanIdentifyEventForfeitBtn().IsMatch) return GameState.LADDER_MATCH;
            if (CanIdentifyEndTurnBtn().IsMatch) return GameState.LADDER_MATCH;
            if (CanIdentifyMidTurn().IsMatch) return GameState.LADDER_MATCH;
            if (CanIdentifyLadderMatchEnd().IsMatch) return GameState.LADDER_MATCH_END;
            if (CanIdentifyZeroEnergy().IsMatch) return GameState.MID_MATCH;
            if (CanIdentifyConquestLobbyPG().IsMatch) return GameState.CONQUEST_LOBBY_PG;

            return GameState.UNKNOWN;
        }

        public GameState DetermineConquestGameState()
        {
            _config.GetWindowPositions();

            if (CanIdentifyMainMenu().IsMatch) return GameState.MAIN_MENU;
            if (CanIdentifyReconnectToGameBtn().IsMatch) return GameState.RECONNECT_TO_GAME;
            if (CanIdentifyConquestPlayBtn().IsMatch) return GameState.CONQUEST_PREMATCH;
            if (CanIdentifyConquestLobbyPG().IsMatch) return GameState.CONQUEST_LOBBY_PG;
            if (CanIdentifyConquestMatchmaking().IsMatch) return GameState.CONQUEST_MATCHMAKING;
            if (CanIdentifyConquestRetreatBtn().IsMatch) return GameState.CONQUEST_MATCH;
            if (CanIdentifyEndTurnBtn().IsMatch) return GameState.CONQUEST_MATCH;
            if (CanIdentifyMidTurn().IsMatch) return GameState.CONQUEST_MATCH;
            if (CanIdentifyConquestConcede().IsMatch) return GameState.CONQUEST_ROUND_END;
            if (CanIdentifyConquestMatchEnd().IsMatch) return GameState.CONQUEST_MATCH_END;
            if (CanIdentifyConquestLossContinue().IsMatch) return GameState.CONQUEST_POSTMATCH_LOSS_SCREEN;
            if (CanIdentifyConquestWinNext().IsMatch) return GameState.CONQUEST_POSTMATCH_WIN_CONTINUE;
            if (CanIdentifyConquestTicketClaim().IsMatch) return GameState.CONQUEST_POSTMATCH_WIN_TICKET;
            if (CanIdentifyZeroEnergy().IsMatch) return GameState.MID_MATCH;

            return GameState.UNKNOWN;
        }

        public GameState DetermineConquestLobbyTier()
        {
            _config.GetWindowPositions();

            if (CanIdentifyConquestLobbyPG().IsMatch) return GameState.CONQUEST_LOBBY_PG;
            if (CanIdentifyConquestLobbySilver().IsMatch) return GameState.CONQUEST_LOBBY_SILVER;
            if (CanIdentifyConquestLobbyGold().IsMatch) return GameState.CONQUEST_LOBBY_GOLD;
            if (CanIdentifyConquestLobbyInfinite().IsMatch) return GameState.CONQUEST_LOBBY_INFINITE;

            return GameState.UNKNOWN;
        }

        #region Check Helpers

        private IdentificationResult CheckSimilarity(Func<Rect> getAreaFunc, string referenceImagePath, double threshold = 0.95)
        {
            var area = getAreaFunc();
            if (threshold != _defaultConfidence)
                threshold = _defaultConfidence;
            return ImageUtilities.CheckImageAreaSimilarity(area, referenceImagePath, threshold);
        }

        private IdentificationResult CheckMultipleSimilarities(params (Func<Rect> GetAreaFunc, string ReferenceImagePath, double Threshold)[] checks)
        {
            var combinedLogs = new List<string>();
            foreach (var (getAreaFunc, referenceImagePath, threshold) in checks)
            {
                var result = CheckSimilarity(getAreaFunc, referenceImagePath, threshold);
                combinedLogs.AddRange(result.Logs);
                if (result.IsMatch)
                    return new IdentificationResult(true, combinedLogs);
            }
            return new IdentificationResult(false, combinedLogs);
        }

        private IdentificationResult CheckSequentially(params Func<IdentificationResult>[] identificationMethods)
        {
            var combinedLogs = new List<string>();

            foreach (var method in identificationMethods)
            {
                var result = method();
                combinedLogs.AddRange(result.Logs);

                if (result.IsMatch)
                    return new IdentificationResult(true, combinedLogs);
            }

            // No match found; return false with combined logs
            return new IdentificationResult(false, combinedLogs);
        }

        #endregion

        #region Core UI

        public IdentificationResult CanIdentifyMainMenu()
            => CheckSimilarity(_mappings.GetBtnPlay, ComponentMappings.REF_LADD_BTN_PLAY);

        public IdentificationResult CanIdentifyReconnectToGameBtn()
            => CheckSimilarity(_mappings.GetBtnPlay, ComponentMappings.REF_BTN_RECONNECT_TO_GAME);

        public IdentificationResult CanIdentifyZeroEnergy()
            => CheckSimilarity(_mappings.GetEnergy, ComponentMappings.REF_ICON_ZERO_ENERGY, 0.925);

        #endregion

        #region Event

        public IdentificationResult CanIdentifyEventMenu()
            => CheckSimilarity(_mappings.GetBtnPlay, ComponentMappings.REF_EVENT_BTN_PLAY, 0.85);

        public IdentificationResult CanIdentifyEventForfeitBtn()
            => CheckSimilarity(_mappings.GetLadderBtnRetreat, ComponentMappings.REF_EVENT_BTN_FORFEIT);

        public IdentificationResult CanIdentifyActiveEventMatch()
            => CheckSequentially(CanIdentifyEventForfeitBtn, CanIdentifyEndTurnBtn, CanIdentifyMidTurn);

        #endregion

        #region Ladder

        public IdentificationResult CanIdentifyActiveLadderMatch()
            => CheckSequentially(CanIdentifyLadderRetreatBtn, CanIdentifyEndTurnBtn, CanIdentifyMidTurn);

        public IdentificationResult CanIdentifyLadderMatchmaking()
            => CheckMultipleSimilarities(
                    (_mappings.GetLadderMatchmakingCancel, ComponentMappings.REF_LADD_BTN_MATCHMAKING_1, _defaultConfidence),
                    (_mappings.GetLadderMatchmakingCancel, ComponentMappings.REF_LADD_BTN_MATCHMAKING_2, _defaultConfidence)
                );

        public IdentificationResult CanIdentifyLadderRetreatBtn()
            => CheckSimilarity(_mappings.GetLadderBtnRetreat, ComponentMappings.REF_LADD_BTN_RETREAT);

        public IdentificationResult CanIdentifyLadderCollectRewardsBtn()
            => CheckSimilarity(_mappings.GetConquestBtnCollect, ComponentMappings.REF_LADD_BTN_COLLECT_REWARDS);

        public IdentificationResult CanIdentifyLadderMatchEndNextBtn()
            => CheckSimilarity(_mappings.GetConquestBtnMatchEndNext2, ComponentMappings.REF_LADD_BTN_MATCH_END_NEXT);

        public IdentificationResult CanIdentifyLadderMatchEnd()
            => CheckSequentially(CanIdentifyLadderCollectRewardsBtn, CanIdentifyLadderMatchEndNextBtn);

        #endregion

        #region Conquest

        public IdentificationResult CanIdentifyActiveConquestMatch()
            => CheckSequentially(CanIdentifyConquestRetreatBtn, CanIdentifyEndTurnBtn, CanIdentifyMidTurn);

        public IdentificationResult CanIdentifyConquestPlayBtn()
            => CheckSimilarity(_mappings.GetBtnPlay, ComponentMappings.REF_CONQ_BTN_PLAY);

        public IdentificationResult CanIdentifyAnyConquestLobby()
            => CheckSequentially(CanIdentifyConquestLobbyPG, CanIdentifyConquestLobbySilver, CanIdentifyConquestLobbyGold, CanIdentifyConquestLobbyInfinite);

        public IdentificationResult CanIdentifyConquestNoTickets()
            => CheckSimilarity(_mappings.GetConquestOwnedTicketsIcon, ComponentMappings.REF_CONQ_LBL_NO_TICKETS);

        public IdentificationResult CanIdentifyConquestLobbyPG()
            => CheckMultipleSimilarities(
                    (_mappings.GetConquestLobbySelection, ComponentMappings.REF_CONQ_LBL_LOBBY_PG_1, _defaultConfidence),
                    (_mappings.GetConquestBannerCrop, ComponentMappings.REF_CONQ_LBL_LOBBY_PG_2, _defaultConfidence)
                );

        public IdentificationResult CanIdentifyConquestLobbySilver()
            => CheckMultipleSimilarities(
                    (_mappings.GetConquestLobbySelection, ComponentMappings.REF_CONQ_LBL_LOBBY_SILVER_1, _defaultConfidence),
                    (_mappings.GetConquestLobbySelection, ComponentMappings.REF_CONQ_LBL_LOBBY_SILVER_2, _defaultConfidence),
                    (_mappings.GetConquestBannerCrop, ComponentMappings.REF_CONQ_LBL_LOBBY_SILVER_3, _defaultConfidence)
                );

        public IdentificationResult CanIdentifyConquestLobbyGold()
            => CheckMultipleSimilarities(
                    (_mappings.GetConquestLobbySelection, ComponentMappings.REF_CONQ_LBL_LOBBY_GOLD_1, _defaultConfidence),
                    (_mappings.GetConquestLobbySelection, ComponentMappings.REF_CONQ_LBL_LOBBY_GOLD_2, _defaultConfidence),
                    (_mappings.GetConquestBannerCrop, ComponentMappings.REF_CONQ_LBL_LOBBY_GOLD_3, _defaultConfidence)
                );

        public IdentificationResult CanIdentifyConquestLobbyInfinite()
            => CheckMultipleSimilarities(
                    (_mappings.GetConquestLobbySelection, ComponentMappings.REF_CONQ_LBL_LOBBY_INFINITE_1, _defaultConfidence),
                    (_mappings.GetConquestLobbySelection, ComponentMappings.REF_CONQ_LBL_LOBBY_INFINITE_2, _defaultConfidence),
                    (_mappings.GetConquestBannerCrop, ComponentMappings.REF_CONQ_LBL_LOBBY_INFINITE_3, _defaultConfidence)
                );

        public IdentificationResult CanIdentifyConquestMatchmaking()
            => CheckMultipleSimilarities(
                    (_mappings.GetConquestMatchmakingCancel, ComponentMappings.REF_CONQ_BTN_MATCHMAKING_1, _defaultConfidence),
                    (_mappings.GetConquestMatchmakingCancel, ComponentMappings.REF_CONQ_BTN_MATCHMAKING_2, _defaultConfidence)
                );

        public IdentificationResult CanIdentifyConquestRetreatBtn()
            => CheckMultipleSimilarities(
                    (_mappings.GetConquestBtnRetreat, ComponentMappings.REF_CONQ_BTN_RETREAT_1, _defaultConfidence),
                    (_mappings.GetConquestBtnRetreat, ComponentMappings.REF_CONQ_BTN_RETREAT_2, _defaultConfidence)
                );

        public IdentificationResult CanIdentifyEndTurnBtn()
            => CheckSimilarity(_mappings.GetConquestBtnEndTurn, ComponentMappings.REF_CONQ_BTN_END_TURN);

        public IdentificationResult CanIdentifyMidTurn()
            => CheckMultipleSimilarities(
                    (_mappings.GetConquestBtnWaiting, ComponentMappings.REF_CONQ_BTN_WAITING, _defaultConfidence),
                    (_mappings.GetConquestBtnWaiting, ComponentMappings.REF_CONQ_BTN_WAITING_2, _defaultConfidence),
                    (_mappings.GetConquestBtnWaiting, ComponentMappings.REF_CONQ_BTN_PLAYING, _defaultConfidence)
                );

        public IdentificationResult CanIdentifyConquestConcede()
            => CheckMultipleSimilarities(
                    (_mappings.GetConquestBtnConcede, ComponentMappings.REF_CONQ_BTN_CONCEDE_1, _defaultConfidence),
                    (_mappings.GetConquestBtnConcede, ComponentMappings.REF_CONQ_BTN_CONCEDE_2, _defaultConfidence)
                );

        public IdentificationResult CanIdentifyConquestMatchEnd()
            => CheckSequentially(CanIdentifyConquestMatchEndNext1, CanIdentifyConquestMatchEndNext2);

        public IdentificationResult CanIdentifyConquestMatchEndNext1()
            => CheckSimilarity(_mappings.GetConquestBtnMatchEndNext1, ComponentMappings.REF_CONQ_BTN_MATCH_END_1);

        public IdentificationResult CanIdentifyConquestMatchEndNext2()
            => CheckSimilarity(_mappings.GetConquestBtnMatchEndNext2, ComponentMappings.REF_CONQ_BTN_MATCH_END_2);

        public IdentificationResult CanIdentifyConquestLossContinue()
            => CheckSimilarity(_mappings.GetConquestBtnContinue, ComponentMappings.REF_CONQ_BTN_CONTINUE);

        public IdentificationResult CanIdentifyConquestWinNext()
            => CheckSimilarity(_mappings.GetConquestVictoryNext, ComponentMappings.REF_CONQ_BTN_WIN_NEXT);

        public IdentificationResult CanIdentifyConquestTicketClaim()
            => CheckMultipleSimilarities(
                    (_mappings.GetConquestTicketClaim, ComponentMappings.REF_CONQ_BTN_WIN_TICKET, 0.8),
                    (_mappings.GetConquestTicketClaim, ComponentMappings.REF_CONQ_BTN_WIN_TICKET_2, 0.8),
                    (_mappings.GetConquestTicketClaim, ComponentMappings.REF_CONQ_BTN_WIN_TICKET_3, 0.8)
                );

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
            _config.GetWindowPositions(); if (CanIdentifyZeroEnergy().IsMatch) goto Exit;
            MouseUtilities.MoveCard(_config.Cards[handPos[1]], _config.Locations[rand.Next(3)], _config.ResetPoint);
            _config.GetWindowPositions(); if (CanIdentifyZeroEnergy().IsMatch) goto Exit;
            MouseUtilities.MoveCard(_config.Cards[handPos[2]], _config.Locations[rand.Next(3)], _config.ResetPoint);
            _config.GetWindowPositions(); if (CanIdentifyZeroEnergy().IsMatch) goto Exit;
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
        /// Simulates clicking the "Claim" button after winning a Conquest match.
        /// </summary>
        public void ClickClaim() => SystemUtilities.Click(_config.ClaimPoint);

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
            var rand = new Random();
            Logger.Log("Attempting blind reset clicks...", _config.LogPath);
            ResetClick();
            Thread.Sleep(rand.Next(500, 750));
            ClearError();
            Thread.Sleep(rand.Next(500, 750));
            ClickNext();
            Thread.Sleep(rand.Next(500, 750));
            ClearError();
            Thread.Sleep(rand.Next(500, 750));
            ResetClick();
            Thread.Sleep(rand.Next(500, 750));
            PressEscKey();
            Thread.Sleep(rand.Next(500, 750));
            PressEscKey();
            Thread.Sleep(rand.Next(500, 750));
            ResetClick();
            Thread.Sleep(rand.Next(500, 750));
            ClearError();
            Thread.Sleep(rand.Next(500, 750));
            ClearError();
            Thread.Sleep(rand.Next(500, 750));
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
