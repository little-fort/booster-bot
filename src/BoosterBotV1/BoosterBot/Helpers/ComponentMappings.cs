
namespace BoosterBot
{
    internal class ComponentMappings
    {
        public readonly BotConfig _config;
        private readonly string _culture;
        public string Culture { get => _culture; }
        public string REF_BTN_RECONNECT_TO_GAME { get => $"reference\\{_culture}\\btn-main-reconnect-preproc.png"; }
        public string REF_EVENT_BTN_PLAY { get => $"reference\\{_culture}\\btn-event-play-preproc.png"; }
        public string REF_EVENT_BTN_FORFEIT { get => $"reference\\{_culture}\\btn-event-forfeit-preproc.png"; }
        public string REF_LADD_BTN_PLAY { get => $"reference\\{_culture}\\btn-main-play-preproc.png"; }
        public string REF_LADD_BTN_MATCHMAKING_1 { get => $"reference\\{_culture}\\btn-ladder-matchmaking-cancel-1-preproc.png"; }
        public string REF_LADD_BTN_MATCHMAKING_2 { get => $"reference\\{_culture}\\btn-ladder-matchmaking-cancel-2-preproc.png"; }
        public string REF_LADD_BTN_RETREAT { get => $"reference\\{_culture}\\btn-ladder-retreat-preproc.png"; }
        public string REF_LADD_BTN_COLLECT_REWARDS { get => $"reference\\{_culture}\\btn-ladder-collect-rewards-preproc.png"; }
        public string REF_LADD_BTN_MATCH_END_NEXT { get => $"reference\\{_culture}\\btn-ladder-match-end-next-preproc.png"; }
        public string REF_ICON_ZERO_ENERGY { get => $"reference\\{_culture}\\lbl-energy-zero-preproc.png"; }
        public string REF_CONQ_BTN_PLAY { get => $"reference\\{_culture}\\btn-conquest-play-preproc.png"; }
        public string REF_CONQ_LBL_ENTRANCE_FEE { get => $"reference\\{_culture}\\btn-conquest-entrance-fee-preproc.png"; }
        public string REF_CONQ_LBL_NO_TICKETS { get => $"reference\\{_culture}\\lbl-conquest-no-tickets-preproc.png"; }
        public string REF_CONQ_LBL_LOBBY_PG_1 { get => $"reference\\{_culture}\\lbl-conquest-pg-1-preproc.png"; }
        public string REF_CONQ_LBL_LOBBY_PG_2 { get => $"reference\\{_culture}\\lbl-conquest-pg-2-preproc.png"; }
        public string REF_CONQ_LBL_LOBBY_PG_3 { get => $"reference\\{_culture}\\lbl-conquest-pg-3-preproc.png"; }
        public string REF_CONQ_LBL_LOBBY_SILVER_1 { get => $"reference\\{_culture}\\lbl-conquest-silver-1-preproc.png"; } // The default "Silver Conquest" label on the lobby entrance screen
        public string REF_CONQ_LBL_LOBBY_SILVER_2 { get => $"reference\\{_culture}\\lbl-conquest-silver-2-preproc.png"; } // The off-center "Silver Conquest" label that has been shifted by a UI bug
        public string REF_CONQ_LBL_LOBBY_SILVER_3 { get => $"reference\\{_culture}\\lbl-conquest-silver-3-preproc.png"; } // The in-lobby "Silver" banner that appears after starting a run
        public string REF_CONQ_LBL_LOBBY_SILVER_4 { get => $"reference\\{_culture}\\lbl-conquest-silver-4-preproc.png"; } // The "1 Gold Ticket" reward label on the lobby entrance screen
        public string REF_CONQ_LBL_LOBBY_GOLD_1 { get => $"reference\\{_culture}\\lbl-conquest-gold-1-preproc.png"; }
        public string REF_CONQ_LBL_LOBBY_GOLD_2 { get => $"reference\\{_culture}\\lbl-conquest-gold-2-preproc.png"; }
        public string REF_CONQ_LBL_LOBBY_GOLD_3 { get => $"reference\\{_culture}\\lbl-conquest-gold-3-preproc.png"; }
        public string REF_CONQ_LBL_LOBBY_GOLD_4 { get => $"reference\\{_culture}\\lbl-conquest-gold-4-preproc.png"; }
        public string REF_CONQ_LBL_LOBBY_INFINITE_1 { get => $"reference\\{_culture}\\lbl-conquest-infinite-1-preproc.png"; }
        public string REF_CONQ_LBL_LOBBY_INFINITE_2 { get => $"reference\\{_culture}\\lbl-conquest-infinite-2-preproc.png"; }
        public string REF_CONQ_LBL_LOBBY_INFINITE_3 { get => $"reference\\{_culture}\\lbl-conquest-infinite-3-preproc.png"; }
        public string REF_CONQ_LBL_LOBBY_INFINITE_4 { get => $"reference\\{_culture}\\lbl-conquest-infinite-4-preproc.png"; }
        public string REF_CONQ_BTN_MATCHMAKING_1 { get => $"reference\\{_culture}\\btn-conquest-matchmaking-cancel-1-preproc.png"; }
        public string REF_CONQ_BTN_MATCHMAKING_2 { get => $"reference\\{_culture}\\btn-conquest-matchmaking-cancel-2-preproc.png"; }
        public string REF_CONQ_BTN_RETREAT_1 { get => $"reference\\{_culture}\\btn-conquest-retreat-preproc.png"; }
        public string REF_CONQ_BTN_RETREAT_2 { get => $"reference\\{_culture}\\btn-conquest-retreat-2-preproc.png"; }
        public string REF_CONQ_BTN_END_TURN { get => $"reference\\{_culture}\\btn-conquest-end-turn-preproc.png"; }
        public string REF_CONQ_BTN_WAITING_1 { get => $"reference\\{_culture}\\btn-conquest-waiting-preproc.png"; }
        public string REF_CONQ_BTN_WAITING_2 { get => $"reference\\{_culture}\\btn-conquest-waiting-2-preproc.png"; }
        public string REF_CONQ_BTN_PLAYING { get => $"reference\\{_culture}\\btn-conquest-playing-preproc.png"; }
        public string REF_CONQ_BTN_PLAYING_2 { get => $"reference\\{_culture}\\btn-conquest-playing-2-preproc.png"; }
        public string REF_CONQ_BTN_CONCEDE_1 { get => $"reference\\{_culture}\\btn-conquest-concede-preproc.png"; }
        public string REF_CONQ_BTN_CONCEDE_2 { get => $"reference\\{_culture}\\btn-conquest-concede-2-preproc.png"; }
        public string REF_CONQ_BTN_MATCH_END_1 { get => $"reference\\{_culture}\\btn-conquest-match-end-next-1-preproc.png"; }
        public string REF_CONQ_BTN_MATCH_END_2 { get => $"reference\\{_culture}\\btn-conquest-match-end-next-2-preproc.png"; }
        public string REF_CONQ_BTN_CONTINUE { get => $"reference\\{_culture}\\btn-conquest-continue-preproc.png"; }
        public string REF_CONQ_BTN_WIN_NEXT { get => $"reference\\{_culture}\\btn-conquest-victory-next-preproc.png"; }
        public string REF_CONQ_BTN_WIN_TICKET { get => $"reference\\{_culture}\\btn-conquest-ticket-claim-preproc.png"; }
        public string REF_CONQ_BTN_WIN_TICKET_2 { get => $"reference\\{_culture}\\btn-conquest-ticket-claim-2-preproc.png"; }
        public string REF_CONQ_BTN_WIN_TICKET_3 { get => $"reference\\{_culture}\\btn-conquest-ticket-claim-3-preproc.png"; }

