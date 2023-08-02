
namespace BoosterBot
{
    internal class ComponentMappings
    {
        public const string REF_LADD_BTN_PLAY = "Reference\\btn-main-play-preproc.png";
        public const string REF_LADD_BTN_MATCHMAKING = "Reference\\btn-ladder-matchmaking-cancel-preproc.png";
        public const string REF_LADD_BTN_RETREAT = "Reference\\btn-ladder-retreat-preproc.png";
        public const string REF_LADD_BTN_COLLECT_REWARDS = "Reference\\btn-ladder-collect-rewards-preproc.png";
        public const string REF_LADD_BTN_MATCH_END_NEXT = "Reference\\btn-ladder-match-end-next-preproc.png";
        public const string REF_ICON_ZERO_ENERGY = "Reference\\lbl-energy-zero-preproc.png";
        public const string REF_CONQ_BTN_PLAY = "Reference\\btn-conquest-play-preproc.png";
        public const string REF_CONQ_LBL_LOBBY_PG = "Reference\\lbl-conquest-pg-preproc.png";
        public const string REF_CONQ_LBL_LOBBY_SILVER = "Reference\\lbl-conquest-silver-preproc.png";
        public const string REF_CONQ_LBL_LOBBY_GOLD = "Reference\\lbl-conquest-gold-preproc.png";
        public const string REF_CONQ_LBL_LOBBY_INFINITE = "Reference\\lbl-conquest-infinite-preproc.png";
        public const string REF_CONQ_BTN_MATCHMAKING = "Reference\\btn-conquest-matchmaking-cancel-preproc.png";
        public const string REF_CONQ_BTN_RETREAT_1 = "Reference\\btn-conquest-retreat-preproc.png";
        public const string REF_CONQ_BTN_RETREAT_2 = "Reference\\btn-conquest-retreat-2-preproc.png";
        public const string REF_CONQ_BTN_END_TURN = "Reference\\btn-conquest-end-turn-preproc.png";
        public const string REF_CONQ_BTN_WAITING = "Reference\\btn-conquest-waiting-preproc.png";
        public const string REF_CONQ_BTN_PLAYING = "Reference\\btn-conquest-playing-preproc.png";
        public const string REF_CONQ_BTN_CONCEDE_1 = "Reference\\btn-conquest-concede-preproc.png";
        public const string REF_CONQ_BTN_CONCEDE_2 = "Reference\\btn-conquest-concede-2-preproc.png";
        public const string REF_CONQ_BTN_MATCH_END_1 = "Reference\\btn-conquest-match-end-next-1-preproc.png";
        public const string REF_CONQ_BTN_MATCH_END_2 = "Reference\\btn-conquest-match-end-next-2-preproc.png";
        public const string REF_CONQ_BTN_CONTINUE = "Reference\\btn-conquest-continue-preproc.png";
        public const string REF_CONQ_BTN_WIN_NEXT = "Reference\\btn-conquest-victory-next-preproc.png";
        public const string REF_CONQ_BTN_WIN_TICKET = "Reference\\btn-conquest-ticket-claim-preproc.png";

        public readonly BotConfig _config;

        public ComponentMappings(BotConfig config)
        {
            _config = config;
        }

        public Rect GetBtnPlay(int center, Dimension screencap) => new Rect
        {
            Left = center - _config.Scale(45),
            Right = center + _config.Scale(40),
            Top = screencap.Height - _config.Scale(235),
            Bottom = screencap.Height - _config.Scale(195)
        };
        
        public Rect GetEnergy(int center, Dimension screencap) => new Rect
        {
            Left = center - _config.Scale(30),
            Right = center + _config.Scale(20),
            Top = screencap.Height - _config.Scale(90),
            Bottom = screencap.Height - _config.Scale(45)
        };

        #region Ladder

        public Rect GetLadderMatchmakingCancel(int center, Dimension screencap) => new Rect
        {
            Left = center - _config.Scale(50),
            Right = center + _config.Scale(40),
            Top = screencap.Height - _config.Scale(85),
            Bottom = screencap.Height - _config.Scale(55)
        };

