using PTCGBattleMetrics.Domain.Entities;

namespace PTCGBattleMetrics.Application.Services;

public static class ArchetypeHelper
{
    public static List<MetaArchetype> GetLimitlessTop10() => new()
    {
        new() { Name = "Dragapult", PrimaryType = "Dragon", ColorHex = "#8B5CF6", Tier = 1 },
        new() { Name = "Mega Excadrill", PrimaryType = "Fighting", ColorHex = "#B45309", Tier = 2 },
        new() { Name = "Alakazam", PrimaryType = "Psychic", ColorHex = "#EC4899", Tier = 3 },
        new() { Name = "Slowking", PrimaryType = "Psychic", ColorHex = "#06B6D4", Tier = 4 },
        new() { Name = "N's Zoroark", PrimaryType = "Darkness", ColorHex = "#334155", Tier = 5 },
        new() { Name = "Festival Lead", PrimaryType = "Grass", ColorHex = "#10B981", Tier = 6 },
        new() { Name = "Dhelmise", PrimaryType = "Grass", ColorHex = "#059669", Tier = 7 },
        new() { Name = "Marnie's Grimmsnarl", PrimaryType = "Darkness", ColorHex = "#4C1D95", Tier = 8 },
        new() { Name = "Mega Lucario", PrimaryType = "Fighting", ColorHex = "#D97706", Tier = 9 },
        new() { Name = "Toucannon", PrimaryType = "Colorless", ColorHex = "#64748B", Tier = 10 }
    };

    public static List<MetaArchetype> GetOrderedArchetypes(IEnumerable<MetaArchetype> baseArchetypes, string? lastFacedArchetype)
    {
        var baseList = baseArchetypes?.ToList() ?? new List<MetaArchetype>();
        if (string.IsNullOrWhiteSpace(lastFacedArchetype))
        {
            return new List<MetaArchetype>(baseList);
        }

        var trimmed = lastFacedArchetype.Trim();
        var result = new List<MetaArchetype>();

        var existing = baseList.FirstOrDefault(a => string.Equals(a.Name, trimmed, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            result.Add(existing);
            foreach (var a in baseList)
            {
                if (!string.Equals(a.Name, trimmed, StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(a);
                }
            }
        }
        else
        {
            result.Add(new MetaArchetype
            {
                Name = trimmed,
                PrimaryType = "Recent",
                ColorHex = "#6366F1",
                Tier = 1
            });
            result.AddRange(baseList);
        }

        return result;
    }
}
