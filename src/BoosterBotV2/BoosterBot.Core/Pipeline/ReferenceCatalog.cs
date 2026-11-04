using BoosterBot.Core.Models;

namespace BoosterBot.Core.Pipeline;

/// <summary>
/// One reference image paired with the screen region it is matched against.
/// PadX/PadY widen the search window around the region (1080p reference pixels).
/// </summary>
public record ReferenceDefinition(
    string Key,
    Func<GameViewport, Rect> GetRegion,
    double Threshold,
    int PadX = ComponentMappings.SearchPadding,
    int PadY = ComponentMappings.SearchPadding)
{
    public Rect GetSearchRegion(GameViewport vp)
        => ComponentMappings.PadForSearch(GetRegion(vp), ViewportDetector.GetScale(vp), PadX, PadY);
}

/// <summary>
/// Single source of truth for which crop region each reference image belongs to.
/// Several references can share a region (e.g., END TURN and PLAYING... occupy the same
/// button). Pairings were ported from V1's GameUtilities and re-verified against the
/// current game UI in 2026-11.
/// </summary>
public static class ReferenceCatalog
{
    private const int CarouselPad = ComponentMappings.CarouselSearchPaddingX;

    public static IReadOnlyList<ReferenceDefinition> All { get; } =
    [
        // Shared
        new(ReferenceKeys.MainPlay, ComponentMappings.GetBtnPlay, DetectionThresholds.MainMenuPlay),
        new(ReferenceKeys.MainReconnect, ComponentMappings.GetBtnReconnect, DetectionThresholds.Default),
        new(ReferenceKeys.EnergyZero, ComponentMappings.GetEnergy, DetectionThresholds.EnergyIndicator),

        // Event
        new(ReferenceKeys.EventPlay, ComponentMappings.GetEventBtnPlay, DetectionThresholds.Default),
        // TODO: still the V1 placeholder image. FORFEIT replaces RETREAT only in certain recurring
        // events; capture it (and verify the region) the next time one of those events is live.
        new(ReferenceKeys.EventForfeit, ComponentMappings.GetLadderBtnRetreat, DetectionThresholds.Default),

        // Ladder
        new(ReferenceKeys.LadderMatchmaking1, ComponentMappings.GetMatchmakingCancel, DetectionThresholds.Default),
        new(ReferenceKeys.LadderMatchmaking2, ComponentMappings.GetMatchmakingCancel, DetectionThresholds.Default),
        new(ReferenceKeys.LadderRetreat, ComponentMappings.GetLadderBtnRetreat, DetectionThresholds.Default),
        new(ReferenceKeys.LadderCollectRewards, ComponentMappings.GetLadderBtnCollect, DetectionThresholds.Default),
        new(ReferenceKeys.LadderMatchEndNext, ComponentMappings.GetConquestBtnMatchEndNext2, DetectionThresholds.Default),

        // In-match buttons (shared by Ladder, Conquest, and Event)
        new(ReferenceKeys.ConquestEndTurn, ComponentMappings.GetConquestBtnEndTurn, DetectionThresholds.ButtonLowConfidence),
        new(ReferenceKeys.ConquestWaiting, ComponentMappings.GetConquestBtnWaiting, DetectionThresholds.ButtonLowConfidence),
        new(ReferenceKeys.ConquestPlaying1, ComponentMappings.GetConquestBtnEndTurn, DetectionThresholds.ButtonLowConfidence),
        new(ReferenceKeys.ConquestPlaying2, ComponentMappings.GetConquestBtnEndTurn, DetectionThresholds.ButtonLowConfidence),

        // Conquest entrance carousel and lobby
        new(ReferenceKeys.ConquestPlay, ComponentMappings.GetConquestBtnPlay, DetectionThresholds.Default),
        new(ReferenceKeys.ConquestEntranceFee, ComponentMappings.GetConquestEntranceFee, DetectionThresholds.Default),
        new(ReferenceKeys.ConquestNoTickets, ComponentMappings.GetConquestOwnedTicketsIcon, DetectionThresholds.Default),

        new(ReferenceKeys.ConquestLobbyPg1, ComponentMappings.GetConquestLobbySelection, DetectionThresholds.Default, CarouselPad),
        new(ReferenceKeys.ConquestLobbyPg2, ComponentMappings.GetConquestBannerCrop, DetectionThresholds.Default),
        new(ReferenceKeys.ConquestLobbyPg3, ComponentMappings.GetConquestLobbyRewardCrop, DetectionThresholds.Default, CarouselPad),

        new(ReferenceKeys.ConquestLobbySilver1, ComponentMappings.GetConquestLobbySelection, DetectionThresholds.Default, CarouselPad),
        new(ReferenceKeys.ConquestLobbySilver3, ComponentMappings.GetConquestBannerCrop, DetectionThresholds.Default),
        new(ReferenceKeys.ConquestLobbySilver4, ComponentMappings.GetConquestLobbyRewardCrop, DetectionThresholds.Default, CarouselPad),

        new(ReferenceKeys.ConquestLobbyGold1, ComponentMappings.GetConquestLobbySelection, DetectionThresholds.Default, CarouselPad),
        new(ReferenceKeys.ConquestLobbyGold3, ComponentMappings.GetConquestBannerCrop, DetectionThresholds.Default),
        new(ReferenceKeys.ConquestLobbyGold4, ComponentMappings.GetConquestLobbyRewardCrop, DetectionThresholds.Default, CarouselPad),

        new(ReferenceKeys.ConquestLobbyInfinite1, ComponentMappings.GetConquestLobbySelection, DetectionThresholds.Default, CarouselPad),
        new(ReferenceKeys.ConquestLobbyInfinite3, ComponentMappings.GetConquestBannerCrop, DetectionThresholds.Default),
        new(ReferenceKeys.ConquestLobbyInfinite4, ComponentMappings.GetConquestLobbyRewardCrop, DetectionThresholds.Default, CarouselPad),

        // Conquest match flow
        new(ReferenceKeys.ConquestMatchmaking1, ComponentMappings.GetMatchmakingCancel, DetectionThresholds.Default),
        new(ReferenceKeys.ConquestMatchmaking2, ComponentMappings.GetMatchmakingCancel, DetectionThresholds.Default),
        new(ReferenceKeys.ConquestRetreat1, ComponentMappings.GetConquestBtnRetreat, DetectionThresholds.ButtonLowConfidence),
        new(ReferenceKeys.ConquestRetreat2, ComponentMappings.GetConquestBtnRetreat, DetectionThresholds.ButtonLowConfidence),
        new(ReferenceKeys.ConquestConcede1, ComponentMappings.GetConquestBtnConcede, DetectionThresholds.Default),
        new(ReferenceKeys.ConquestConcede2, ComponentMappings.GetConquestBtnConcede, DetectionThresholds.Default),
        new(ReferenceKeys.ConquestMatchEnd1, ComponentMappings.GetConquestBtnMatchEndNext1, DetectionThresholds.Default),
        new(ReferenceKeys.ConquestMatchEnd2, ComponentMappings.GetConquestBtnMatchEndNext2, DetectionThresholds.Default),
        new(ReferenceKeys.ConquestVictoryNext, ComponentMappings.GetConquestVictoryNext, DetectionThresholds.Default),
        new(ReferenceKeys.ConquestTicketClaim, ComponentMappings.GetConquestTicketClaim, DetectionThresholds.Default),
    ];

    /// <summary>Finds a definition by key. Accepts the key with or without the "-preproc" suffix.</summary>
    public static ReferenceDefinition? Find(string key)
    {
        if (!key.EndsWith("-preproc"))
            key += "-preproc";
        return All.FirstOrDefault(d => d.Key == key);
    }
}
