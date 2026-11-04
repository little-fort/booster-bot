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
    public const string ConquestLobbySilver3 = "lbl-conquest-silver-3-preproc";
    public const string ConquestLobbySilver4 = "lbl-conquest-silver-4-preproc";

    public const string ConquestLobbyGold1 = "lbl-conquest-gold-1-preproc";
    public const string ConquestLobbyGold3 = "lbl-conquest-gold-3-preproc";
    public const string ConquestLobbyGold4 = "lbl-conquest-gold-4-preproc";

    public const string ConquestLobbyInfinite1 = "lbl-conquest-infinite-1-preproc";
    public const string ConquestLobbyInfinite3 = "lbl-conquest-infinite-3-preproc";
    public const string ConquestLobbyInfinite4 = "lbl-conquest-infinite-4-preproc";

    public const string ConquestMatchmaking1 = "btn-conquest-matchmaking-cancel-1-preproc";
    public const string ConquestMatchmaking2 = "btn-conquest-matchmaking-cancel-2-preproc";

    public const string ConquestRetreat1 = "btn-conquest-retreat-preproc";
    public const string ConquestRetreat2 = "btn-conquest-retreat-2-preproc";

    public const string ConquestEndTurn = "btn-conquest-end-turn-preproc";

    public const string ConquestWaiting = "btn-conquest-waiting-preproc";

    public const string ConquestPlaying1 = "btn-conquest-playing-preproc";
    public const string ConquestPlaying2 = "btn-conquest-playing-2-preproc";

    public const string ConquestConcede1 = "btn-conquest-concede-preproc";
    public const string ConquestConcede2 = "btn-conquest-concede-2-preproc";

    public const string ConquestMatchEnd1 = "btn-conquest-match-end-next-1-preproc";
    public const string ConquestMatchEnd2 = "btn-conquest-match-end-next-2-preproc";

    public const string ConquestVictoryNext = "btn-conquest-victory-next-preproc";

    public const string ConquestTicketClaim = "btn-conquest-ticket-claim-preproc";
}

/// <summary>
/// Screen regions where UI elements appear. Each region is the exact crop a reference image is
/// taken from; detection searches a padded version of it (see <see cref="PadForSearch"/>).
///
/// Coordinates are 1080p reference pixels relative to the game viewport (its horizontal center,
/// top, or bottom edge) and are multiplied by the viewport scale, so a region lands on the same
/// UI element at any resolution or aspect ratio. Regions marked "measured 2026-11" were measured
/// from live 1920x1080 captures of the current game UI; the rest are unverified V1 values for
/// screens that have not been captured yet.
/// </summary>
public static class ComponentMappings
{
    private static int S(int offset, double scale) => (int)(offset * scale);

    // Default search padding added around crop regions (in 1080p reference pixels).
    // Gives template matching room to slide and find the best alignment
    // even when the crop isn't pixel-perfect.
    public const int SearchPadding = 15;

    // The Conquest tier carousel sometimes settles off-center (observed +18 and +30px),
    // shifting the tier title and reward label sideways. Search those regions wider.
    public const int CarouselSearchPaddingX = 50;

    public static Rect PadForSearch(Rect region, double scale) => PadForSearch(region, scale, SearchPadding, SearchPadding);

    public static Rect PadForSearch(Rect region, double scale, int padX, int padY)
    {
        int px = S(padX, scale), py = S(padY, scale);
        return new()
        {
            Left = region.Left - px,
            Top = region.Top - py,
            Right = region.Right + px,
            Bottom = region.Bottom + py
        };
    }

    // Builds a region from 1080p offsets relative to the viewport's horizontal center
    private static Rect FromCenterBottom(GameViewport vp, int left, int right, int topFromBottom, int bottomFromBottom)
    {
        double s = ViewportDetector.GetScale(vp);
        return new()
        {
            Left = vp.CenterX + S(left, s),
            Right = vp.CenterX + S(right, s),
            Top = vp.Bottom - S(topFromBottom, s),
            Bottom = vp.Bottom - S(bottomFromBottom, s)
        };
    }

    private static Rect FromCenterTop(GameViewport vp, int left, int right, int top, int bottom)
    {
        double s = ViewportDetector.GetScale(vp);
        return new()
        {
            Left = vp.CenterX + S(left, s),
            Right = vp.CenterX + S(right, s),
            Top = vp.Top + S(top, s),
            Bottom = vp.Top + S(bottom, s)
        };
    }

    // Shared

    // Main menu PLAY (unchanged from V1; its V2 reference was regenerated 2026-05)
    public static Rect GetBtnPlay(GameViewport vp) => FromCenterBottom(vp, -45, 40, 235, 195);

    // Two-line "RECONNECT TO / GAME" that replaces main menu PLAY while a match is in progress,
    // measured 2026-11 (x 882–1045, y 828–882)
    public static Rect GetBtnReconnect(GameViewport vp) => FromCenterBottom(vp, -82, 89, 255, 195);

