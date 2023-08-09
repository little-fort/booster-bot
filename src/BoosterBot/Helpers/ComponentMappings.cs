
namespace BoosterBot
{
    internal class ComponentMappings
    {
        public const string REF_BTN_RECONNECT_TO_GAME = "reference\\btn-main-reconnect-preproc.png";
        public const string REF_LADD_BTN_PLAY = "reference\\btn-main-play-preproc.png";
        public const string REF_LADD_BTN_MATCHMAKING_1 = "reference\\btn-ladder-matchmaking-cancel-1-preproc.png";
        public const string REF_LADD_BTN_MATCHMAKING_2 = "reference\\btn-ladder-matchmaking-cancel-2-preproc.png";
        public const string REF_LADD_BTN_RETREAT = "reference\\btn-ladder-retreat-preproc.png";
        public const string REF_LADD_BTN_COLLECT_REWARDS = "reference\\btn-ladder-collect-rewards-preproc.png";
        public const string REF_LADD_BTN_MATCH_END_NEXT = "reference\\btn-ladder-match-end-next-preproc.png";
        public const string REF_ICON_ZERO_ENERGY = "reference\\lbl-energy-zero-preproc.png";
        public const string REF_CONQ_BTN_PLAY = "reference\\btn-conquest-play-preproc.png";
        public const string REF_CONQ_LBL_NO_TICKETS = "reference\\lbl-conquest-no-tickets-preproc.png";
        public const string REF_CONQ_LBL_LOBBY_PG_1 = "reference\\lbl-conquest-pg-1-preproc.png";
        public const string REF_CONQ_LBL_LOBBY_PG_2 = "reference\\lbl-conquest-pg-2-preproc.png";
        public const string REF_CONQ_LBL_LOBBY_SILVER_1 = "reference\\lbl-conquest-silver-1-preproc.png";
        public const string REF_CONQ_LBL_LOBBY_SILVER_2 = "reference\\lbl-conquest-silver-2-preproc.png";
        public const string REF_CONQ_LBL_LOBBY_SILVER_3 = "reference\\lbl-conquest-silver-3-preproc.png";
        public const string REF_CONQ_LBL_LOBBY_GOLD_1 = "reference\\lbl-conquest-gold-1-preproc.png";
        public const string REF_CONQ_LBL_LOBBY_GOLD_2 = "reference\\lbl-conquest-gold-2-preproc.png";
        public const string REF_CONQ_LBL_LOBBY_GOLD_3 = "reference\\lbl-conquest-gold-3-preproc.png";
        public const string REF_CONQ_LBL_LOBBY_INFINITE_1 = "reference\\lbl-conquest-infinite-1-preproc.png";
        public const string REF_CONQ_LBL_LOBBY_INFINITE_2 = "reference\\lbl-conquest-infinite-2-preproc.png";
        public const string REF_CONQ_LBL_LOBBY_INFINITE_3 = "reference\\lbl-conquest-infinite-3-preproc.png";
        public const string REF_CONQ_BTN_MATCHMAKING_1 = "reference\\btn-conquest-matchmaking-cancel-1-preproc.png";
        public const string REF_CONQ_BTN_MATCHMAKING_2 = "reference\\btn-conquest-matchmaking-cancel-2-preproc.png";
        public const string REF_CONQ_BTN_RETREAT_1 = "reference\\btn-conquest-retreat-preproc.png";
        public const string REF_CONQ_BTN_RETREAT_2 = "reference\\btn-conquest-retreat-2-preproc.png";
        public const string REF_CONQ_BTN_END_TURN = "reference\\btn-conquest-end-turn-preproc.png";
        public const string REF_CONQ_BTN_WAITING = "reference\\btn-conquest-waiting-preproc.png";
        public const string REF_CONQ_BTN_PLAYING = "reference\\btn-conquest-playing-preproc.png";
        public const string REF_CONQ_BTN_CONCEDE_1 = "reference\\btn-conquest-concede-preproc.png";
        public const string REF_CONQ_BTN_CONCEDE_2 = "reference\\btn-conquest-concede-2-preproc.png";
        public const string REF_CONQ_BTN_MATCH_END_1 = "reference\\btn-conquest-match-end-next-1-preproc.png";
        public const string REF_CONQ_BTN_MATCH_END_2 = "reference\\btn-conquest-match-end-next-2-preproc.png";
        public const string REF_CONQ_BTN_CONTINUE = "reference\\btn-conquest-continue-preproc.png";
        public const string REF_CONQ_BTN_WIN_NEXT = "reference\\btn-conquest-victory-next-preproc.png";
        public const string REF_CONQ_BTN_WIN_TICKET = "reference\\btn-conquest-ticket-claim-preproc.png";

        public readonly BotConfig _config;

        public ComponentMappings(BotConfig config)
        {
            _config = config;
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
            Top = _config.Screencap.Height - 185,
            Bottom = _config.Screencap.Height - 155
        };

        #endregion
    }
}
