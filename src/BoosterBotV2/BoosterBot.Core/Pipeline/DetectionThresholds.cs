namespace BoosterBot.Core.Pipeline;

public static class DetectionThresholds
{
    public const double Default = 0.90;
    public const double ButtonHighConfidence = 0.95;
    public const double ButtonLowConfidence = 0.80;
    public const double LobbyIndicator = 0.85;
    public const double EnergyIndicator = 0.925;
    public const double MatchmakingIndicator = 0.70;
}
