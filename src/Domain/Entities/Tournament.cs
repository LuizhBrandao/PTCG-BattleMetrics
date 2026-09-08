using PTCGBattleMetrics.Domain.Enums;

namespace PTCGBattleMetrics.Domain.Entities;

public class Tournament
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? StoreOrVenue { get; set; }
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public TournamentCategory Category { get; set; } = TournamentCategory.Casual;
    public TournamentFormat Format { get; set; } = TournamentFormat.SwissBo1;
    public int? TotalParticipants { get; set; }
    public int? FinalStanding { get; set; }
    public int? ChampionshipPoints { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public List<Match> Matches { get; set; } = new();

    public int TotalWins => Matches.Count(m => m.Result == MatchResult.Win);
    public int TotalLosses => Matches.Count(m => m.Result == MatchResult.Loss);
    public int TotalTies => Matches.Count(m => m.Result == MatchResult.Tie);
    public int MatchPoints => (TotalWins * 3) + (TotalTies * 1);
    public string RecordDisplay => $"{TotalWins}-{TotalLosses}-{TotalTies}";
}
