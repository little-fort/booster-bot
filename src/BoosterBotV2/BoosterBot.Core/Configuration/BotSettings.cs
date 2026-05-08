using BoosterBot.Core.Models;

namespace BoosterBot.Core.Configuration;

public class BotSettings
{
    public BotMode DefaultMode { get; set; } = BotMode.Conquest;
    public ConquestTier MaxConquestTier { get; set; } = ConquestTier.ProvingGrounds;
    public int RetreatAfterTurn { get; set; }
    public double SnapProbability { get; set; } = 0.465;
    public string GameLanguage { get; set; } = "en";
    public string AppLanguage { get; set; } = "en-US";
    public bool SaveDebugScreenshots { get; set; }
    public bool VerboseLogging { get; set; }
    public double Scaling { get; set; } = 1.0;
    public string[] GameProcessNames { get; set; } = ["SNAP", "SnapCN", "streaming_client"];
}
