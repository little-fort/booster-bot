namespace BoosterBot.Core.Pipeline;

public static class DetectionThresholds
{
    public const double Default = 0.90;
    public const double ButtonHighConfidence = 0.95;
    public const double ButtonLowConfidence = 0.80;
    public const double LobbyIndicator = 0.85;
    // The Conquest lobby PLAY is the same lettering ~6px higher and scores 88–89% against the
    // main menu reference; real main menu captures score ≥99.7% at every tested resolution.
    public const double MainMenuPlay = 0.93;
    // "0" vs other digits: zeros score ≥99.9%, a "6" scores 88.3% (2026-11 corpus). 8/9 not yet sampled.
    public const double EnergyIndicator = 0.95;
    public const double MatchmakingIndicator = 0.70;
}
