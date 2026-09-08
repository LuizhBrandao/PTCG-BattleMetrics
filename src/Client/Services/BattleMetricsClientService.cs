using System.Net.Http.Json;
using PTCGBattleMetrics.Application.DTOs;
using PTCGBattleMetrics.Application.Services;
using PTCGBattleMetrics.Domain.Entities;
using PTCGBattleMetrics.Domain.Enums;

namespace PTCGBattleMetrics.Client.Services;

public class BattleMetricsClientService
{
    private readonly HttpClient _http;
    private readonly ILocalStorageService _storage;
    private readonly IMetricsService _metricsService;

    private const string DecksStorageKey = "ptcg_cached_decks";
    private const string MatchesStorageKey = "ptcg_cached_matches";
    private const string TournamentsStorageKey = "ptcg_cached_tournaments";
    private const string PendingSyncKey = "ptcg_pending_matches";
    private const string ActiveDeckIdKey = "ptcg_active_deck_id";
    private const string ActiveTournamentIdKey = "ptcg_active_tourney_id";

    public event Action? OnDataChanged;

    public bool IsOnline { get; private set; } = true;
    public int PendingSyncCount { get; private set; } = 0;

    public BattleMetricsClientService(HttpClient http, ILocalStorageService storage, IMetricsService metricsService)
    {
        _http = http;
        _storage = storage;
        _metricsService = metricsService;
    }

    public async Task InitializeAsync()
    {
        var pending = await _storage.GetItemAsync<List<CreateMatchRequest>>(PendingSyncKey) ?? new();
        PendingSyncCount = pending.Count;
        OnDataChanged?.Invoke();
    }

    // Active Deck & Tournament state
    public async Task<Guid?> GetActiveDeckIdAsync()
    {
        var str = await _storage.GetItemAsync<string>(ActiveDeckIdKey);
        return Guid.TryParse(str, out var id) ? id : null;
    }

    public async Task SetActiveDeckIdAsync(Guid? id)
    {
        if (id.HasValue)
            await _storage.SetItemAsync(ActiveDeckIdKey, id.Value.ToString());
        else
            await _storage.RemoveItemAsync(ActiveDeckIdKey);
        OnDataChanged?.Invoke();
    }

    public async Task<Guid?> GetActiveTournamentIdAsync()
    {
        var str = await _storage.GetItemAsync<string>(ActiveTournamentIdKey);
        return Guid.TryParse(str, out var id) ? id : null;
    }

    public async Task SetActiveTournamentIdAsync(Guid? id)
    {
        if (id.HasValue)
            await _storage.SetItemAsync(ActiveTournamentIdKey, id.Value.ToString());
        else
            await _storage.RemoveItemAsync(ActiveTournamentIdKey);
        OnDataChanged?.Invoke();
    }

    // Decks
    public async Task<List<DeckResponse>> GetDecksAsync()
    {
        try
        {
            var remote = await _http.GetFromJsonAsync<List<DeckResponse>>("api/decks");
            if (remote != null && remote.Count > 0)
            {
                IsOnline = true;
                await _storage.SetItemAsync(DecksStorageKey, remote);
                return remote;
            }
        }
        catch
        {
            IsOnline = false;
        }

        // Fallback to cache
        var cached = await _storage.GetItemAsync<List<DeckResponse>>(DecksStorageKey) ?? new();
        return cached;
    }

