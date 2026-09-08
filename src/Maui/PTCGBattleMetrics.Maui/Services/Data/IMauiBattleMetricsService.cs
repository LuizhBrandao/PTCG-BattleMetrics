using PTCGBattleMetrics.Application.DTOs;
using PTCGBattleMetrics.Domain.Entities;

namespace PTCGBattleMetrics.Maui.Services.Data;

public interface IMauiBattleMetricsService
{
    bool IsOnline { get; }
    int PendingSyncCount { get; }

    Task<List<DeckResponse>> GetDecksAsync();
    Task<DeckResponse?> CreateDeckAsync(CreateDeckRequest request);
    Task<List<MetaArchetype>> GetArchetypesAsync();
    Task<List<TournamentResponse>> GetTournamentsAsync();
    Task<TournamentResponse?> CreateTournamentAsync(CreateTournamentRequest request);
    Task<List<MatchResponse>> GetMatchesAsync(Guid? deckId = null, Guid? tournamentId = null);
    Task<MatchResponse> SaveMatchAsync(CreateMatchRequest request);
    Task<int> SyncPendingMatchesAsync();

    Task<OverviewMetricsDto> GetOverviewMetricsAsync(Guid? deckId = null);
    Task<List<MatchupMatrixItemDto>> GetMatchupMatrixAsync(Guid? deckId = null);
    Task<InitiativeMetricsDto> GetInitiativeMetricsAsync(Guid? deckId = null);
    Task<AdvancedTelemetryDto> GetAdvancedTelemetryAsync(Guid? deckId = null);
}
