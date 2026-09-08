using PTCGBattleMetrics.Domain.Enums;

namespace PTCGBattleMetrics.Application.DTOs;

public record CardDto(
    string Name,
    int Quantity,
    string SetCode,
    string CollectorNumber,
    CardType CardType,
    TrainerSubType? TrainerSubType,
    bool IsTechCard
);

public record PtcglParseResult(
    bool Success,
    string? ErrorMessage,
    int TotalCards,
    int PokemonCount,
    int TrainerCount,
    int EnergyCount,
    List<CardDto> Cards,
    List<string> SuggestedTechCards
)
{
    public int TotalCount => TotalCards;
}

public record CreateDeckRequest(
    string Name,
    string Archetype,
    string Version,
    string? RawList,
    List<CardDto>? Cards,
    List<string>? TechCards
);

public record DeckResponse(
    Guid Id,
    string Name,
    string Archetype,
    string Version,
    int PokemonCount,
    int TrainerCount,
    int EnergyCount,
    int TotalCards,
    DateTimeOffset CreatedAt,
    List<string> TechCards,
    List<CardDto> Cards
)
{
    public int CardCount => TotalCards;
}

public record CreateGameDetailRequest(
    int GameNumber,
    GameResult Result,
    TurnOrder? TurnOrder,
    int? PlayerPrizesRemaining,
    int? OpponentPrizesRemaining,
    WinCondition? WinCondition,
    string? StartingActivePokemon,
    string? Notes
);

public record CreateMatchRequest(
    Guid DeckId,
    string OpponentArchetype,
    MatchResult Result,
    Guid? TournamentId = null,
    int? RoundNumber = null,
    int? TableNumber = null,
    string? OpponentName = null,
    string? OpponentPopId = null,
    bool? CoinFlipWon = null,
    TurnOrder? TurnOrder = null,
    int? PlayerMulligans = null,
    int? OpponentMulligans = null,
    int? PlayerPrizesRemaining = null,
    int? OpponentPrizesRemaining = null,
    WinCondition? WinCondition = null,
    string? StartingActivePokemon = null,
    string? TacticalNotes = null,
    List<string>? TechCardsUsed = null,
    List<CreateGameDetailRequest>? Games = null,
    DateTimeOffset? CreatedAt = null
);

public record MatchResponse(
    Guid Id,
    Guid DeckId,
    string DeckName,
    string DeckArchetype,
    Guid? TournamentId,
    string? TournamentName,
    string OpponentArchetype,
    MatchResult Result,
    DateTimeOffset CreatedAt,
    int? RoundNumber,
    int? TableNumber,
    string? OpponentName,
    string? OpponentPopId,
    bool? CoinFlipWon,
    TurnOrder? TurnOrder,
    int? PlayerMulligans,
    int? OpponentMulligans,
    int? PlayerPrizesRemaining,
    int? OpponentPrizesRemaining,
    int? PlayerPrizesTaken,
    int? OpponentPrizesTaken,
    WinCondition? WinCondition,
    string? StartingActivePokemon,
    string? TacticalNotes,
    List<string> TechCardsUsed,
    List<CreateGameDetailRequest> Games,
    int MatchPoints
);

public record CreateTournamentRequest(
    string Name,
    string? StoreOrVenue,
    DateOnly Date,
    TournamentCategory Category,
    TournamentFormat Format,
    int? TotalParticipants = null,
    int? FinalStanding = null,
    int? ChampionshipPoints = null,
    string? Notes = null
);

public record TournamentResponse(
    Guid Id,
    string Name,
    string? StoreOrVenue,
    DateOnly Date,
    TournamentCategory Category,
    TournamentFormat Format,
    int? TotalParticipants,
    int? FinalStanding,
    int? ChampionshipPoints,
    int TotalWins,
    int TotalLosses,
    int TotalTies,
    int MatchPoints,
    string RecordDisplay,
    int MatchesCount
)
{
    public int TotalMatches => MatchesCount;
}

public record SyncBatchRequest(
    List<CreateMatchRequest> OfflineMatches
);

public record SyncBatchResponse(
    int SyncedCount,
    List<Guid> CreatedMatchIds
);