        public ComponentMappings(BotConfig config)
        {
            _config = config;
            _culture = config.Settings["gameLanguage"]?.Split('-')[0] ?? "en";
        }

        public Rect GetBtnPlay() => new()
        {
            Left = _config.Center - 45,
            Right = _config.Center + 40,
            Top = _config.Screencap.Height - 235,
            Bottom = _config.Screencap.Height - 195
        };
        
        public Rect GetEnergy() => new()
        {
            Left = _config.Center - 30,
            Right = _config.Center + 20,
            Top = _config.Screencap.Height - 90,
            Bottom = _config.Screencap.Height - 45
        };

        #region Ladder

        public Rect GetLadderMatchmakingCancel() => new()
        {
            Left = _config.Center - 50,
            Right = _config.Center + 40,
            Top = _config.Screencap.Height - 85,
            Bottom = _config.Screencap.Height - 55
        };

        public Rect GetLadderBtnRetreat() => new()
        {
            Left = _config.Center - 385,
            Right = _config.Center - 275,
            Top = _config.Screencap.Height - 80,
            Bottom = _config.Screencap.Height - 60
        };

        public Rect GetConquestBtnCollect() => new()
        {
            Left = _config.Center + 245,
            Right = _config.Center + 375,
            Top = _config.Screencap.Height - 80,
            Bottom = _config.Screencap.Height - 60
        };

