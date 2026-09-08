namespace PTCGBattleMetrics.Domain.Entities;

public class MetaArchetype
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string PrimaryType { get; set; } = "Colorless"; // Fire, Water, Grass, Lightning, Psychic, Fighting, Darkness, Metal, Dragon, Colorless
    public string ColorHex { get; set; } = "#4F46E5";
    public int Tier { get; set; } = 1;
    public bool IsActiveInStandard { get; set; } = true;
}
