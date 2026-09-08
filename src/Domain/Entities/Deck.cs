using PTCGBattleMetrics.Domain.Enums;

namespace PTCGBattleMetrics.Domain.Entities;

public class Deck
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Archetype { get; set; } = string.Empty;
    public string Version { get; set; } = "v1.0";
    public string? RawList { get; set; }
    public int PokemonCount { get; set; }
    public int TrainerCount { get; set; }
    public int EnergyCount { get; set; }
    public int TotalCards => PokemonCount + TrainerCount + EnergyCount;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public List<DeckCard> Cards { get; set; } = new();
    public List<string> TechCards { get; set; } = new();
    public List<Match> Matches { get; set; } = new();

    public void RecalculateCounts()
    {
        PokemonCount = Cards.Where(c => c.CardType == CardType.Pokemon).Sum(c => c.Quantity);
        TrainerCount = Cards.Where(c => c.CardType == CardType.Trainer).Sum(c => c.Quantity);
        EnergyCount = Cards.Where(c => c.CardType == CardType.Energy).Sum(c => c.Quantity);
    }
}
