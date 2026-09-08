using Microsoft.EntityFrameworkCore;
using PTCGBattleMetrics.Application.Common.Interfaces;
using PTCGBattleMetrics.Application.DTOs;
using PTCGBattleMetrics.Application.Services;
using PTCGBattleMetrics.Domain.Entities;
using PTCGBattleMetrics.Domain.Enums;
using PTCGBattleMetrics.Infrastructure.Persistence;
using PTCGBattleMetrics.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. Database & Persistence
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "ptcg_battle_metrics.db");
builder.Services.AddDbContext<BattleMetricsDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// 2. Application & Infrastructure Services
builder.Services.AddScoped<IMatchRepository, MatchRepository>();
builder.Services.AddScoped<IDeckRepository, DeckRepository>();
builder.Services.AddScoped<ITournamentRepository, TournamentRepository>();
builder.Services.AddScoped<IMetaArchetypeRepository, MetaArchetypeRepository>();
builder.Services.AddScoped<IMetricsService, MetricsService>();

// 3. CORS for Mobile PWA / Blazor WASM
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 4. Swagger / OpenAPI Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "PTCG Battle Metrics API",
        Version = "v1",
        Description = "API REST para rastreamento competitivo de partidas de Pokémon TCG e telemetria avançada."
    });
});

var app = builder.Build();

// Automatically initialize and seed database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BattleMetricsDbContext>();
    await DbInitializer.InitializeAsync(db);
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PTCG Battle Metrics API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("AllowAll");

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

// -------------------------------------------------------------
// API Endpoints
// -------------------------------------------------------------

// DECK ENDPOINTS
var decksApi = app.MapGroup("/api/decks").WithTags("Decks");

decksApi.MapGet("/", async (IDeckRepository repo) =>
{
    var decks = await repo.GetAllAsync();
    var response = decks.Select(d => new DeckResponse(
        d.Id, d.Name, d.Archetype, d.Version,
        d.PokemonCount, d.TrainerCount, d.EnergyCount, d.TotalCards,
        d.CreatedAt, d.TechCards,
        d.Cards.Select(c => new CardDto(c.Name, c.Quantity, c.SetCode, c.CollectorNumber, c.CardType, c.TrainerSubType, c.IsTechCard)).ToList()
    ));
    return Results.Ok(response);
});

decksApi.MapGet("/{id:guid}", async (Guid id, IDeckRepository repo) =>
{
    var d = await repo.GetByIdAsync(id);
    if (d == null) return Results.NotFound();

    var response = new DeckResponse(
        d.Id, d.Name, d.Archetype, d.Version,
        d.PokemonCount, d.TrainerCount, d.EnergyCount, d.TotalCards,
        d.CreatedAt, d.TechCards,
        d.Cards.Select(c => new CardDto(c.Name, c.Quantity, c.SetCode, c.CollectorNumber, c.CardType, c.TrainerSubType, c.IsTechCard)).ToList()
    );
    return Results.Ok(response);
});

decksApi.MapPost("/", async (CreateDeckRequest request, IDeckRepository repo) =>
{
    var deck = new Deck
    {
        Name = request.Name,
        Archetype = request.Archetype,
        Version = string.IsNullOrWhiteSpace(request.Version) ? "v1.0" : request.Version,
        RawList = request.RawList,
        TechCards = request.TechCards ?? new List<string>(),
        Cards = request.Cards?.Select(c => new DeckCard
        {
            Name = c.Name,
            Quantity = c.Quantity,
            SetCode = c.SetCode,
            CollectorNumber = c.CollectorNumber,
            CardType = c.CardType,
            TrainerSubType = c.TrainerSubType,
            IsTechCard = c.IsTechCard
        }).ToList() ?? new List<DeckCard>()
    };

    deck.RecalculateCounts();
    await repo.AddAsync(deck);

    var response = new DeckResponse(
        deck.Id, deck.Name, deck.Archetype, deck.Version,
        deck.PokemonCount, deck.TrainerCount, deck.EnergyCount, deck.TotalCards,
        deck.CreatedAt, deck.TechCards,
        deck.Cards.Select(c => new CardDto(c.Name, c.Quantity, c.SetCode, c.CollectorNumber, c.CardType, c.TrainerSubType, c.IsTechCard)).ToList()
    );
    return Results.Created($"/api/decks/{deck.Id}", response);
});

