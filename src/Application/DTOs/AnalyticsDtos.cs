using PTCGBattleMetrics.Domain.Enums;

namespace PTCGBattleMetrics.Application.DTOs;

public record OverviewMetricsDto(
    int TotalMatches,
    int TotalWins,
    int TotalLosses,
    int TotalTies,
    double OverallWinRate,
    double NonTieWinRate,
    List<DeckPerformanceDto> DecksPerformance,
    List<FormatPerformanceDto> FormatsPerformance
);

public record DeckPerformanceDto(
    Guid DeckId,
    string DeckName,
    string Archetype,
    int Matches,
    int Wins,
    int Losses,
    int Ties,
    double WinRate
);

public record FormatPerformanceDto(
    string FormatName,
    int Matches,
    int Wins,
    int Losses,
    int Ties,
    double WinRate
);

public record InitiativeMetricsDto(
    int FirstTotal,
    int FirstWins,
    int FirstLosses,
    int FirstTies,
    double FirstWinRate,
    int SecondTotal,
    int SecondWins,
    int SecondLosses,
    int SecondTies,
    double SecondWinRate,
    List<ArchetypeInitiativeDto> ArchetypeInitiatives
);

public record ArchetypeInitiativeDto(
    string Archetype,
    int FirstMatches,
    double FirstWinRate,
    int SecondMatches,
    double SecondWinRate
);

public record MatchupMatrixItemDto(
    string OpponentArchetype,
    int Matches,
    int Wins,
    int Losses,
    int Ties,
    double WinRate,
    MatchupRating Rating,
    double AvgPlayerPrizesTaken,
    double AvgOpponentPrizesTaken,
    double GoingFirstWinRate,
    double GoingSecondWinRate
);

public record AdvancedTelemetryDto(
    CoinFlipMetricsDto CoinFlip,
    PrizeLossMetricsDto PrizeLosses,
    MulliganCorrelationDto Mulligans,
    List<TechCardImpactDto> TechCardImpacts,
    List<WinConditionBreakdownDto> WinConditions
);

public record CoinFlipMetricsDto(
    int WonCoinFlipCount,
    int WonCoinFlipWins,
    double WonCoinFlipWinRate,
    int LostCoinFlipCount,
    int LostCoinFlipWins,
    double LostCoinFlipWinRate,
    double CoinFlipAdvantageDelta
);

public record PrizeLossMetricsDto(
    double AvgPrizesTakenInLosses,
    int TotalLossesEvaluated,
    int CloseLossesCount,       // 4-5 prizes taken
    int ModerateLossesCount,    // 2-3 prizes taken
    int BlowoutLossesCount      // 0-1 prizes taken
);

public record MulliganCorrelationDto(
    double AvgPlayerMulligans,
    int ZeroMulliganMatches,
    double ZeroMulliganWinRate,
    int OneMulliganMatches,
    double OneMulliganWinRate,
    int TwoOrMoreMulligansMatches,
    double TwoOrMoreMulligansWinRate
);

public record TechCardImpactDto(
    string TechCardName,
    int MatchesWithCard,
    double WinRateWithCard,
    int MatchesWithoutCard,
    double WinRateWithoutCard,
    double ImpactDelta
);

public record WinConditionBreakdownDto(
    WinCondition Condition,
    string ConditionName,
    int Count,
    double Percentage
);