    // Energy orb numeral, measured 2026-11 (x 945–975, y 986–1025 at 1080p). Cropped tight to the
    // digit: the orb's glow is shared by every digit and inflates scores for non-zero values.
    public static Rect GetEnergy(GameViewport vp) => FromCenterBottom(vp, -16, 16, 95, 54);

    // Matchmaking CANCEL, measured 2026-11 (identical in Ladder, Conquest, and the main Play queue)
    public static Rect GetMatchmakingCancel(GameViewport vp) => FromCenterBottom(vp, -42, 37, 84, 53);

    // Single-line RETREAT used by Ladder and Events, measured 2026-11 (x 585–691, y 988–1022)
    public static Rect GetLadderBtnRetreat(GameViewport vp) => FromCenterBottom(vp, -379, -265, 96, 54);

    // COLLECT REWARDS after a Ladder/Event match, measured 2026-11 (x 1214–1357, y 990–1018)
    public static Rect GetLadderBtnCollect(GameViewport vp) => FromCenterBottom(vp, 250, 401, 94, 58);

    // Event

    // Event screen PLAY sits above the main menu's, measured 2026-11 (x 917–1005, y 807–848)
    public static Rect GetEventBtnPlay(GameViewport vp) => FromCenterBottom(vp, -47, 49, 277, 228);

    // Conquest

    // In-lobby banner at the top of the screen after entering a tier, measured 2026-11 (PG: x 862–1058, y 25–50)
    public static Rect GetConquestBannerCrop(GameViewport vp) => FromCenterTop(vp, -110, 110, 22, 53);

    // Tier title on the entrance carousel, measured 2026-11 (x 832–1088, y 132–161). Search with CarouselSearchPaddingX.
    public static Rect GetConquestLobbySelection(GameViewport vp) => FromCenterTop(vp, -132, 132, 128, 165);

    // Reward label on the entrance carousel ("1 Silver Ticket" etc.), measured 2026-11 (x 1017–1193, y 462–490).
    // Search with CarouselSearchPaddingX.
    public static Rect GetConquestLobbyRewardCrop(GameViewport vp) => FromCenterTop(vp, 55, 237, 458, 494);

    // "ENTRANCE FEE" label on paid tiers, measured 2026-11 (x 863–1059, y 749–782)
    public static Rect GetConquestEntranceFee(GameViewport vp) => FromCenterBottom(vp, -100, 102, 334, 295);

    // "0/1 OWNED" ticket count on paid tiers, measured 2026-11 (x 879–1042, y 919–946)
    public static Rect GetConquestOwnedTicketsIcon(GameViewport vp) => FromCenterBottom(vp, -85, 86, 165, 130);

    // Lobby PLAY after entering a tier, measured 2026-11 (x 918–1005, y 836–877)
    public static Rect GetConquestBtnPlay(GameViewport vp) => FromCenterBottom(vp, -45, 48, 248, 200);

    // Two-line "RETREAT / ROUND n"; only the RETREAT line, measured 2026-11 (x 585–681, y 982–1012)
    public static Rect GetConquestBtnRetreat(GameViewport vp) => FromCenterBottom(vp, -379, -275, 101, 65);

    // CONCEDE between Conquest rounds, measured 2026-11 (x 580–695, y 990–1025)
    public static Rect GetConquestBtnConcede(GameViewport vp) => FromCenterBottom(vp, -383, -262, 93, 52);

    // END TURN / PLAYING... share this slot (turn counter below excluded), measured 2026-11 (x 1234–1331, y 975–998)
    public static Rect GetConquestBtnEndTurn(GameViewport vp) => FromCenterBottom(vp, 270, 375, 108, 79);

    // "UNDO END TURN" shown after ending your turn while the opponent is still playing (the
    // "waiting" state; V1's waiting-1 reference was a cropped version), measured 2026-11 (x 1213–1355, y 972–1005)
    public static Rect GetConquestBtnWaiting(GameViewport vp) => FromCenterBottom(vp, 250, 398, 110, 73);

    // In-match NEXT after a Conquest battle ends, measured 2026-11 (x 1256–1316, y 989–1019)
    public static Rect GetConquestBtnMatchEndNext1(GameViewport vp) => FromCenterBottom(vp, 292, 360, 95, 57);

    // Results-screen NEXT (Ladder, Event, Conquest "Battle Defeated" and "Battle Victory!"), measured 2026-11 (x 1245–1322, y 1001–1036)
    public static Rect GetConquestBtnMatchEndNext2(GameViewport vp) => FromCenterBottom(vp, 281, 366, 83, 40);

    // Centered NEXT on "YOU BEAT THE <TIER>!" after a Conquest run is won, measured 2026-11 (x 917–1007, y 888–932)
    public static Rect GetConquestVictoryNext(GameViewport vp) => FromCenterBottom(vp, -48, 52, 196, 144);

    // CLAIM! on the ticket reward screen after a Conquest run is won, measured 2026-11 (x 900–1023, y 947–990).
    // (V1's region sat above the button and captured only its border stripes.)
    public static Rect GetConquestTicketClaim(GameViewport vp) => FromCenterBottom(vp, -64, 67, 137, 86);
}
