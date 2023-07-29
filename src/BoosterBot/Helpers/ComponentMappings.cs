using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoosterBot
{
    internal class ComponentMappings
    {
        public const string REF_LADD_BTN_PLAY = "Reference\\btn-main-play-preproc.png";
        public const string REF_ICON_ZERO_ENERGY = "Reference\\lbl-energy-zero-preproc.png";
        public const string REF_CONQ_BTN_PLAY = "Reference\\btn-conquest-play-preproc.png";
        public const string REF_CONQ_LBL_LOBBY_PG = "Reference\\lbl-conquest-pg-preproc.png";
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

        public static Rect GetBtnPlay(int center, Dimension screencap) => new Rect
        {
            Left = center - 45,
            Right = center + 40,
            Top = screencap.Height - 235,
            Bottom = screencap.Height - 195
        };
        
        public static Rect GetEnergy(int center, Dimension screencap) => new Rect
        {
            Left = center - 30,
            Right = center + 20,
            Top = screencap.Height - 90,
            Bottom = screencap.Height - 45
        };

        public static Rect GetConquestLobbySelection(int center) => new Rect
        {
            Left = center - 130,
            Right = center + 120,
            Top = 135,
            Bottom = 160
        };

        public static Rect GetConquestMatchmakingCancel(int center, Dimension screencap) => new Rect
        {
            Left = center - 55,
            Right = center + 40,
            Top = screencap.Height - 85,
            Bottom = screencap.Height - 55
        };

        public static Rect GetConquestBtnConcede(int center, Dimension screencap) => new Rect
        {
            Left = center - 380,
            Right = center - 265,
            Top = screencap.Height - 80,
            Bottom = screencap.Height - 55
        };

        public static Rect GetConquestBtnRetreat(int center, Dimension screencap) => new Rect
        {
            Left = center - 375,
            Right = center - 285,
            Top = screencap.Height - 90,
            Bottom = screencap.Height - 70
        };

        public static Rect GetConquestBtnEndTurn(int center, Dimension screencap) => new Rect
        {
            Left = center + 325,
            Right = center + 370,
            Top = screencap.Height - 90,
            Bottom = screencap.Height - 70
        };

        public static Rect GetConquestBtnWaiting(int center, Dimension screencap) => new Rect
        {
            Left = center + 265,
            Right = center + 355,
            Top = screencap.Height - 90,
            Bottom = screencap.Height - 70
        };

        public static Rect GetConquestBtnMatchEndNext1(int center, Dimension screencap) => new Rect
        {
            Left = center + 285,
            Right = center + 350,
            Top = screencap.Height - 80,
            Bottom = screencap.Height - 60
        };

        public static Rect GetConquestBtnMatchEndNext2(int center, Dimension screencap) => new Rect
        {
            Left = center + 285,
            Right = center + 355,
            Top = screencap.Height - 70,
            Bottom = screencap.Height - 40
        };

        public static Rect GetConquestBtnContinue(int center, Dimension screencap) => new Rect
        {
            Left = center - 85,
            Right = center + 85,
            Top = screencap.Height - 180,
            Bottom = screencap.Height - 145
        };

        public static Rect GetConquestVictoryNext(int center, Dimension screencap) => new Rect
        {
            Left = center - 55,
            Right = center + 40,
            Top = screencap.Height - 180,
            Bottom = screencap.Height - 145
        };

        public static Rect GetConquestTicketClaim(int center, Dimension screencap) => new Rect
        {
            Left = center - 65,
            Right = center + 40,
            Top = screencap.Height - 185,
            Bottom = screencap.Height - 155
        };
    }
}
