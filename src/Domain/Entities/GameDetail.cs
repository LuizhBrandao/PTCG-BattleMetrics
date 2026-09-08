using PTCGBattleMetrics.Domain.Enums;

namespace PTCGBattleMetrics.Domain.Entities;

public class GameDetail
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MatchId { get; set; }
    public Match? Match { get; set; }

    public int GameNumber { get; set; } = 1;
    public GameResult Result { get; set; } = GameResult.Win;
    public TurnOrder? TurnOrder { get; set; }
    public int? PlayerPrizesRemaining { get; set; }
    public int? OpponentPrizesRemaining { get; set; }
    public WinCondition? WinCondition { get; set; }
    public string? StartingActivePokemon { get; set; }
    public string? Notes { get; set; }

    public int? PlayerPrizesTaken => PlayerPrizesRemaining.HasValue
        ? Math.Max(0, 6 - PlayerPrizesRemaining.Value)
        : (Result == GameResult.Win ? 6 : null);

    public int? OpponentPrizesTaken => OpponentPrizesRemaining.HasValue
        ? Math.Max(0, 6 - OpponentPrizesRemaining.Value)
        : (Result == GameResult.Loss ? 6 : null);
}