        #endregion

        #region Conquest

        public Rect GetConquestBannerCrop() => new()
        {
            Left = _config.Center - 60,
            Right = _config.Center + 50,
            Top = 20,
            Bottom = 55
        };

        public Rect GetConquestLobbySelection() => new()
        {
            Left = _config.Center - 130,
            Right = _config.Center + 120,
            Top = 135,
            Bottom = 160
        };

        public Rect GetConquestLobbyRewardCrop() => new()
        {
            Left = _config.Center + 95,
            Right = _config.Center + 195,
            Top = 465,
            Bottom = 485
        };

        public Rect GetConquestOwnedTicketsIcon() => new()
        {
            Left = _config.Center - 85,
            Right = _config.Center + 75,
            Top = _config.Screencap.Height - 155,
            Bottom = _config.Screencap.Height - 125
        };

        public Rect GetConquestMatchmakingCancel() => new()
        {
            Left = _config.Center - 55,
            Right = _config.Center + 40,
            Top = _config.Screencap.Height - 85,
            Bottom = _config.Screencap.Height - 55
        };

        public Rect GetConquestBtnConcede() => new()
        {
            Left = _config.Center - 380,
            Right = _config.Center - 265,
            Top = _config.Screencap.Height - 80,
            Bottom = _config.Screencap.Height - 55
        };

        public Rect GetConquestBtnRetreat() => new()
        {
            Left = _config.Center - 375,
            Right = _config.Center - 285,
            Top = _config.Screencap.Height - 90,
            Bottom = _config.Screencap.Height - 70
        };

        public Rect GetConquestBtnEndTurn() => new()
        {
            Left = _config.Center + 325,
            Right = _config.Center + 370,
            Top = _config.Screencap.Height - 90,
            Bottom = _config.Screencap.Height - 70
        };

        public Rect GetConquestBtnWaiting() => new()
        {
            Left = _config.Center + 265,
            Right = _config.Center + 355,
            Top = _config.Screencap.Height - 90,
            Bottom = _config.Screencap.Height - 70
        };

        public Rect GetConquestBtnMatchEndNext1() => new()
        {
            Left = _config.Center + 285,
            Right = _config.Center + 350,
            Top = _config.Screencap.Height - 80,
            Bottom = _config.Screencap.Height - 60
        };

        public Rect GetConquestBtnMatchEndNext2() => new()
        {
            Left = _config.Center + 285,
            Right = _config.Center + 355,
            Top = _config.Screencap.Height - 70,
            Bottom = _config.Screencap.Height - 40
        };

        public Rect GetConquestBtnContinue() => new()
        {
            Left = _config.Center - 85,
            Right = _config.Center + 85,
            Top = _config.Screencap.Height - 180,
            Bottom = _config.Screencap.Height - 145
        };

        public Rect GetConquestVictoryNext() => new()
        {
            Left = _config.Center - 55,
            Right = _config.Center + 40,
            Top = _config.Screencap.Height - 180,
            Bottom = _config.Screencap.Height - 145
        };

        public Rect GetConquestTicketClaim() => new()
        {
            Left = _config.Center - 65,
            Right = _config.Center + 40,
            Top = _config.Screencap.Height - 160,
            Bottom = _config.Screencap.Height - 130
        };

        #endregion
    }
}
