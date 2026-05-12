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
    // All V1 pixel offsets were calibrated at 1080p. The Scale() helper
    // converts them to the detected viewport's actual size so crop regions
    // land on the correct pixels at any resolution or aspect ratio.

    private static int S(int v1Offset, double scale) => (int)(v1Offset * scale);

    // Search padding added around crop regions (in 1080p reference pixels).
    // Gives template matching room to slide and find the best alignment
    // even when the crop isn't pixel-perfect.
    private const int SearchPadding = 15;

    public static Rect PadForSearch(Rect region, double scale)
    {
        int pad = S(SearchPadding, scale);
        return new()
        {
            Left = region.Left - pad,
            Top = region.Top - pad,
            Right = region.Right + pad,
            Bottom = region.Bottom + pad
        };
    }

    // Shared

    public static Rect GetBtnPlay(GameViewport vp)
    {
        double s = ViewportDetector.GetScale(vp);
        return new()
        {
            Left = vp.CenterX - S(45, s),
            Right = vp.CenterX + S(40, s),
            Top = vp.Bottom - S(235, s),
            Bottom = vp.Bottom - S(195, s)
        };
    }

    public static Rect GetEnergy(GameViewport vp)
    {
        double s = ViewportDetector.GetScale(vp);
        return new()
        {
            Left = vp.CenterX - S(30, s),
            Right = vp.CenterX + S(20, s),
            Top = vp.Bottom - S(90, s),
            Bottom = vp.Bottom - S(45, s)
        };
    }

    // Ladder

    public static Rect GetLadderMatchmakingCancel(GameViewport vp)
    {
        double s = ViewportDetector.GetScale(vp);
        return new()
        {
            Left = vp.CenterX - S(50, s),
            Right = vp.CenterX + S(40, s),
            Top = vp.Bottom - S(85, s),
            Bottom = vp.Bottom - S(55, s)
        };
    }

    public static Rect GetLadderBtnRetreat(GameViewport vp)
    {
        double s = ViewportDetector.GetScale(vp);
        return new()
        {
            Left = vp.CenterX - S(385, s),
            Right = vp.CenterX - S(275, s),
            Top = vp.Bottom - S(80, s),
            Bottom = vp.Bottom - S(60, s)
        };
    }

    public static Rect GetLadderBtnCollect(GameViewport vp)
    {
        double s = ViewportDetector.GetScale(vp);
        return new()
        {
            Left = vp.CenterX + S(245, s),
            Right = vp.CenterX + S(375, s),
            Top = vp.Bottom - S(80, s),
            Bottom = vp.Bottom - S(60, s)
        };
    }

    // Conquest

    public static Rect GetConquestBannerCrop(GameViewport vp)
    {
        double s = ViewportDetector.GetScale(vp);
        return new()
        {
            Left = vp.CenterX - S(60, s),
            Right = vp.CenterX + S(50, s),
            Top = vp.Top + S(20, s),
            Bottom = vp.Top + S(55, s)
        };
    }

    public static Rect GetConquestLobbySelection(GameViewport vp)
    {
        double s = ViewportDetector.GetScale(vp);
        return new()
        {
            Left = vp.CenterX - S(130, s),
            Right = vp.CenterX + S(120, s),
            Top = vp.Top + S(135, s),
            Bottom = vp.Top + S(160, s)
        };
    }

    public static Rect GetConquestLobbyRewardCrop(GameViewport vp)
    {
        double s = ViewportDetector.GetScale(vp);
        return new()
        {
            Left = vp.CenterX + S(95, s),
            Right = vp.CenterX + S(195, s),
            Top = vp.Top + S(465, s),
            Bottom = vp.Top + S(485, s)
        };
    }

    public static Rect GetConquestOwnedTicketsIcon(GameViewport vp)
    {
        double s = ViewportDetector.GetScale(vp);
        return new()
        {
            Left = vp.CenterX - S(85, s),
            Right = vp.CenterX + S(75, s),
            Top = vp.Bottom - S(155, s),
            Bottom = vp.Bottom - S(125, s)
        };
    }

    public static Rect GetConquestMatchmakingCancel(GameViewport vp)
    {
        double s = ViewportDetector.GetScale(vp);
        return new()
        {
            Left = vp.CenterX - S(55, s),
            Right = vp.CenterX + S(40, s),
            Top = vp.Bottom - S(85, s),
            Bottom = vp.Bottom - S(55, s)
        };
    }

    public static Rect GetConquestBtnConcede(GameViewport vp)
    {
        double s = ViewportDetector.GetScale(vp);
        return new()
        {
            Left = vp.CenterX - S(380, s),
            Right = vp.CenterX - S(265, s),
            Top = vp.Bottom - S(80, s),
            Bottom = vp.Bottom - S(55, s)
        };
    }

    public static Rect GetConquestBtnRetreat(GameViewport vp)
    {
        double s = ViewportDetector.GetScale(vp);
        return new()
        {
            Left = vp.CenterX - S(375, s),
            Right = vp.CenterX - S(285, s),
            Top = vp.Bottom - S(90, s),
            Bottom = vp.Bottom - S(70, s)
        };
    }

    public static Rect GetConquestBtnEndTurn(GameViewport vp)
    {
        double s = ViewportDetector.GetScale(vp);
        return new()
        {
            Left = vp.CenterX + S(325, s),
            Right = vp.CenterX + S(370, s),
            Top = vp.Bottom - S(90, s),
            Bottom = vp.Bottom - S(70, s)
        };
    }

    public static Rect GetConquestBtnWaiting(GameViewport vp)
    {
        double s = ViewportDetector.GetScale(vp);
        return new()
        {
            Left = vp.CenterX + S(265, s),
            Right = vp.CenterX + S(355, s),
            Top = vp.Bottom - S(90, s),
            Bottom = vp.Bottom - S(70, s)
        };
    }

    public static Rect GetConquestBtnMatchEndNext1(GameViewport vp)
    {
        double s = ViewportDetector.GetScale(vp);
        return new()
        {
            Left = vp.CenterX + S(285, s),
            Right = vp.CenterX + S(350, s),
            Top = vp.Bottom - S(80, s),
            Bottom = vp.Bottom - S(60, s)
        };
    }

    public static Rect GetConquestBtnMatchEndNext2(GameViewport vp)
    {
        double s = ViewportDetector.GetScale(vp);
        return new()
        {
            Left = vp.CenterX + S(285, s),
            Right = vp.CenterX + S(355, s),
            Top = vp.Bottom - S(70, s),
            Bottom = vp.Bottom - S(40, s)
        };
    }

    public static Rect GetConquestBtnContinue(GameViewport vp)
    {
        double s = ViewportDetector.GetScale(vp);
        return new()
        {
            Left = vp.CenterX - S(85, s),
            Right = vp.CenterX + S(85, s),
            Top = vp.Bottom - S(180, s),
            Bottom = vp.Bottom - S(145, s)
        };
    }

    public static Rect GetConquestVictoryNext(GameViewport vp)
    {
        double s = ViewportDetector.GetScale(vp);
        return new()
        {
            Left = vp.CenterX - S(55, s),
            Right = vp.CenterX + S(40, s),
            Top = vp.Bottom - S(180, s),
            Bottom = vp.Bottom - S(145, s)
        };
    }

    public static Rect GetConquestTicketClaim(GameViewport vp)
    {
        double s = ViewportDetector.GetScale(vp);
        return new()
        {
            Left = vp.CenterX - S(65, s),
            Right = vp.CenterX + S(40, s),
            Top = vp.Bottom - S(160, s),
            Bottom = vp.Bottom - S(130, s)
        };
    }
}