        public Rect GetLadderBtnRetreat( int center, Dimension screencap) => new Rect
        {
            Left = center - _config.Scale(385),
            Right = center - _config.Scale(275),
            Top = screencap.Height - _config.Scale(80),
            Bottom = screencap.Height - _config.Scale(60)
        };

        public Rect GetConquestBtnCollect(int center, Dimension screencap) => new Rect
        {
            Left = center + _config.Scale(245),
            Right = center + _config.Scale(375),
            Top = screencap.Height - _config.Scale(80),
            Bottom = screencap.Height - _config.Scale(60)
        };

        #endregion

        #region Conquest

        public Rect GetConquestBannerCrop() => new Rect
        {
            Left = _config.Center - _config.Scale(110),
            Right = _config.Center + _config.Scale(100),
            Top = _config.Scale(15),
            Bottom = _config.Scale(60)
        };

        public Rect GetConquestLobbySelection(int center) => new Rect
        {
            Left = center - _config.Scale(130),
            Right = center + _config.Scale(120),
            Top = _config.Scale(135),
            Bottom = _config.Scale(160)
        };

        public Rect GetConquestMatchmakingCancel(int center, Dimension screencap) => new Rect
        {
            Left = center - _config.Scale(55),
            Right = center + _config.Scale(40),
            Top = screencap.Height - _config.Scale(85),
            Bottom = screencap.Height - _config.Scale(55)
        };

        public Rect GetConquestBtnConcede(int center, Dimension screencap) => new Rect
        {
            Left = center - _config.Scale(380),
            Right = center - _config.Scale(265),
            Top = screencap.Height - _config.Scale(80),
            Bottom = screencap.Height - _config.Scale(55)
        };

        public Rect GetConquestBtnRetreat(int center, Dimension screencap) => new Rect
        {
            Left = center - _config.Scale(375),
            Right = center - _config.Scale(285),
            Top = screencap.Height - _config.Scale(90),
            Bottom = screencap.Height - _config.Scale(70)
        };

        public Rect GetConquestBtnEndTurn(int center, Dimension screencap) => new Rect
        {
            Left = center + _config.Scale(325),
            Right = center + _config.Scale(370),
            Top = screencap.Height - _config.Scale(90),
            Bottom = screencap.Height - _config.Scale(70)
        };

        public Rect GetConquestBtnWaiting(int center, Dimension screencap) => new Rect
        {
            Left = center + _config.Scale(265),
            Right = center + _config.Scale(355),
            Top = screencap.Height - _config.Scale(90),
            Bottom = screencap.Height - _config.Scale(70)
        };

        public Rect GetConquestBtnMatchEndNext1(int center, Dimension screencap) => new Rect
        {
            Left = center + _config.Scale(285),
            Right = center + _config.Scale(350),
            Top = screencap.Height - _config.Scale(80),
            Bottom = screencap.Height - _config.Scale(60)
        };

        public Rect GetConquestBtnMatchEndNext2(int center, Dimension screencap) => new Rect
        {
            Left = center + _config.Scale(285),
            Right = center + _config.Scale(355),
            Top = screencap.Height - _config.Scale(70),
            Bottom = screencap.Height - _config.Scale(40)
        };

        public Rect GetConquestBtnContinue(int center, Dimension screencap) => new Rect
        {
            Left = center - _config.Scale(85),
            Right = center + _config.Scale(85),
            Top = screencap.Height - _config.Scale(180),
            Bottom = screencap.Height - _config.Scale(145)
        };

        public Rect GetConquestVictoryNext(int center, Dimension screencap) => new Rect
        {
            Left = center - _config.Scale(55),
            Right = center + _config.Scale(40),
            Top = screencap.Height - _config.Scale(180),
            Bottom = screencap.Height - _config.Scale(145)
        };

        public Rect GetConquestTicketClaim(int center, Dimension screencap) => new Rect
        {
            Left = center - _config.Scale(65),
            Right = center + _config.Scale(40),
            Top = screencap.Height - _config.Scale(185),
            Bottom = screencap.Height - _config.Scale(155)
        };

        #endregion
    }
}
