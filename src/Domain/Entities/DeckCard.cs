using PTCGBattleMetrics.Domain.Enums;

namespace PTCGBattleMetrics.Domain.Entities;

public class DeckCard
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DeckId { get; set; }
    public Deck? Deck { get; set; }

    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public string SetCode { get; set; } = string.Empty;
    public string CollectorNumber { get; set; } = string.Empty;

    public CardType CardType { get; set; }
    public TrainerSubType? TrainerSubType { get; set; }
    public bool IsTechCard { get; set; }
}
