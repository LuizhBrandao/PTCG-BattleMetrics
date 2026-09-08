using PTCGBattleMetrics.Domain.Enums;

namespace PTCGBattleMetrics.Domain.Entities;

public class Match
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Fast Input Mandatory Fields
    public Guid DeckId { get; set; }
    public Deck? Deck { get; set; }

    public Guid? TournamentId { get; set; }
    public Tournament? Tournament { get; set; }

    public string OpponentArchetype { get; set; } = string.Empty;
    public MatchResult Result { get; set; } = MatchResult.Win;

    // Progressive Disclosure: Detailed Telemetry (Optional)
    public int? RoundNumber { get; set; }
    public int? TableNumber { get; set; }
    public string? OpponentName { get; set; }
    public string? OpponentPopId { get; set; }

    public bool? CoinFlipWon { get; set; }
    public TurnOrder? TurnOrder { get; set; }

    public int? PlayerMulligans { get; set; }
    public int? OpponentMulligans { get; set; }

    public int? PlayerPrizesRemaining { get; set; }
    public int? OpponentPrizesRemaining { get; set; }

    public WinCondition? WinCondition { get; set; }
    public string? StartingActivePokemon { get; set; }
    public string? TacticalNotes { get; set; }

    public List<string> TechCardsUsed { get; set; } = new();
    public List<GameDetail> Games { get; set; } = new();

    // Domain helpers & calculated properties
    public int? PlayerPrizesTaken => PlayerPrizesRemaining.HasValue
        ? Math.Clamp(6 - PlayerPrizesRemaining.Value, 0, 6)
        : (Result == MatchResult.Win ? 6 : null);

    public int? OpponentPrizesTaken => OpponentPrizesRemaining.HasValue
        ? Math.Clamp(6 - OpponentPrizesRemaining.Value, 0, 6)
        : (Result == MatchResult.Loss ? 6 : null);

    public int MatchPoints => Result switch
    {
        MatchResult.Win => 3,
        MatchResult.Tie => 1,
        _ => 0
    };
}
