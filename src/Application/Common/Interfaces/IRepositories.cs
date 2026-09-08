using PTCGBattleMetrics.Domain.Entities;

namespace PTCGBattleMetrics.Application.Common.Interfaces;

public interface IMatchRepository
{
    Task<List<Match>> GetAllAsync(Guid? deckId = null, Guid? tournamentId = null, CancellationToken cancellationToken = default);
    Task<Match?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Match match, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<Match> matches, CancellationToken cancellationToken = default);
    Task UpdateAsync(Match match, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IDeckRepository
{
    Task<List<Deck>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Deck?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Deck deck, CancellationToken cancellationToken = default);
    Task UpdateAsync(Deck deck, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface ITournamentRepository
{
    Task<List<Tournament>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Tournament?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Tournament tournament, CancellationToken cancellationToken = default);
    Task UpdateAsync(Tournament tournament, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IMetaArchetypeRepository
{
    Task<List<MetaArchetype>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<MetaArchetype?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}
