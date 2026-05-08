using BoosterBot.Core.Models;

namespace BoosterBot.Core.Pipeline;

public static class ReferenceKeys
{
    public const string MainPlay = "btn-main-play-preproc";
    public const string MainReconnect = "btn-main-reconnect-preproc";

    public const string EventPlay = "btn-event-play-preproc";
    public const string EventForfeit = "btn-event-forfeit-preproc";

    public const string LadderMatchmaking1 = "btn-ladder-matchmaking-cancel-1-preproc";
    public const string LadderMatchmaking2 = "btn-ladder-matchmaking-cancel-2-preproc";
    public const string LadderRetreat = "btn-ladder-retreat-preproc";
    public const string LadderCollectRewards = "btn-ladder-collect-rewards-preproc";
    public const string LadderMatchEndNext = "btn-ladder-match-end-next-preproc";

    public const string EnergyZero = "lbl-energy-zero-preproc";

    public const string ConquestPlay = "btn-conquest-play-preproc";
    public const string ConquestEntranceFee = "btn-conquest-entrance-fee-preproc";
    public const string ConquestNoTickets = "lbl-conquest-no-tickets-preproc";

    public const string ConquestLobbyPg1 = "lbl-conquest-pg-1-preproc";
    public const string ConquestLobbyPg2 = "lbl-conquest-pg-2-preproc";
    public const string ConquestLobbyPg3 = "lbl-conquest-pg-3-preproc";

    public const string ConquestLobbySilver1 = "lbl-conquest-silver-1-preproc";
    public const string ConquestLobbySilver2 = "lbl-conquest-silver-2-preproc";
    public const string ConquestLobbySilver3 = "lbl-conquest-silver-3-preproc";
    public const string ConquestLobbySilver4 = "lbl-conquest-silver-4-preproc";

    public const string ConquestLobbyGold1 = "lbl-conquest-gold-1-preproc";
    public const string ConquestLobbyGold2 = "lbl-conquest-gold-2-preproc";
    public const string ConquestLobbyGold3 = "lbl-conquest-gold-3-preproc";
    public const string ConquestLobbyGold4 = "lbl-conquest-gold-4-preproc";

    public const string ConquestLobbyInfinite1 = "lbl-conquest-infinite-1-preproc";
    public const string ConquestLobbyInfinite2 = "lbl-conquest-infinite-2-preproc";
    public const string ConquestLobbyInfinite3 = "lbl-conquest-infinite-3-preproc";
    public const string ConquestLobbyInfinite4 = "lbl-conquest-infinite-4-preproc";

    public const string ConquestMatchmaking1 = "btn-conquest-matchmaking-cancel-1-preproc";
    public const string ConquestMatchmaking2 = "btn-conquest-matchmaking-cancel-2-preproc";

    public const string ConquestRetreat1 = "btn-conquest-retreat-preproc";
    public const string ConquestRetreat2 = "btn-conquest-retreat-2-preproc";

    public const string ConquestEndTurn = "btn-conquest-end-turn-preproc";

    public const string ConquestWaiting1 = "btn-conquest-waiting-preproc";
    public const string ConquestWaiting2 = "btn-conquest-waiting-2-preproc";

    public const string ConquestPlaying1 = "btn-conquest-playing-preproc";
    public const string ConquestPlaying2 = "btn-conquest-playing-2-preproc";

    public const string ConquestConcede1 = "btn-conquest-concede-preproc";
    public const string ConquestConcede2 = "btn-conquest-concede-2-preproc";

    public const string ConquestMatchEnd1 = "btn-conquest-match-end-next-1-preproc";
    public const string ConquestMatchEnd2 = "btn-conquest-match-end-next-2-preproc";

    public const string ConquestContinue = "btn-conquest-continue-preproc";
    public const string ConquestVictoryNext = "btn-conquest-victory-next-preproc";

    public const string ConquestTicketClaim1 = "btn-conquest-ticket-claim-preproc";
    public const string ConquestTicketClaim2 = "btn-conquest-ticket-claim-2-preproc";
    public const string ConquestTicketClaim3 = "btn-conquest-ticket-claim-3-preproc";
}

public static class ComponentMappings
{
    // Shared