decksApi.MapPost("/parse-ptcgl", (ParsePtcglRequest request) =>
{
    var result = PtcglDeckParser.Parse(request.RawText);
    return Results.Ok(result);
});

decksApi.MapDelete("/{id:guid}", async (Guid id, IDeckRepository repo) =>
{
    await repo.DeleteAsync(id);
    return Results.NoContent();
});

// MATCH ENDPOINTS (Fast Input + Detailed Telemetry)
var matchesApi = app.MapGroup("/api/matches").WithTags("Matches");

matchesApi.MapGet("/", async (Guid? deckId, Guid? tournamentId, IMatchRepository repo) =>
{
    var matches = await repo.GetAllAsync(deckId, tournamentId);
    var response = matches.Select(m => new MatchResponse(
        m.Id, m.DeckId, m.Deck?.Name ?? "Deck", m.Deck?.Archetype ?? "Arquetipo",
        m.TournamentId, m.Tournament?.Name,
        m.OpponentArchetype, m.Result, m.CreatedAt,
        m.RoundNumber, m.TableNumber, m.OpponentName, m.OpponentPopId,
        m.CoinFlipWon, m.TurnOrder, m.PlayerMulligans, m.OpponentMulligans,
        m.PlayerPrizesRemaining, m.OpponentPrizesRemaining,
        m.PlayerPrizesTaken, m.OpponentPrizesTaken,
        m.WinCondition, m.StartingActivePokemon, m.TacticalNotes,
        m.TechCardsUsed,
        m.Games.Select(g => new CreateGameDetailRequest(
            g.GameNumber, g.Result, g.TurnOrder, g.PlayerPrizesRemaining, g.OpponentPrizesRemaining,
            g.WinCondition, g.StartingActivePokemon, g.Notes)).ToList(),
        m.MatchPoints
    ));
    return Results.Ok(response);
});

matchesApi.MapGet("/{id:guid}", async (Guid id, IMatchRepository repo) =>
{
    var m = await repo.GetByIdAsync(id);
    if (m == null) return Results.NotFound();

    var response = new MatchResponse(
        m.Id, m.DeckId, m.Deck?.Name ?? "Deck", m.Deck?.Archetype ?? "Arquetipo",
        m.TournamentId, m.Tournament?.Name,
        m.OpponentArchetype, m.Result, m.CreatedAt,
        m.RoundNumber, m.TableNumber, m.OpponentName, m.OpponentPopId,
        m.CoinFlipWon, m.TurnOrder, m.PlayerMulligans, m.OpponentMulligans,
        m.PlayerPrizesRemaining, m.OpponentPrizesRemaining,
        m.PlayerPrizesTaken, m.OpponentPrizesTaken,
        m.WinCondition, m.StartingActivePokemon, m.TacticalNotes,
        m.TechCardsUsed,
        m.Games.Select(g => new CreateGameDetailRequest(
            g.GameNumber, g.Result, g.TurnOrder, g.PlayerPrizesRemaining, g.OpponentPrizesRemaining,
            g.WinCondition, g.StartingActivePokemon, g.Notes)).ToList(),
        m.MatchPoints
    );
    return Results.Ok(response);
});

