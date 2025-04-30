using System.Drawing;
using System.Runtime.InteropServices;
using BoosterBot.Models;


namespace BoosterBot
{
    internal class GameUtilities(BotConfig config)
    {
        private readonly BotConfig _config = config;
        private readonly ComponentMappings _mappings = new ComponentMappings(config);
        private double _defaultConfidence = 0.95;
        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // 释放托管资源
                    if (_config is IDisposable configDisposable)
                    {
                        configDisposable.Dispose();
                    }
                    if (_mappings is IDisposable mappingsDisposable)
                    {
                        mappingsDisposable.Dispose();
                    }
                }

                _disposed = true;
            }
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
                ("LADDER_MATCHMAKING", () => CanIdentifyLadderMatchmaking()),
                ("LADDER_MATCH (Retreat button)", () => CanIdentifyLadderRetreatBtn()),
                ("LADDER_MATCH (End turn button)", () => CanIdentifyEndTurnBtn()),
                ("LADDER_MATCH (Mid turn buttons)", () => CanIdentifyMidTurn()),
                ("LADDER_MATCH_END", () => CanIdentifyLadderMatchEnd()),
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
                ("CONQUEST_ENTRANCE_FEE", () => CanIdentifyConquestEntranceFee()),
                ("CONQUEST_PREMATCH", CanIdentifyConquestPlayBtn),
                ("CONQUEST_LOBBY_PG", () => CanIdentifyConquestLobbyPG()),
                ("CONQUEST_LOBBY_SILVER", () => CanIdentifyConquestLobbySilver()),
                ("CONQUEST_LOBBY_GOLD", () => CanIdentifyConquestLobbyGold()),
                ("CONQUEST_LOBBY_INFINITE", () => CanIdentifyConquestLobbyInfinite()),
                ("CONQUEST_MATCHMAKING", () => CanIdentifyConquestMatchmaking()),
                ("CONQUEST_MATCH (Retreat button)", () => CanIdentifyConquestRetreatBtn()),
                ("CONQUEST_MATCH (End turn button)", () => CanIdentifyEndTurnBtn()),
                ("CONQUEST_MATCH (Mid turn buttons)", () => CanIdentifyMidTurn()),
                ("CONQUEST_ROUND_END", () => CanIdentifyConquestConcede()),
                ("CONQUEST_MATCH_END", () => CanIdentifyConquestMatchEnd()),
                ("CONQUEST_POSTMATCH_LOSS_SCREEN", CanIdentifyConquestLossContinue),
                ("CONQUEST_POSTMATCH_WIN_CONTINUE", CanIdentifyConquestWinNext),
                ("CONQUEST_POSTMATCH_WIN_TICKET", () => CanIdentifyConquestTicketClaim())
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
        private IdentificationResult CheckSimilarity(Func<Rect> getAreaFunc, string referenceImagePath, double threshold = 0.95, bool cropOnly = false)
        {
            var area = getAreaFunc();
            if (area.Left >= area.Right || area.Top >= area.Bottom)
            {
                return new IdentificationResult(false, new List<string> { $"无效的矩形区域: {area}" });
            }
            if (threshold != _defaultConfidence)
                threshold = _defaultConfidence;
            return ImageUtilities.CheckImageAreaSimilarity(area, referenceImagePath, threshold, cropOnly: cropOnly);
        }
        private IdentificationResult CheckMultipleSimilarities(bool returnFirstFound = true, params (Func<Rect> GetAreaFunc, string ReferenceImagePath, double Threshold)[] checks)
        {
            var combinedLogs = new List<string>();
            foreach (var (getAreaFunc, referenceImagePath, threshold) in checks)
            {
                var result = CheckSimilarity(getAreaFunc, referenceImagePath, threshold);
                combinedLogs.AddRange(result.Logs);
                if (result.IsMatch && returnFirstFound)
                    return new IdentificationResult(true, combinedLogs);
            }
            return new IdentificationResult(false, combinedLogs);
        }
        private IdentificationResult CheckSequentially(bool returnFirstFound = true, params Func<bool, IdentificationResult>[] identificationMethods)
        {
            var combinedLogs = new List<string>();

            foreach (var method in identificationMethods)
            {
                var result = method(returnFirstFound);
                combinedLogs.AddRange(result.Logs);

                if (result.IsMatch && returnFirstFound)
                    return new IdentificationResult(true, combinedLogs);
            }

            // No match found; return false with combined logs
            return new IdentificationResult(false, combinedLogs);
        }
        #endregion
        #region Core UI

        public IdentificationResult CanIdentifyMainMenu()
            => CheckSimilarity(_mappings.GetBtnPlay, _mappings.REF_LADD_BTN_PLAY);

        public IdentificationResult CanIdentifyReconnectToGameBtn()
            => CheckSimilarity(_mappings.GetBtnPlay, _mappings.REF_BTN_RECONNECT_TO_GAME);

        public IdentificationResult CanIdentifyZeroEnergy()
            => CheckSimilarity(_mappings.GetEnergy, _mappings.REF_ICON_ZERO_ENERGY, 0.925);

        #endregion
        public IdentificationResult RunFullIdentifySequence(Func<bool, IdentificationResult> func, bool returnFirstFound = false)
            => func(returnFirstFound);
        #region Event

        public IdentificationResult CanIdentifyEventMenu()
            => CheckSimilarity(_mappings.GetBtnPlay, _mappings.REF_EVENT_BTN_PLAY, 0.85);

        public IdentificationResult CanIdentifyEventForfeitBtn(bool returnFirstFound = true)
            => CheckSimilarity(_mappings.GetLadderBtnRetreat, _mappings.REF_EVENT_BTN_FORFEIT);

        public IdentificationResult CanIdentifyActiveEventMatch(bool returnFirstFound = true)
            => CheckSequentially(returnFirstFound, CanIdentifyEventForfeitBtn, CanIdentifyEndTurnBtn, CanIdentifyMidTurn);

        #endregion
        #region Ladder

        public IdentificationResult CanIdentifyActiveLadderMatch(bool returnFirstFound = true)
            => CheckSequentially(returnFirstFound, CanIdentifyLadderRetreatBtn, CanIdentifyEndTurnBtn, CanIdentifyMidTurn);

        public IdentificationResult CanIdentifyLadderMatchmaking(bool returnFirstFound = true)
            => CheckMultipleSimilarities(returnFirstFound,
                    (_mappings.GetLadderMatchmakingCancel, _mappings.REF_LADD_BTN_MATCHMAKING_1, _defaultConfidence),
                    (_mappings.GetLadderMatchmakingCancel, _mappings.REF_LADD_BTN_MATCHMAKING_2, _defaultConfidence)
                );

        public IdentificationResult CanIdentifyLadderRetreatBtn(bool returnFirstFound = true)
            => CheckSimilarity(_mappings.GetLadderBtnRetreat, _mappings.REF_LADD_BTN_RETREAT);

        public IdentificationResult CanIdentifyLadderCollectRewardsBtn(bool returnFirstFound = true)
            => CheckSimilarity(_mappings.GetConquestBtnCollect, _mappings.REF_LADD_BTN_COLLECT_REWARDS);

        public IdentificationResult CanIdentifyLadderMatchEndNextBtn(bool returnFirstFound = true)
            => CheckSimilarity(_mappings.GetConquestBtnMatchEndNext2, _mappings.REF_LADD_BTN_MATCH_END_NEXT);

        public IdentificationResult CanIdentifyLadderMatchEnd(bool returnFirstFound = true)
            => CheckSequentially(returnFirstFound, CanIdentifyLadderCollectRewardsBtn, CanIdentifyLadderMatchEndNextBtn);

        #endregion
        #region Conquest

        public IdentificationResult CanIdentifyActiveConquestMatch()
            => CheckSequentially(returnFirstFound: true, CanIdentifyConquestRetreatBtn, CanIdentifyEndTurnBtn, CanIdentifyMidTurn);

        public IdentificationResult CanIdentifyConquestPlayBtn()
            => CheckSimilarity(_mappings.GetBtnPlay, _mappings.REF_CONQ_BTN_PLAY);

        public IdentificationResult CanIdentifyAnyConquestLobby()
            => CheckSequentially(returnFirstFound: true, CanIdentifyConquestLobbyPG, CanIdentifyConquestLobbySilver, CanIdentifyConquestLobbyGold, CanIdentifyConquestLobbyInfinite);

        public IdentificationResult CanIdentifyConquestEntranceFee(bool returnFirstFound = true)
            => CheckSimilarity(_mappings.GetConquestVictoryNext, _mappings.REF_CONQ_LBL_ENTRANCE_FEE);

        public IdentificationResult CanIdentifyConquestNoTickets()
            => CheckSimilarity(_mappings.GetConquestOwnedTicketsIcon, _mappings.REF_CONQ_LBL_NO_TICKETS);

        public IdentificationResult CanIdentifyConquestLobbyPG(bool returnFirstFound = true)
            => CheckMultipleSimilarities(returnFirstFound,
                    (_mappings.GetConquestLobbyRewardCrop, _mappings.REF_CONQ_LBL_LOBBY_PG_3, _defaultConfidence),
                    (_mappings.GetConquestLobbySelection, _mappings.REF_CONQ_LBL_LOBBY_PG_1, _defaultConfidence),
                    (_mappings.GetConquestBannerCrop, _mappings.REF_CONQ_LBL_LOBBY_PG_2, _defaultConfidence)
                );

        public IdentificationResult CanIdentifyConquestLobbySilver(bool returnFirstFound = true)
            => CheckMultipleSimilarities(returnFirstFound,
                    (_mappings.GetConquestLobbyRewardCrop, _mappings.REF_CONQ_LBL_LOBBY_SILVER_4, _defaultConfidence),
                    (_mappings.GetConquestLobbySelection, _mappings.REF_CONQ_LBL_LOBBY_SILVER_1, _defaultConfidence),
                    (_mappings.GetConquestLobbySelection, _mappings.REF_CONQ_LBL_LOBBY_SILVER_2, _defaultConfidence),
                    (_mappings.GetConquestBannerCrop, _mappings.REF_CONQ_LBL_LOBBY_SILVER_3, _defaultConfidence)
                );

        public IdentificationResult CanIdentifyConquestLobbyGold(bool returnFirstFound = true)
            => CheckMultipleSimilarities(returnFirstFound,
                    (_mappings.GetConquestLobbyRewardCrop, _mappings.REF_CONQ_LBL_LOBBY_GOLD_4, _defaultConfidence),
                    (_mappings.GetConquestLobbySelection, _mappings.REF_CONQ_LBL_LOBBY_GOLD_1, _defaultConfidence),
                    (_mappings.GetConquestLobbySelection, _mappings.REF_CONQ_LBL_LOBBY_GOLD_2, _defaultConfidence),
                    (_mappings.GetConquestBannerCrop, _mappings.REF_CONQ_LBL_LOBBY_GOLD_3, _defaultConfidence)
                );

        public IdentificationResult CanIdentifyConquestLobbyInfinite(bool returnFirstFound = true)
            => CheckMultipleSimilarities(returnFirstFound,
                    (_mappings.GetConquestLobbyRewardCrop, _mappings.REF_CONQ_LBL_LOBBY_INFINITE_4, _defaultConfidence),
                    (_mappings.GetConquestLobbySelection, _mappings.REF_CONQ_LBL_LOBBY_INFINITE_1, _defaultConfidence),
                    (_mappings.GetConquestLobbySelection, _mappings.REF_CONQ_LBL_LOBBY_INFINITE_2, _defaultConfidence),
                    (_mappings.GetConquestBannerCrop, _mappings.REF_CONQ_LBL_LOBBY_INFINITE_3, _defaultConfidence)
                );

        public IdentificationResult CanIdentifyConquestMatchmaking(bool returnFirstFound = true)
            => CheckMultipleSimilarities(returnFirstFound,
                    (_mappings.GetConquestMatchmakingCancel, _mappings.REF_CONQ_BTN_MATCHMAKING_1, _defaultConfidence),
                    (_mappings.GetConquestMatchmakingCancel, _mappings.REF_CONQ_BTN_MATCHMAKING_2, _defaultConfidence)
                );

        public IdentificationResult CanIdentifyConquestRetreatBtn(bool returnFirstFound = true)
            => CheckMultipleSimilarities(returnFirstFound,
                    (_mappings.GetConquestBtnRetreat, _mappings.REF_CONQ_BTN_RETREAT_1, _defaultConfidence),
                    (_mappings.GetConquestBtnRetreat, _mappings.REF_CONQ_BTN_RETREAT_2, _defaultConfidence)
                );

        public IdentificationResult CanIdentifyEndTurnBtn(bool returnFirstFound = true)
            => CheckSimilarity(_mappings.GetConquestBtnEndTurn, _mappings.REF_CONQ_BTN_END_TURN);

        public IdentificationResult CanIdentifyMidTurn(bool returnFirstFound = true)
            => CheckMultipleSimilarities(returnFirstFound,
                    (_mappings.GetConquestBtnWaiting, _mappings.REF_CONQ_BTN_WAITING_1, _defaultConfidence),
                    (_mappings.GetConquestBtnWaiting, _mappings.REF_CONQ_BTN_WAITING_2, _defaultConfidence),
                    (_mappings.GetConquestBtnWaiting, _mappings.REF_CONQ_BTN_PLAYING, _defaultConfidence)
                );

        public IdentificationResult CanIdentifyConquestConcede(bool returnFirstFound = true)
            => CheckMultipleSimilarities(returnFirstFound,
                    (_mappings.GetConquestBtnConcede, _mappings.REF_CONQ_BTN_CONCEDE_1, _defaultConfidence),
                    (_mappings.GetConquestBtnConcede, _mappings.REF_CONQ_BTN_CONCEDE_2, _defaultConfidence)
                );

        public IdentificationResult CanIdentifyConquestMatchEnd(bool returnFirstFound = true)
            => CheckSequentially(returnFirstFound, CanIdentifyConquestMatchEndNext1, CanIdentifyConquestMatchEndNext2);

        public IdentificationResult CanIdentifyConquestMatchEndNext1(bool returnFirstFound = true)
            => CheckSimilarity(_mappings.GetConquestBtnMatchEndNext1, _mappings.REF_CONQ_BTN_MATCH_END_1);

        public IdentificationResult CanIdentifyConquestMatchEndNext2(bool returnFirstFound = true)
            => CheckSimilarity(_mappings.GetConquestBtnMatchEndNext2, _mappings.REF_CONQ_BTN_MATCH_END_2);

        public IdentificationResult CanIdentifyConquestLossContinue()
            => CheckSimilarity(_mappings.GetConquestBtnContinue, _mappings.REF_CONQ_BTN_CONTINUE);

        public IdentificationResult CanIdentifyConquestWinNext()
            => CheckSimilarity(_mappings.GetConquestVictoryNext, _mappings.REF_CONQ_BTN_WIN_NEXT);

        public IdentificationResult CanIdentifyConquestTicketClaim(bool returnFirstFound = true)
            => CheckMultipleSimilarities(returnFirstFound,
                    (_mappings.GetConquestTicketClaim, _mappings.REF_CONQ_BTN_WIN_TICKET, 0.8),
                    (_mappings.GetConquestTicketClaim, _mappings.REF_CONQ_BTN_WIN_TICKET_2, 0.8),
                    (_mappings.GetConquestTicketClaim, _mappings.REF_CONQ_BTN_WIN_TICKET_3, 0.8)
                );

        #endregion
        public IdentificationResult CanIdentifyExperienceClaim(bool returnFirstFound = true)
            => CheckMultipleSimilarities(returnFirstFound,
                    (_mappings.GetExperienceClaim, _mappings.REF_EXP_BTN_CLAIM_1, 0.9),
                    (_mappings.GetExperienceClaim, _mappings.REF_EXP_BTN_CLAIM_2, 0.9)
            );
        public void ResetClick() => SystemUtilities.Click(_config.ResetPoint);
        public void ResetMenu() => SystemUtilities.Click(_config.Window.Left + _config.Center, _config.Window.Bottom - _config.Scale(5));
        public void ClearError() => SystemUtilities.Click(_config.ClearErrorPoint);
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

        public bool ClickSnap()
        {
            SystemUtilities.Click(_config.SnapPoint);
            Logger.Log(_config.Localizer, "Log_OhSnap", _config.LogPath);
            return true;
        }


        public void ClickPlay() => SystemUtilities.Click(_config.PlayPoint);
        public void ClickCancel() => SystemUtilities.Click(_config.CancelPoint);
        public void ClickNext() => SystemUtilities.Click(_config.NextPoint);
        public void ClickClaim() => SystemUtilities.Click(_config.ClaimPoint);
        public void ClickExperienceClaim()
        {
            SystemUtilities.Click(_config.ExperienceClaimPoint);
            Thread.Sleep(800);
        }

        public void ClickRetreat()
        {
            _config.GetWindowPositions();
            SystemUtilities.Click(_config.RetreatPoint);

            Thread.Sleep(1000);
            SystemUtilities.Click(_config.RetreatConfirmPoint);

            Thread.Sleep(5000);
        }
        public void ClickConcede()
        {
            _config.GetWindowPositions();
            SystemUtilities.Click(_config.ConcedePoint);

            Thread.Sleep(1000);
            SystemUtilities.Click(_config.ConcedeConfirmPoint);

            Thread.Sleep(5000);
        }
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        public void PressEscKey()
        {
            const byte VK_ESCAPE = 0x1B;
            const int KEYEVENTF_KEYDOWN = 0x0000;
            const int KEYEVENTF_KEYUP = 0x0002;

            // 按下 ESC
            keybd_event(VK_ESCAPE, 0, KEYEVENTF_KEYDOWN, 0);
            // 释放 ESC
            keybd_event(VK_ESCAPE, 0, KEYEVENTF_KEYUP, 0);
        }

        public void BlindReset()
        {
            _config.GetWindowPositions();
            var rand = new Random();
            Logger.Log(_config.Localizer, "Log_BlindReset", _config.LogPath);
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
            if (_disposed)
                throw new ObjectDisposedException(nameof(GameUtilities));

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
                Console.WriteLine($"[{point.X}, {point.Y}] #{pixelColor.Name}");

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