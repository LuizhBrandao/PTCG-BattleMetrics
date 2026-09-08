using Microsoft.EntityFrameworkCore;
using PTCGBattleMetrics.Application.Common.Interfaces;
using PTCGBattleMetrics.Domain.Entities;
using PTCGBattleMetrics.Infrastructure.Persistence;

namespace PTCGBattleMetrics.Infrastructure.Repositories;

public class MatchRepository : IMatchRepository
{
    private readonly BattleMetricsDbContext _context;

    public MatchRepository(BattleMetricsDbContext context)
    {
        _context = context;
    }

    public async Task<List<Match>> GetAllAsync(Guid? deckId = null, Guid? tournamentId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Matches
            .Include(m => m.Deck)
            .Include(m => m.Tournament)
            .Include(m => m.Games)
            .AsQueryable();

        if (deckId.HasValue)
            query = query.Where(m => m.DeckId == deckId.Value);

        if (tournamentId.HasValue)
            query = query.Where(m => m.TournamentId == tournamentId.Value);

        var matches = await query.ToListAsync(cancellationToken);
        return matches.OrderByDescending(m => m.CreatedAt).ToList();
    }

    public async Task<Match?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Matches
            .Include(m => m.Deck)
            .Include(m => m.Tournament)
            .Include(m => m.Games)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task AddAsync(Match match, CancellationToken cancellationToken = default)
    {
        await _context.Matches.AddAsync(match, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<Match> matches, CancellationToken cancellationToken = default)
    {
        await _context.Matches.AddRangeAsync(matches, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Match match, CancellationToken cancellationToken = default)
    {
        _context.Matches.Update(match);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var match = await _context.Matches.FindAsync(new object[] { id }, cancellationToken);
        if (match != null)
        {
            _context.Matches.Remove(match);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

public class DeckRepository : IDeckRepository
{
    private readonly BattleMetricsDbContext _context;

    public DeckRepository(BattleMetricsDbContext context)
    {
        _context = context;
    }

    public async Task<List<Deck>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var decks = await _context.Decks
            .Include(d => d.Cards)
            .ToListAsync(cancellationToken);
        return decks.OrderByDescending(d => d.CreatedAt).ToList();
    }

    public async Task<Deck?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Decks
            .Include(d => d.Cards)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task AddAsync(Deck deck, CancellationToken cancellationToken = default)
    {
        await _context.Decks.AddAsync(deck, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Deck deck, CancellationToken cancellationToken = default)
    {
        _context.Decks.Update(deck);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var deck = await _context.Decks.FindAsync(new object[] { id }, cancellationToken);
        if (deck != null)
        {
            _context.Decks.Remove(deck);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

public class TournamentRepository : ITournamentRepository
{
    private readonly BattleMetricsDbContext _context;

    public TournamentRepository(BattleMetricsDbContext context)
    {
        _context = context;
    }

    public async Task<List<Tournament>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tourneys = await _context.Tournaments
            .Include(t => t.Matches)
            .ToListAsync(cancellationToken);
        return tourneys.OrderByDescending(t => t.Date).ToList();
    }

    public async Task<Tournament?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tournaments
            .Include(t => t.Matches)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task AddAsync(Tournament tournament, CancellationToken cancellationToken = default)
    {
        await _context.Tournaments.AddAsync(tournament, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Tournament tournament, CancellationToken cancellationToken = default)
    {
        _context.Tournaments.Update(tournament);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tournament = await _context.Tournaments.FindAsync(new object[] { id }, cancellationToken);
        if (tournament != null)
        {
            _context.Tournaments.Remove(tournament);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

public class MetaArchetypeRepository : IMetaArchetypeRepository
{
    private readonly BattleMetricsDbContext _context;

    public MetaArchetypeRepository(BattleMetricsDbContext context)
    {
        _context = context;
    }

    public async Task<List<MetaArchetype>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.MetaArchetypes
            .Where(a => a.IsActiveInStandard)
            .OrderBy(a => a.Tier)
            .ThenBy(a => a.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<MetaArchetype?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.MetaArchetypes
            .FirstOrDefaultAsync(a => a.Name.ToLower() == name.ToLower(), cancellationToken);
    }
}