matchesApi.MapPost("/", async (CreateMatchRequest request, IMatchRepository repo, IDeckRepository deckRepo) =>
{
    var match = new Match
    {
        DeckId = request.DeckId,
        TournamentId = request.TournamentId,
        OpponentArchetype = request.OpponentArchetype,
        Result = request.Result,
        CreatedAt = request.CreatedAt ?? DateTimeOffset.UtcNow,
        RoundNumber = request.RoundNumber,
        TableNumber = request.TableNumber,
        OpponentName = request.OpponentName,
        OpponentPopId = request.OpponentPopId,
        CoinFlipWon = request.CoinFlipWon,
        TurnOrder = request.TurnOrder,
        PlayerMulligans = request.PlayerMulligans,
        OpponentMulligans = request.OpponentMulligans,
        PlayerPrizesRemaining = request.PlayerPrizesRemaining,
        OpponentPrizesRemaining = request.OpponentPrizesRemaining,
        WinCondition = request.WinCondition,
        StartingActivePokemon = request.StartingActivePokemon,
        TacticalNotes = request.TacticalNotes,
        TechCardsUsed = request.TechCardsUsed ?? new List<string>(),
        Games = request.Games?.Select(g => new GameDetail
        {
            GameNumber = g.GameNumber,
            Result = g.Result,
            TurnOrder = g.TurnOrder,
            PlayerPrizesRemaining = g.PlayerPrizesRemaining,
            OpponentPrizesRemaining = g.OpponentPrizesRemaining,
            WinCondition = g.WinCondition,
            StartingActivePokemon = g.StartingActivePokemon,
            Notes = g.Notes
        }).ToList() ?? new List<GameDetail>()
    };

    await repo.AddAsync(match);

    var createdMatch = await repo.GetByIdAsync(match.Id);
    var response = new MatchResponse(
        createdMatch!.Id, createdMatch.DeckId, createdMatch.Deck?.Name ?? "", createdMatch.Deck?.Archetype ?? "",
        createdMatch.TournamentId, createdMatch.Tournament?.Name,
        createdMatch.OpponentArchetype, createdMatch.Result, createdMatch.CreatedAt,
        createdMatch.RoundNumber, createdMatch.TableNumber, createdMatch.OpponentName, createdMatch.OpponentPopId,
        createdMatch.CoinFlipWon, createdMatch.TurnOrder, createdMatch.PlayerMulligans, createdMatch.OpponentMulligans,
        createdMatch.PlayerPrizesRemaining, createdMatch.OpponentPrizesRemaining,
        createdMatch.PlayerPrizesTaken, createdMatch.OpponentPrizesTaken,
        createdMatch.WinCondition, createdMatch.StartingActivePokemon, createdMatch.TacticalNotes,
        createdMatch.TechCardsUsed,
        createdMatch.Games.Select(g => new CreateGameDetailRequest(
            g.GameNumber, g.Result, g.TurnOrder, g.PlayerPrizesRemaining, g.OpponentPrizesRemaining,
            g.WinCondition, g.StartingActivePokemon, g.Notes)).ToList(),
        createdMatch.MatchPoints
    );

    return Results.Created($"/api/matches/{match.Id}", response);
});

matchesApi.MapDelete("/{id:guid}", async (Guid id, IMatchRepository repo) =>
{
    await repo.DeleteAsync(id);
    return Results.NoContent();
});

// TOURNAMENT ENDPOINTS
var tourneysApi = app.MapGroup("/api/tournaments").WithTags("Tournaments");

tourneysApi.MapGet("/", async (ITournamentRepository repo) =>
{
    var tourneys = await repo.GetAllAsync();
    var response = tourneys.Select(t => new TournamentResponse(
        t.Id, t.Name, t.StoreOrVenue, t.Date, t.Category, t.Format,
        t.TotalParticipants, t.FinalStanding, t.ChampionshipPoints,
        t.TotalWins, t.TotalLosses, t.TotalTies, t.MatchPoints,
        t.RecordDisplay, t.Matches.Count
    ));
    return Results.Ok(response);
});

tourneysApi.MapGet("/{id:guid}", async (Guid id, ITournamentRepository repo) =>
{
    var t = await repo.GetByIdAsync(id);
    if (t == null) return Results.NotFound();

    var response = new TournamentResponse(
        t.Id, t.Name, t.StoreOrVenue, t.Date, t.Category, t.Format,
        t.TotalParticipants, t.FinalStanding, t.ChampionshipPoints,
        t.TotalWins, t.TotalLosses, t.TotalTies, t.MatchPoints,
        t.RecordDisplay, t.Matches.Count
    );
    return Results.Ok(response);
});