    public async Task<DeckResponse?> CreateDeckAsync(CreateDeckRequest request)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("api/decks", request);
            if (res.IsSuccessStatusCode)
            {
                var created = await res.Content.ReadFromJsonAsync<DeckResponse>();
                if (created != null)
                {
                    var cached = await GetDecksAsync();
                    cached.Insert(0, created);
                    await _storage.SetItemAsync(DecksStorageKey, cached);
                    await SetActiveDeckIdAsync(created.Id);
                    return created;
                }
            }
        }
        catch
        {
            IsOnline = false;
        }

        // Offline deck creation
        var localDeck = new DeckResponse(
            Id: Guid.NewGuid(),
            Name: request.Name,
            Archetype: request.Archetype,
            Version: request.Version,
            PokemonCount: request.Cards?.Where(c => c.CardType == CardType.Pokemon).Sum(c => c.Quantity) ?? 0,
            TrainerCount: request.Cards?.Where(c => c.CardType == CardType.Trainer).Sum(c => c.Quantity) ?? 0,
            EnergyCount: request.Cards?.Where(c => c.CardType == CardType.Energy).Sum(c => c.Quantity) ?? 0,
            TotalCards: request.Cards?.Sum(c => c.Quantity) ?? 0,
            CreatedAt: DateTimeOffset.UtcNow,
            TechCards: request.TechCards ?? new(),
            Cards: request.Cards ?? new()
        );

        var decks = await _storage.GetItemAsync<List<DeckResponse>>(DecksStorageKey) ?? new();
        decks.Insert(0, localDeck);
        await _storage.SetItemAsync(DecksStorageKey, decks);
        await SetActiveDeckIdAsync(localDeck.Id);
        return localDeck;
    }

    // Archetypes
    public async Task<List<MetaArchetype>> GetArchetypesAsync()
    {
        try
        {
            var list = await _http.GetFromJsonAsync<List<MetaArchetype>>("api/archetypes");
            if (list != null && list.Count > 0)
            {
                IsOnline = true;
                return list;
            }
        }
        catch
        {
            IsOnline = false;
        }

        // Fallback meta archetypes
        return new List<MetaArchetype>
        {
            new() { Name = "Charizard ex", PrimaryType = "Fire", ColorHex = "#EF4444", Tier = 1 },
            new() { Name = "Lugia VSTAR", PrimaryType = "Colorless", ColorHex = "#A855F7", Tier = 1 },
            new() { Name = "Gardevoir ex", PrimaryType = "Psychic", ColorHex = "#EC4899", Tier = 1 },
            new() { Name = "Raging Bolt ex", PrimaryType = "Dragon", ColorHex = "#EAB308", Tier = 1 },
            new() { Name = "Dragapult ex", PrimaryType = "Dragon", ColorHex = "#8B5CF6", Tier = 1 },
            new() { Name = "Terapagos ex", PrimaryType = "Colorless", ColorHex = "#06B6D4", Tier = 1 },
            new() { Name = "Regidrago VSTAR", PrimaryType = "Dragon", ColorHex = "#10B981", Tier = 1 },
            new() { Name = "Miraidon ex", PrimaryType = "Lightning", ColorHex = "#F59E0B", Tier = 2 },
            new() { Name = "Roaring Moon ex", PrimaryType = "Darkness", ColorHex = "#475569", Tier = 2 },
            new() { Name = "Gholdengo ex", PrimaryType = "Metal", ColorHex = "#CBD5E1", Tier = 2 },
            new() { Name = "Snorlax Stall", PrimaryType = "Colorless", ColorHex = "#64748B", Tier = 2 },
            new() { Name = "Ancient Box", PrimaryType = "Darkness", ColorHex = "#334155", Tier = 2 },
            new() { Name = "Chien-Pao ex", PrimaryType = "Water", ColorHex = "#38BDF8", Tier = 2 },
            new() { Name = "Palkia VSTAR", PrimaryType = "Water", ColorHex = "#2563EB", Tier = 2 },
            new() { Name = "Gengar ex", PrimaryType = "Darkness", ColorHex = "#581C87", Tier = 3 }
        };
    }

    // Matches & Offline Fast Input
    public async Task<List<MatchResponse>> GetMatchesAsync(Guid? deckId = null, Guid? tournamentId = null)
    {
        try
        {
            var url = "api/matches";
            if (deckId.HasValue || tournamentId.HasValue)
            {
                var queryParams = new List<string>();
                if (deckId.HasValue) queryParams.Add($"deckId={deckId.Value}");
                if (tournamentId.HasValue) queryParams.Add($"tournamentId={tournamentId.Value}");
                url += "?" + string.Join("&", queryParams);
            }

            var remote = await _http.GetFromJsonAsync<List<MatchResponse>>(url);
            if (remote != null)
            {
                IsOnline = true;
                await _storage.SetItemAsync(MatchesStorageKey, remote);
                return remote;
            }
        }
        catch
        {
            IsOnline = false;
        }

        var cached = await _storage.GetItemAsync<List<MatchResponse>>(MatchesStorageKey) ?? new();
        if (deckId.HasValue) cached = cached.Where(m => m.DeckId == deckId.Value).ToList();
        if (tournamentId.HasValue) cached = cached.Where(m => m.TournamentId == tournamentId.Value).ToList();
        return cached;
    }

    public async Task<MatchResponse> SaveMatchAsync(CreateMatchRequest request)
    {
        var matchId = Guid.NewGuid();
        var decks = await GetDecksAsync();
        var deck = decks.FirstOrDefault(d => d.Id == request.DeckId);

        var responseDto = new MatchResponse(
            Id: matchId,
            DeckId: request.DeckId,
            DeckName: deck?.Name ?? "Deck",
            DeckArchetype: deck?.Archetype ?? "Arquetipo",
            TournamentId: request.TournamentId,
            TournamentName: null,
            OpponentArchetype: request.OpponentArchetype,
            Result: request.Result,
            CreatedAt: request.CreatedAt ?? DateTimeOffset.UtcNow,
            RoundNumber: request.RoundNumber,
            TableNumber: request.TableNumber,
            OpponentName: request.OpponentName,
            OpponentPopId: request.OpponentPopId,
            CoinFlipWon: request.CoinFlipWon,
            TurnOrder: request.TurnOrder,
            PlayerMulligans: request.PlayerMulligans,
            OpponentMulligans: request.OpponentMulligans,
            PlayerPrizesRemaining: request.PlayerPrizesRemaining,
            OpponentPrizesRemaining: request.OpponentPrizesRemaining,
            PlayerPrizesTaken: request.PlayerPrizesRemaining.HasValue ? Math.Clamp(6 - request.PlayerPrizesRemaining.Value, 0, 6) : (request.Result == MatchResult.Win ? 6 : null),
            OpponentPrizesTaken: request.OpponentPrizesRemaining.HasValue ? Math.Clamp(6 - request.OpponentPrizesRemaining.Value, 0, 6) : (request.Result == MatchResult.Loss ? 6 : null),
            WinCondition: request.WinCondition,
            StartingActivePokemon: request.StartingActivePokemon,
            TacticalNotes: request.TacticalNotes,
            TechCardsUsed: request.TechCardsUsed ?? new(),
            Games: request.Games ?? new(),
            MatchPoints: request.Result == MatchResult.Win ? 3 : (request.Result == MatchResult.Tie ? 1 : 0)
        );

        // Always save locally immediately
        var cachedMatches = await _storage.GetItemAsync<List<MatchResponse>>(MatchesStorageKey) ?? new();
        cachedMatches.Insert(0, responseDto);
        await _storage.SetItemAsync(MatchesStorageKey, cachedMatches);

        // Attempt API sync
        bool synced = false;
        try
        {
            var res = await _http.PostAsJsonAsync("api/matches", request);
            if (res.IsSuccessStatusCode)
            {
                IsOnline = true;
                synced = true;
            }
        }
        catch
        {
            IsOnline = false;
        }

        if (!synced)
        {
            var pending = await _storage.GetItemAsync<List<CreateMatchRequest>>(PendingSyncKey) ?? new();
            pending.Add(request);
            await _storage.SetItemAsync(PendingSyncKey, pending);
            PendingSyncCount = pending.Count;
        }

        OnDataChanged?.Invoke();
        return responseDto;
    }

    public async Task<int> SyncPendingMatchesAsync()
    {
        var pending = await _storage.GetItemAsync<List<CreateMatchRequest>>(PendingSyncKey) ?? new();
        if (pending.Count == 0) return 0;

        try
        {
            var syncReq = new SyncBatchRequest(pending);
            var res = await _http.PostAsJsonAsync("api/sync", syncReq);
            if (res.IsSuccessStatusCode)
            {
                var result = await res.Content.ReadFromJsonAsync<SyncBatchResponse>();
                await _storage.RemoveItemAsync(PendingSyncKey);
                PendingSyncCount = 0;
                IsOnline = true;
                OnDataChanged?.Invoke();
                return result?.SyncedCount ?? pending.Count;
            }
        }
        catch
        {
            IsOnline = false;
        }

        return 0;
    }

    // Tournaments
    public async Task<List<TournamentResponse>> GetTournamentsAsync()
    {
        try
        {
            var remote = await _http.GetFromJsonAsync<List<TournamentResponse>>("api/tournaments");
            if (remote != null)
            {
                IsOnline = true;
                await _storage.SetItemAsync(TournamentsStorageKey, remote);
                return remote;
            }
        }
        catch
        {
            IsOnline = false;
        }

        return await _storage.GetItemAsync<List<TournamentResponse>>(TournamentsStorageKey) ?? new();
    }

    public async Task<TournamentResponse?> CreateTournamentAsync(CreateTournamentRequest request)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("api/tournaments", request);
            if (res.IsSuccessStatusCode)
            {
                var created = await res.Content.ReadFromJsonAsync<TournamentResponse>();
                if (created != null)
                {
                    var list = await GetTournamentsAsync();
                    list.Insert(0, created);
                    await _storage.SetItemAsync(TournamentsStorageKey, list);
                    await SetActiveTournamentIdAsync(created.Id);
                    return created;
                }
            }
        }
        catch
        {
            IsOnline = false;
        }

        var localTourney = new TournamentResponse(
            Id: Guid.NewGuid(),
            Name: request.Name,
            StoreOrVenue: request.StoreOrVenue,
            Date: request.Date,
            Category: request.Category,
            Format: request.Format,
            TotalParticipants: request.TotalParticipants,
            FinalStanding: request.FinalStanding,
            ChampionshipPoints: request.ChampionshipPoints,
            TotalWins: 0,
            TotalLosses: 0,
            TotalTies: 0,
            MatchPoints: 0,
            RecordDisplay: "0-0-0",
            MatchesCount: 0
        );

        var cached = await _storage.GetItemAsync<List<TournamentResponse>>(TournamentsStorageKey) ?? new();
        cached.Insert(0, localTourney);
        await _storage.SetItemAsync(TournamentsStorageKey, cached);
        await SetActiveTournamentIdAsync(localTourney.Id);
        return localTourney;
    }

    // Local & Remote Analytics Execution
    public async Task<OverviewMetricsDto> GetOverviewMetricsAsync(Guid? deckId = null)
    {
        try
        {
            var url = "api/analytics/overview" + (deckId.HasValue ? $"?deckId={deckId.Value}" : "");
            var res = await _http.GetFromJsonAsync<OverviewMetricsDto>(url);
            if (res != null) return res;
        }
        catch
        {
            // Compute locally in WebAssembly
        }

        var matches = await GetMatchesAsync(deckId);
        var domainMatches = matches.Select(ToDomainMatch).ToList();
        return _metricsService.CalculateOverview(domainMatches);
    }

    public async Task<List<MatchupMatrixItemDto>> GetMatchupMatrixAsync(Guid? deckId = null)
    {
        try
        {
            var url = "api/analytics/matchups" + (deckId.HasValue ? $"?deckId={deckId.Value}" : "");
            var res = await _http.GetFromJsonAsync<List<MatchupMatrixItemDto>>(url);
            if (res != null) return res;
        }
        catch
        {
            // Compute locally
        }

        var matches = await GetMatchesAsync(deckId);
        var domainMatches = matches.Select(ToDomainMatch).ToList();
        return _metricsService.CalculateMatchupMatrix(domainMatches);
    }

    public async Task<InitiativeMetricsDto> GetInitiativeMetricsAsync(Guid? deckId = null)
    {
        try
        {
            var url = "api/analytics/initiative" + (deckId.HasValue ? $"?deckId={deckId.Value}" : "");
            var res = await _http.GetFromJsonAsync<InitiativeMetricsDto>(url);
            if (res != null) return res;
        }
        catch
        {
            // Compute locally
        }

        var matches = await GetMatchesAsync(deckId);
        var domainMatches = matches.Select(ToDomainMatch).ToList();
        return _metricsService.CalculateInitiative(domainMatches);
    }

    public async Task<AdvancedTelemetryDto> GetAdvancedTelemetryAsync(Guid? deckId = null)
    {
        try
        {
            var url = "api/analytics/telemetry" + (deckId.HasValue ? $"?deckId={deckId.Value}" : "");
            var res = await _http.GetFromJsonAsync<AdvancedTelemetryDto>(url);
            if (res != null) return res;
        }
        catch
        {
            // Compute locally
        }

        var matches = await GetMatchesAsync(deckId);
        var domainMatches = matches.Select(ToDomainMatch).ToList();
        var decks = await GetDecksAsync();
        var domainDecks = decks.Select(d => new Deck
        {
            Id = d.Id,
            Name = d.Name,
            Archetype = d.Archetype,
            TechCards = d.TechCards
        }).ToList();

        return _metricsService.CalculateAdvancedTelemetry(domainMatches, domainDecks);
    }

    private static Match ToDomainMatch(MatchResponse m) => new()
    {
        Id = m.Id,
        DeckId = m.DeckId,
        TournamentId = m.TournamentId,
        OpponentArchetype = m.OpponentArchetype,
        Result = m.Result,
        CreatedAt = m.CreatedAt,
        RoundNumber = m.RoundNumber,
        TableNumber = m.TableNumber,
        OpponentName = m.OpponentName,
        OpponentPopId = m.OpponentPopId,
        CoinFlipWon = m.CoinFlipWon,
        TurnOrder = m.TurnOrder,
        PlayerMulligans = m.PlayerMulligans,
        OpponentMulligans = m.OpponentMulligans,
        PlayerPrizesRemaining = m.PlayerPrizesRemaining,
        OpponentPrizesRemaining = m.OpponentPrizesRemaining,
        WinCondition = m.WinCondition,
        StartingActivePokemon = m.StartingActivePokemon,
        TacticalNotes = m.TacticalNotes,
        TechCardsUsed = m.TechCardsUsed
    };
}
