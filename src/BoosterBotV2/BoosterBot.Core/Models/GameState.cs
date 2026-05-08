namespace BoosterBot.Core.Models;

public enum GameState
{
    Unknown,
    MainMenu,
    EventMenu,
    Reconnecting,

    Matchmaking,
    ActiveMatch,
    MidTurn,
    MatchEnd,
    MatchEndRewards,

    ConquestLobby,
    ConquestNoTickets,
    ConquestPrematch,
    ConquestRoundEnd,
    ConquestPostMatchLoss,
    ConquestPostMatchWin,
    ConquestTicketClaim,
}

public enum ConquestTier
{
    ProvingGrounds,
    Silver,
    Gold,
    Infinite
}

public record GameStateResult(
    GameState State,
    ConquestTier? Tier = null,
    double Confidence = 0.0
);
