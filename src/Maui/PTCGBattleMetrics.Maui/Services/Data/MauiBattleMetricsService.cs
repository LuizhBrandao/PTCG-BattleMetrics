using System.Net.Http.Json;
using System.Text.Json;
using CommunityToolkit.Mvvm.Messaging;
using PTCGBattleMetrics.Application.DTOs;
using PTCGBattleMetrics.Application.Services;
using PTCGBattleMetrics.Domain.Entities;
using PTCGBattleMetrics.Domain.Enums;
using PTCGBattleMetrics.Maui.Messages;
using PTCGBattleMetrics.Maui.Services.Settings;

namespace PTCGBattleMetrics.Maui.Services.Data;

public class MauiBattleMetricsService : IMauiBattleMetricsService
{
    private readonly HttpClient _httpClient;
    private readonly ISettingsService _settings;
    private readonly IMetricsService _metricsService;

    private const string CachedDecksKey = "maui_cached_decks";
    private const string CachedMatchesKey = "maui_cached_matches";
    private const string CachedTournamentsKey = "maui_cached_tournaments";
    private const string PendingMatchesKey = "maui_pending_matches";

    public bool IsOnline { get; private set; } = true;
    public int PendingSyncCount => GetPendingMatches().Count;

    public MauiBattleMetricsService(HttpClient httpClient, ISettingsService settings, IMetricsService metricsService)
    {
        _httpClient = httpClient;
        _settings = settings;
        _metricsService = metricsService;
        UpdateBaseAddress();
    }

    private void UpdateBaseAddress()
    {
        var endpoint = _settings.ApiEndpointBase.TrimEnd('/') + "/";
        if (Uri.TryCreate(endpoint, UriKind.Absolute, out var uri))
        {
            _httpClient.BaseAddress = uri;
        }
    }

    // DECKS
    public async Task<List<DeckResponse>> GetDecksAsync()
    {
        if (!_settings.OfflineMode)
        {
            try
            {
                UpdateBaseAddress();
                var remote = await _httpClient.GetFromJsonAsync<List<DeckResponse>>("api/decks");
                if (remote != null)
                {
                    IsOnline = true;
                    SaveToPreferences(CachedDecksKey, remote);
                    return remote;
                }
            }
            catch
            {
                IsOnline = false;
            }
        }

        var cached = LoadFromPreferences<List<DeckResponse>>(CachedDecksKey) ?? new();
        if (cached.Count == 0)
        {
            // Seed default deck for standalone local experience
            cached = new List<DeckResponse>
            {
                new(
                    Id: Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Name: "Charizard Pidgeot ex",
                    Archetype: "Charizard ex",
                    Version: "v1.2",
                    PokemonCount: 17,
                    TrainerCount: 36,
                    EnergyCount: 7,
                    TotalCards: 60,
                    CreatedAt: DateTimeOffset.UtcNow,
                    TechCards: new() { "Prime Catcher", "Forest Seal Stone", "Maximum Belt", "Lost Vacuum", "Canceling Cologne", "Fezandipiti ex" },
                    Cards: new()
                )
            };
            SaveToPreferences(CachedDecksKey, cached);
        }
        return cached;
    }