tourneysApi.MapPost("/", async (CreateTournamentRequest request, ITournamentRepository repo) =>
{
    var t = new Tournament
    {
        Name = request.Name,
        StoreOrVenue = request.StoreOrVenue,
        Date = request.Date,
        Category = request.Category,
        Format = request.Format,
        TotalParticipants = request.TotalParticipants,
        FinalStanding = request.FinalStanding,
        ChampionshipPoints = request.ChampionshipPoints,
        Notes = request.Notes
    };

    await repo.AddAsync(t);

    var response = new TournamentResponse(
        t.Id, t.Name, t.StoreOrVenue, t.Date, t.Category, t.Format,
        t.TotalParticipants, t.FinalStanding, t.ChampionshipPoints,
        0, 0, 0, 0, "0-0-0", 0
    );
    return Results.Created($"/api/tournaments/{t.Id}", response);
});

tourneysApi.MapDelete("/{id:guid}", async (Guid id, ITournamentRepository repo) =>
{
    await repo.DeleteAsync(id);
    return Results.NoContent();
});

// META ARCHETYPES ENDPOINT (For Fast Input pills)
app.MapGet("/api/archetypes", async (IMetaArchetypeRepository repo) =>
{
    var archetypes = await repo.GetAllAsync();
    return Results.Ok(archetypes);
}).WithTags("Archetypes");

// ANALYTICS ENDPOINTS
var analyticsApi = app.MapGroup("/api/analytics").WithTags("Analytics");

analyticsApi.MapGet("/overview", async (Guid? deckId, IMatchRepository matchRepo, IMetricsService metricsService) =>
{
    var matches = await matchRepo.GetAllAsync(deckId);
    var overview = metricsService.CalculateOverview(matches);
    return Results.Ok(overview);
});

analyticsApi.MapGet("/matchups", async (Guid? deckId, IMatchRepository matchRepo, IMetricsService metricsService) =>
{
    var matches = await matchRepo.GetAllAsync(deckId);
    var matrix = metricsService.CalculateMatchupMatrix(matches);
    return Results.Ok(matrix);
});

analyticsApi.MapGet("/initiative", async (Guid? deckId, IMatchRepository matchRepo, IMetricsService metricsService) =>
{
    var matches = await matchRepo.GetAllAsync(deckId);
    var initiative = metricsService.CalculateInitiative(matches);
    return Results.Ok(initiative);
});

analyticsApi.MapGet("/telemetry", async (Guid? deckId, IMatchRepository matchRepo, IDeckRepository deckRepo, IMetricsService metricsService) =>
{
    var matches = await matchRepo.GetAllAsync(deckId);
    var decks = await deckRepo.GetAllAsync();
    var telemetry = metricsService.CalculateAdvancedTelemetry(matches, decks);
    return Results.Ok(telemetry);
});

// OFFLINE SYNC BATCH ENDPOINT
app.MapPost("/api/sync", async (SyncBatchRequest request, IMatchRepository matchRepo) =>
{
    var createdIds = new List<Guid>();
    if (request.OfflineMatches != null && request.OfflineMatches.Any())
    {
        var entities = request.OfflineMatches.Select(req => new Match
        {
            DeckId = req.DeckId,
            TournamentId = req.TournamentId,
            OpponentArchetype = req.OpponentArchetype,
            Result = req.Result,
            CreatedAt = req.CreatedAt ?? DateTimeOffset.UtcNow,
            RoundNumber = req.RoundNumber,
            TableNumber = req.TableNumber,
            OpponentName = req.OpponentName,
            OpponentPopId = req.OpponentPopId,
            CoinFlipWon = req.CoinFlipWon,
            TurnOrder = req.TurnOrder,
            PlayerMulligans = req.PlayerMulligans,
            OpponentMulligans = req.OpponentMulligans,
            PlayerPrizesRemaining = req.PlayerPrizesRemaining,
            OpponentPrizesRemaining = req.OpponentPrizesRemaining,
            WinCondition = req.WinCondition,
            StartingActivePokemon = req.StartingActivePokemon,
            TacticalNotes = req.TacticalNotes,
            TechCardsUsed = req.TechCardsUsed ?? new List<string>()
        }).ToList();

        await matchRepo.AddRangeAsync(entities);
        createdIds.AddRange(entities.Select(e => e.Id));
    }

    return Results.Ok(new SyncBatchResponse(createdIds.Count, createdIds));
}).WithTags("Sync");

app.MapFallbackToFile("index.html");

app.Run();

public class ParsePtcglRequest
{
    public string RawText { get; set; } = string.Empty;
}
