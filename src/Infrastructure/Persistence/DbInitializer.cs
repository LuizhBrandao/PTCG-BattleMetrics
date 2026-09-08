using Microsoft.EntityFrameworkCore;
using PTCGBattleMetrics.Application.Services;
using PTCGBattleMetrics.Domain.Entities;
using PTCGBattleMetrics.Domain.Enums;

namespace PTCGBattleMetrics.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task InitializeAsync(BattleMetricsDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        // Seed Meta Archetypes if empty
        if (!await context.MetaArchetypes.AnyAsync())
        {
            var metaArchetypes = new List<MetaArchetype>
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
                new() { Name = "Gengar ex", PrimaryType = "Darkness", ColorHex = "#581C87", Tier = 3 },
                new() { Name = "Glaceon / Froslass", PrimaryType = "Water", ColorHex = "#7DD3FC", Tier = 3 },
                new() { Name = "Banette ex", PrimaryType = "Psychic", ColorHex = "#9333EA", Tier = 3 }
            };
            await context.MetaArchetypes.AddRangeAsync(metaArchetypes);
            await context.SaveChangesAsync();
        }

        // Seed Sample Deck & Tournament with Matches if empty
        if (!await context.Decks.AnyAsync())
        {
            var rawZardList = """
                Pokémon: 10
                4 Charmander MEW 4
                1 Charmeleon MEW 5
                3 Charizard ex OBF 125
                2 Pidgey MEW 16
                2 Pidgeot ex OBF 164
                1 Lumineon V BRS 40
                1 Rotom V CRZ 45
                1 Radiant Charizard PGO 11
                1 Fezandipiti ex SFA 38
                1 Cleffa OBF 80

                Trainer: 18
                4 Arven OBF 186
                3 Boss's Orders PAL 172
                2 Iono PAL 185
                1 Professor's Research SVI 189
                4 Ultra Ball SVI 196
                4 Rare Candy SVI 191
                4 Buddy-Buddy Poffin TEF 144
                2 Nest Ball SVI 181
                2 Super Rod PAL 188
                1 Counter Catcher PAR 160
                1 Lost Vacuum CRZ 135
                1 Pal Pad SVI 182
                1 Prime Catcher TEF 157
                1 Forest Seal Stone SIT 156
                1 Maximum Belt TEF 154
                1 Defiance Band SVI 169
                2 Artazon PAL 171
                1 Collapsed Stadium BRS 137

                Energy: 2
                6 Basic {R} Energy SVE 2
                1 Mist Energy TEF 161

                Total Cards: 60
                """;

            var parseResult = PtcglDeckParser.Parse(rawZardList);

            var defaultDeck = new Deck
            {
                Id = Guid.NewGuid(),
                Name = "Charizard Pidgeot ex",
                Archetype = "Charizard ex",
                Version = "v1.2",
                RawList = rawZardList,
                PokemonCount = parseResult.PokemonCount,
                TrainerCount = parseResult.TrainerCount,
                EnergyCount = parseResult.EnergyCount,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-14),
                TechCards = new() { "Prime Catcher", "Forest Seal Stone", "Maximum Belt", "Lost Vacuum", "Canceling Cologne", "Fezandipiti ex" },
                Cards = parseResult.Cards.Select(c => new DeckCard
                {
                    Name = c.Name,
                    Quantity = c.Quantity,
                    SetCode = c.SetCode,
                    CollectorNumber = c.CollectorNumber,
                    CardType = c.CardType,
                    TrainerSubType = c.TrainerSubType,
                    IsTechCard = c.IsTechCard
                }).ToList()
            };

            await context.Decks.AddAsync(defaultDeck);
            await context.SaveChangesAsync();

            // Seed Sample Tournament (League Cup)
            var sampleTournament = new Tournament
            {
                Id = Guid.NewGuid(),
                Name = "League Cup Nerdz Games",
                StoreOrVenue = "Nerdz Arena",
                Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)),
                Category = TournamentCategory.LeagueCup,
                Format = TournamentFormat.SwissBo3,
                TotalParticipants = 32,
                FinalStanding = 2,
                ChampionshipPoints = 40,
                Notes = "Excelente torneio! Final disputada contra Lugia VSTAR."
            };

            await context.Tournaments.AddAsync(sampleTournament);
            await context.SaveChangesAsync();

            // Seed Sample Matches with full telemetry
            var sampleMatches = new List<Match>
            {
                new()
                {
                    DeckId = defaultDeck.Id,
                    TournamentId = sampleTournament.Id,
                    RoundNumber = 1,
                    TableNumber = 12,
                    OpponentName = "Lucas Silva",
                    OpponentArchetype = "Gardevoir ex",
                    Result = MatchResult.Win,
                    CoinFlipWon = true,
                    TurnOrder = TurnOrder.First,
                    PlayerMulligans = 0,
                    OpponentMulligans = 1,
                    PlayerPrizesRemaining = 0,
                    OpponentPrizesRemaining = 3,
                    WinCondition = WinCondition.PrizeKnockout,
                    StartingActivePokemon = "Charmander",
                    TacticalNotes = "Setup rápido no T2. Boss no Gardevoir ex fechou o jogo.",
                    TechCardsUsed = new() { "Prime Catcher", "Forest Seal Stone" },
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-2).AddHours(-5)
                },
                new()
                {
                    DeckId = defaultDeck.Id,
                    TournamentId = sampleTournament.Id,
                    RoundNumber = 2,
                    TableNumber = 5,
                    OpponentName = "Guilherme Costa",
                    OpponentArchetype = "Raging Bolt ex",
                    Result = MatchResult.Win,
                    CoinFlipWon = false,
                    TurnOrder = TurnOrder.Second,
                    PlayerMulligans = 1,
                    OpponentMulligans = 0,
                    PlayerPrizesRemaining = 0,
                    OpponentPrizesRemaining = 2,
                    WinCondition = WinCondition.PrizeKnockout,
                    StartingActivePokemon = "Pidgey",
                    TacticalNotes = "Uso de Maximum Belt no Radiant Charizard para nocautear Ogerpon ex.",
                    TechCardsUsed = new() { "Maximum Belt", "Fezandipiti ex" },
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-2).AddHours(-4)
                },
                new()
                {
                    DeckId = defaultDeck.Id,
                    TournamentId = sampleTournament.Id,
                    RoundNumber = 3,
                    TableNumber = 2,
                    OpponentName = "Matheus Oliveira",
                    OpponentArchetype = "Dragapult ex",
                    Result = MatchResult.Loss,
                    CoinFlipWon = false,
                    TurnOrder = TurnOrder.Second,
                    PlayerMulligans = 2,
                    OpponentMulligans = 0,
                    PlayerPrizesRemaining = 2, // Close loss (took 4 prizes)
                    OpponentPrizesRemaining = 0,
                    WinCondition = WinCondition.PrizeKnockout,
                    StartingActivePokemon = "Rotom V",
                    TacticalNotes = "Phantom Dive espalhou dano nos Charmanders do banco. Perdi por sequenciamento.",
                    TechCardsUsed = new() { "Forest Seal Stone" },
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-2).AddHours(-3)
                },
                new()
                {
                    DeckId = defaultDeck.Id,
                    TournamentId = sampleTournament.Id,
                    RoundNumber = 4,
                    TableNumber = 4,
                    OpponentName = "Rafael Santos",
                    OpponentArchetype = "Terapagos ex",
                    Result = MatchResult.Win,
                    CoinFlipWon = true,
                    TurnOrder = TurnOrder.First,
                    PlayerMulligans = 0,
                    OpponentMulligans = 0,
                    PlayerPrizesRemaining = 0,
                    OpponentPrizesRemaining = 4,
                    WinCondition = WinCondition.Concede,
                    StartingActivePokemon = "Charmander",
                    TacticalNotes = "Lost Vacuum tirou a Area Zero Underdepths no momento crítico. Oponente concedeu.",
                    TechCardsUsed = new() { "Lost Vacuum", "Prime Catcher" },
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-2).AddHours(-2)
                },
                new()
                {
                    DeckId = defaultDeck.Id,
                    TournamentId = sampleTournament.Id,
                    RoundNumber = 5,
                    TableNumber = 1,
                    OpponentName = "Felipe Almeida",
                    OpponentArchetype = "Lugia VSTAR",
                    Result = MatchResult.Win,
                    CoinFlipWon = true,
                    TurnOrder = TurnOrder.First,
                    PlayerMulligans = 0,
                    OpponentMulligans = 0,
                    PlayerPrizesRemaining = 0,
                    OpponentPrizesRemaining = 1,
                    WinCondition = WinCondition.PrizeKnockout,
                    StartingActivePokemon = "Charmander",
                    TacticalNotes = "Partida épica. Fezandipiti ex garantiu a compra de 3 cartas após nocaute do Pidgeot.",
                    TechCardsUsed = new() { "Fezandipiti ex", "Prime Catcher" },
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-2).AddHours(-1)
                },
                // Casual training matches
                new()
                {
                    DeckId = defaultDeck.Id,
                    OpponentArchetype = "Miraidon ex",
                    Result = MatchResult.Win,
                    CoinFlipWon = true,
                    TurnOrder = TurnOrder.First,
                    PlayerMulligans = 0,
                    OpponentMulligans = 0,
                    PlayerPrizesRemaining = 0,
                    OpponentPrizesRemaining = 2,
                    WinCondition = WinCondition.PrizeKnockout,
                    StartingActivePokemon = "Charmander",
                    TechCardsUsed = new() { "Prime Catcher" },
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-1)
                },
                new()
                {
                    DeckId = defaultDeck.Id,
                    OpponentArchetype = "Snorlax Stall",
                    Result = MatchResult.Tie,
                    CoinFlipWon = false,
                    TurnOrder = TurnOrder.Second,
                    PlayerMulligans = 0,
                    OpponentMulligans = 0,
                    PlayerPrizesRemaining = 3,
                    OpponentPrizesRemaining = 6,
                    WinCondition = WinCondition.TimeoutSuddenDeath,
                    StartingActivePokemon = "Pidgey",
                    TacticalNotes = "Block de Snorlax atrasou o jogo, timeout na rodada de 30 minutos.",
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-1).AddHours(2)
                }
            };

            await context.Matches.AddRangeAsync(sampleMatches);
            await context.SaveChangesAsync();
        }
    }
}