    public async Task<DeckResponse?> CreateDeckAsync(CreateDeckRequest request)
    {
        if (!_settings.OfflineMode)
        {
            try
            {
                UpdateBaseAddress();
                var res = await _httpClient.PostAsJsonAsync("api/decks", request);
                if (res.IsSuccessStatusCode)
                {
                    var created = await res.Content.ReadFromJsonAsync<DeckResponse>();
                    if (created != null)
                    {
                        var list = await GetDecksAsync();
                        list.Insert(0, created);
                        SaveToPreferences(CachedDecksKey, list);
                        _settings.ActiveDeckId = created.Id;
                        WeakReferenceMessenger.Default.Send(new ActiveDeckChangedMessage(created.Id));
                        return created;
                    }
                }
            }
            catch
            {
                IsOnline = false;
            }
        }

        // Local creation
        var local = new DeckResponse(
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

        var cachedList = await GetDecksAsync();
        cachedList.Insert(0, local);
        SaveToPreferences(CachedDecksKey, cachedList);
        _settings.ActiveDeckId = local.Id;
        WeakReferenceMessenger.Default.Send(new ActiveDeckChangedMessage(local.Id));
        return local;
    }

    // ARCHETYPES
    public async Task<List<MetaArchetype>> GetArchetypesAsync()
    {
        if (!_settings.OfflineMode)
        {
            try
            {
                UpdateBaseAddress();
                var list = await _httpClient.GetFromJsonAsync<List<MetaArchetype>>("api/archetypes");
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
        }

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
            new() { Name = "Ancient Box", PrimaryType = "Darkness", ColorHex = "#334155", Tier = 2 }
        };
    }

    // TOURNAMENTS
    public async Task<List<TournamentResponse>> GetTournamentsAsync()
    {
        if (!_settings.OfflineMode)
        {
            try
            {
                UpdateBaseAddress();
                var remote = await _httpClient.GetFromJsonAsync<List<TournamentResponse>>("api/tournaments");
                if (remote != null)
                {
                    IsOnline = true;
                    SaveToPreferences(CachedTournamentsKey, remote);
                    return remote;
                }
            }
            catch
            {
                IsOnline = false;
            }
        }

        return LoadFromPreferences<List<TournamentResponse>>(CachedTournamentsKey) ?? new();
    }

    public async Task<TournamentResponse?> CreateTournamentAsync(CreateTournamentRequest request)
    {
        if (!_settings.OfflineMode)
        {
            try
            {
                UpdateBaseAddress();
                var res = await _httpClient.PostAsJsonAsync("api/tournaments", request);
                if (res.IsSuccessStatusCode)
                {
                    var created = await res.Content.ReadFromJsonAsync<TournamentResponse>();
                    if (created != null)
                    {
                        var list = await GetTournamentsAsync();
                        list.Insert(0, created);
                        SaveToPreferences(CachedTournamentsKey, list);
                        _settings.ActiveTournamentId = created.Id;
                        WeakReferenceMessenger.Default.Send(new ActiveTournamentChangedMessage(created.Id));
                        return created;
                    }
                }
            }
            catch
            {
                IsOnline = false;
            }
        }

        var local = new TournamentResponse(
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

        var cached = await GetTournamentsAsync();
        cached.Insert(0, local);
        SaveToPreferences(CachedTournamentsKey, cached);
        _settings.ActiveTournamentId = local.Id;
        WeakReferenceMessenger.Default.Send(new ActiveTournamentChangedMessage(local.Id));
        return local;
    }

    // MATCHES & OFFLINE RESILIENCE
    public async Task<List<MatchResponse>> GetMatchesAsync(Guid? deckId = null, Guid? tournamentId = null)
    {
        if (!_settings.OfflineMode)
        {
            try
            {
                UpdateBaseAddress();
                var url = "api/matches";
                var qList = new List<string>();
                if (deckId.HasValue) qList.Add($"deckId={deckId.Value}");
                if (tournamentId.HasValue) qList.Add($"tournamentId={tournamentId.Value}");
                if (qList.Count > 0) url += "?" + string.Join("&", qList);

                var remote = await _httpClient.GetFromJsonAsync<List<MatchResponse>>(url);
                if (remote != null)
                {
                    IsOnline = true;
                    SaveToPreferences(CachedMatchesKey, remote);
                    return remote;
                }
            }
            catch
            {
                IsOnline = false;
            }
        }

        var cached = LoadFromPreferences<List<MatchResponse>>(CachedMatchesKey) ?? new();
        if (deckId.HasValue) cached = cached.Where(m => m.DeckId == deckId.Value).ToList();
        if (tournamentId.HasValue) cached = cached.Where(m => m.TournamentId == tournamentId.Value).ToList();
        return cached;
    }

    public async Task<MatchResponse> SaveMatchAsync(CreateMatchRequest request)
    {
        var decks = await GetDecksAsync();
        var deck = decks.FirstOrDefault(d => d.Id == request.DeckId);

        var matchId = Guid.NewGuid();
        var response = new MatchResponse(
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

        // Always store locally first
        var cached = LoadFromPreferences<List<MatchResponse>>(CachedMatchesKey) ?? new();
        cached.Insert(0, response);
        SaveToPreferences(CachedMatchesKey, cached);

        bool synced = false;
        if (!_settings.OfflineMode)
        {
            try
            {
                UpdateBaseAddress();
                var res = await _httpClient.PostAsJsonAsync("api/matches", request);
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
        }

        if (!synced)
        {
            var pending = GetPendingMatches();
            pending.Add(request);
            SaveToPreferences(PendingMatchesKey, pending);
        }

        // Notify subscribers using WeakReferenceMessenger (Chapter 5)
        WeakReferenceMessenger.Default.Send(new MatchLoggedMessage(response));
        return response;
    }

    public async Task<int> SyncPendingMatchesAsync()
    {
        var pending = GetPendingMatches();
        if (pending.Count == 0) return 0;

        try
        {
            UpdateBaseAddress();
            var req = new SyncBatchRequest(pending);
            var res = await _httpClient.PostAsJsonAsync("api/sync", req);
            if (res.IsSuccessStatusCode)
            {
                Preferences.Remove(PendingMatchesKey);
                IsOnline = true;
                return pending.Count;
            }
        }
        catch
        {
            IsOnline = false;
        }

        return 0;
    }

    // ANALYTICS (Offline-capable using shared Application library)
    public async Task<OverviewMetricsDto> GetOverviewMetricsAsync(Guid? deckId = null)
    {
        if (!_settings.OfflineMode)
        {
            try
            {
                UpdateBaseAddress();
                var url = "api/analytics/overview" + (deckId.HasValue ? $"?deckId={deckId.Value}" : "");
                var res = await _httpClient.GetFromJsonAsync<OverviewMetricsDto>(url);
                if (res != null) return res;
            }
            catch { }
        }

        var matches = await GetMatchesAsync(deckId);
        return _metricsService.CalculateOverview(matches.Select(ToDomainMatch));
    }

    public async Task<List<MatchupMatrixItemDto>> GetMatchupMatrixAsync(Guid? deckId = null)
    {
        if (!_settings.OfflineMode)
        {
            try
            {
                UpdateBaseAddress();
                var url = "api/analytics/matchups" + (deckId.HasValue ? $"?deckId={deckId.Value}" : "");
                var res = await _httpClient.GetFromJsonAsync<List<MatchupMatrixItemDto>>(url);
                if (res != null) return res;
            }
            catch { }
        }

        var matches = await GetMatchesAsync(deckId);
        return _metricsService.CalculateMatchupMatrix(matches.Select(ToDomainMatch));
    }

    public async Task<InitiativeMetricsDto> GetInitiativeMetricsAsync(Guid? deckId = null)
    {
        if (!_settings.OfflineMode)
        {
            try
            {
                UpdateBaseAddress();
                var url = "api/analytics/initiative" + (deckId.HasValue ? $"?deckId={deckId.Value}" : "");
                var res = await _httpClient.GetFromJsonAsync<InitiativeMetricsDto>(url);
                if (res != null) return res;
            }
            catch { }
        }

        var matches = await GetMatchesAsync(deckId);
        return _metricsService.CalculateInitiative(matches.Select(ToDomainMatch));
    }

    public async Task<AdvancedTelemetryDto> GetAdvancedTelemetryAsync(Guid? deckId = null)
    {
        if (!_settings.OfflineMode)
        {
            try
            {
                UpdateBaseAddress();
                var url = "api/analytics/telemetry" + (deckId.HasValue ? $"?deckId={deckId.Value}" : "");
                var res = await _httpClient.GetFromJsonAsync<AdvancedTelemetryDto>(url);
                if (res != null) return res;
            }
            catch { }
        }

        var matches = await GetMatchesAsync(deckId);
        var decks = await GetDecksAsync();
        return _metricsService.CalculateAdvancedTelemetry(
            matches.Select(ToDomainMatch),
            decks.Select(d => new Deck { Id = d.Id, Name = d.Name, Archetype = d.Archetype, TechCards = d.TechCards })
        );
    }

    private List<CreateMatchRequest> GetPendingMatches() =>
        LoadFromPreferences<List<CreateMatchRequest>>(PendingMatchesKey) ?? new();

    private static void SaveToPreferences<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value);
        Preferences.Set(key, json);
    }

    private static T? LoadFromPreferences<T>(string key)
    {
        var json = Preferences.Get(key, string.Empty);
        if (string.IsNullOrWhiteSpace(json)) return default;
        try { return JsonSerializer.Deserialize<T>(json); } catch { return default; }
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