    public static Rect GetBtnPlay(Dimension screen, int center) => new()
    {
        Left = center - 45,
        Right = center + 40,
        Top = screen.Height - 235,
        Bottom = screen.Height - 195
    };

    public static Rect GetEnergy(Dimension screen, int center) => new()
    {
        Left = center - 30,
        Right = center + 20,
        Top = screen.Height - 90,
        Bottom = screen.Height - 45
    };

    // Ladder

    public static Rect GetLadderMatchmakingCancel(Dimension screen, int center) => new()
    {
        Left = center - 50,
        Right = center + 40,
        Top = screen.Height - 85,
        Bottom = screen.Height - 55
    };

    public static Rect GetLadderBtnRetreat(Dimension screen, int center) => new()
    {
        Left = center - 385,
        Right = center - 275,
        Top = screen.Height - 80,
        Bottom = screen.Height - 60
    };

    public static Rect GetLadderBtnCollect(Dimension screen, int center) => new()
    {
        Left = center + 245,
        Right = center + 375,
        Top = screen.Height - 80,
        Bottom = screen.Height - 60
    };

    // Conquest

    public static Rect GetConquestBannerCrop(Dimension screen, int center) => new()
    {
        Left = center - 60,
        Right = center + 50,
        Top = 20,
        Bottom = 55
    };

    public static Rect GetConquestLobbySelection(Dimension screen, int center) => new()
    {
        Left = center - 130,
        Right = center + 120,
        Top = 135,
        Bottom = 160
    };

    public static Rect GetConquestLobbyRewardCrop(Dimension screen, int center) => new()
    {
        Left = center + 95,
        Right = center + 195,
        Top = 465,
        Bottom = 485
    };

    public static Rect GetConquestOwnedTicketsIcon(Dimension screen, int center) => new()
    {
        Left = center - 85,
        Right = center + 75,
        Top = screen.Height - 155,
        Bottom = screen.Height - 125
    };

    public static Rect GetConquestMatchmakingCancel(Dimension screen, int center) => new()
    {
        Left = center - 55,
        Right = center + 40,
        Top = screen.Height - 85,
        Bottom = screen.Height - 55
    };

    public static Rect GetConquestBtnConcede(Dimension screen, int center) => new()
    {
        Left = center - 380,
        Right = center - 265,
        Top = screen.Height - 80,
        Bottom = screen.Height - 55
    };

    public static Rect GetConquestBtnRetreat(Dimension screen, int center) => new()
    {
        Left = center - 375,
        Right = center - 285,
        Top = screen.Height - 90,
        Bottom = screen.Height - 70
    };

    public static Rect GetConquestBtnEndTurn(Dimension screen, int center) => new()
    {
        Left = center + 325,
        Right = center + 370,
        Top = screen.Height - 90,
        Bottom = screen.Height - 70
    };

    public static Rect GetConquestBtnWaiting(Dimension screen, int center) => new()
    {
        Left = center + 265,
        Right = center + 355,
        Top = screen.Height - 90,
        Bottom = screen.Height - 70
    };

    public static Rect GetConquestBtnMatchEndNext1(Dimension screen, int center) => new()
    {
        Left = center + 285,
        Right = center + 350,
        Top = screen.Height - 80,
        Bottom = screen.Height - 60
    };

    public static Rect GetConquestBtnMatchEndNext2(Dimension screen, int center) => new()
    {
        Left = center + 285,
        Right = center + 355,
        Top = screen.Height - 70,
        Bottom = screen.Height - 40
    };

    public static Rect GetConquestBtnContinue(Dimension screen, int center) => new()
    {
        Left = center - 85,
        Right = center + 85,
        Top = screen.Height - 180,
        Bottom = screen.Height - 145
    };

    public static Rect GetConquestVictoryNext(Dimension screen, int center) => new()
    {
        Left = center - 55,
        Right = center + 40,
        Top = screen.Height - 180,
        Bottom = screen.Height - 145
    };

    public static Rect GetConquestTicketClaim(Dimension screen, int center) => new()
    {
        Left = center - 65,
        Right = center + 40,
        Top = screen.Height - 160,
        Bottom = screen.Height - 130
    };
}
