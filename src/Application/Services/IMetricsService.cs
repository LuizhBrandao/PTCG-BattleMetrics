using PTCGBattleMetrics.Application.DTOs;
using PTCGBattleMetrics.Domain.Entities;

namespace PTCGBattleMetrics.Application.Services;

public interface IMetricsService
{
    OverviewMetricsDto CalculateOverview(IEnumerable<Match> matches);
    InitiativeMetricsDto CalculateInitiative(IEnumerable<Match> matches);
    List<MatchupMatrixItemDto> CalculateMatchupMatrix(IEnumerable<Match> matches);
    AdvancedTelemetryDto CalculateAdvancedTelemetry(IEnumerable<Match> matches, IEnumerable<Deck>? decks = null);
}
