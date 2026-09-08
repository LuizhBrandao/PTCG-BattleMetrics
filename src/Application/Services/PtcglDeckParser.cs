using PTCGBattleMetrics.Application.DTOs;
using PTCGBattleMetrics.Domain.Entities;
using PTCGBattleMetrics.Domain.Enums;

namespace PTCGBattleMetrics.Application.Services;

public static class PtcglDeckParser
{
    private static readonly HashSet<string> KnownSupporters = new(StringComparer.OrdinalIgnoreCase)
    {
        "Arven", "Boss's Orders", "Iono", "Professor's Research", "Professor Sada's Vitality",
        "Colress's Tenacity", "Colress's Experiment", "Carmine", "Kieran", "Ciphermaniac's Codebreaking",
        "Eri", "Briar", "Crispin", "Janine's Secret Art", "Penny", "Roxanne", "Irida", "Melony",
        "Gardenia's Vigor", "Judge", "Thorton", "Atticus", "Larry", "Nemona", "Roseanne's Backup",
        "Morty's Conviction", "Giacomo", "Tulip", "Clavell", "Geeta", "Explorer's Guidance",
        "Lacey", "Drayton", "Professor Turo's Scenario", "Cynthia's Ambition", "Peony", "Raihan",
        "Klara", "Avery", "Cheren's Care", "Grant", "Zisu", "Adaman", "Candice", "Serena"
    };

    private static readonly HashSet<string> KnownStadiums = new(StringComparer.OrdinalIgnoreCase)
    {
        "Artazon", "Collapsed Stadium", "PokéStop", "PokeStop", "Jamming Tower", "Town Store",
        "Mesagoza", "Temple of Sinnoh", "Calamitous Snowy Mountain", "Calamitous Wasteland",
        "Magma Basin", "Lost City", "Path to the Peak", "Beach Court", "Neutral Center",
        "Grand Tree", "Dangerous Laser", "Festival Grounds", "Neutralizing Gas", "Lake Acuity",
        "Full Metal Lab", "Area Zero Underdepths", "Gravity Mountain"
    };

    private static readonly HashSet<string> KnownTools = new(StringComparer.OrdinalIgnoreCase)
    {
        "Forest Seal Stone", "Maximum Belt", "Heavy Baton", "Bravery Charm", "Hero's Cape",
        "Defiance Vest", "Technical Machine: Evolution", "Technical Machine: Devolution",
        "Technical Machine: Blindside", "Technical Machine: Crisis Punch", "Technical Machine: Turbo",
        "Rescue Board", "Emergency Board", "Rigid Band", "Air Balloon", "Choice Belt", "Exp. Share",
        "Vitality Band", "Justified Gloves", "Survival Brace", "Dangerous Laser Tool", "Powerglass",
        "Defiance Band", "Supereffective Glasses"
    };

    private static readonly HashSet<string> KnownCompetitiveTechs = new(StringComparer.OrdinalIgnoreCase)
    {
        "Iron Leaves ex", "Canceling Cologne", "Prime Catcher", "Lost Vacuum", "Counter Catcher",
        "Forest Seal Stone", "Maximum Belt", "Hero's Cape", "Heavy Baton", "Fezandipiti ex",
        "Klefki", "Cornerstone Mask Ogerpon ex", "Wellspring Mask Ogerpon ex", "Hearthflame Mask Ogerpon ex",
        "Mimikyu", "Spiritomb", "Flutter Mane", "Bloodmoon Ursaluna ex", "Lumineon V", "Rotom V",
        "Radiant Charizard", "Radiant Greninja", "Radiant Tsareena", "Dusknoir", "Dusclops",
        "Budew", "Fan Rotom", "Pal Pad", "Thorton", "Eri", "Xerosic's Machinations", "Mew ex",
        "Cleffa", "Neutral Center", "Grand Tree", "Technical Machine: Devolution", "Rabsca",
        "Unfair Stamp", "Secret Box", "Hyper Aroma", "Night Stretcher", "Briar"
    };

    public static PtcglParseResult Parse(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
        {
            return new PtcglParseResult(
                Success: false,
                ErrorMessage: "O texto da lista está vazio.",
                TotalCards: 0,
                PokemonCount: 0,
                TrainerCount: 0,
                EnergyCount: 0,
                Cards: new List<CardDto>(),
                SuggestedTechCards: new List<string>()
            );
        }

        var lines = rawText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        CardType currentSection = CardType.Pokemon;
        var cards = new List<CardDto>();
        var suggestedTechs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            // Check Section Headers
            if (line.StartsWith("Pokémon:", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("Pokemon:", StringComparison.OrdinalIgnoreCase))
            {
                currentSection = CardType.Pokemon;
                continue;
            }
            if (line.StartsWith("Trainer:", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("Treinador:", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("Treinadores:", StringComparison.OrdinalIgnoreCase))
            {
                currentSection = CardType.Trainer;
                continue;
            }
            if (line.StartsWith("Energy:", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("Energia:", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("Energias:", StringComparison.OrdinalIgnoreCase))
            {
                currentSection = CardType.Energy;
                continue;
            }
            if (line.StartsWith("Total Cards:", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("Total de cartas:", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 4 && int.TryParse(parts[0], out var qty))
            {
                int lastIdx = parts.Length - 1;
                var possibleTag = parts[lastIdx];
                if (parts.Length >= 5 && (possibleTag.Equals("PH", StringComparison.OrdinalIgnoreCase) ||
                                          possibleTag.Equals("SR", StringComparison.OrdinalIgnoreCase) ||
                                          possibleTag.Equals("UR", StringComparison.OrdinalIgnoreCase) ||
                                          possibleTag.Equals("IR", StringComparison.OrdinalIgnoreCase) ||
                                          possibleTag.Equals("SIR", StringComparison.OrdinalIgnoreCase)))
                {
                    lastIdx--;
                }

                var collectorNumber = parts[lastIdx];
                var setCode = parts[lastIdx - 1];
                int nameCount = lastIdx - 2;
                var cardName = nameCount > 0
                    ? string.Join(" ", parts.Skip(1).Take(nameCount)).Trim()
                    : parts[1];

                TrainerSubType? subType = null;
                if (currentSection == CardType.Trainer)
                {
                    subType = ClassifyTrainer(cardName);
                }

                bool isTech = IsCandidateTechCard(cardName, qty);
                if (isTech)
                {
                    suggestedTechs.Add(cardName);
                }

                cards.Add(new CardDto(
                    Name: cardName,
                    Quantity: qty,
                    SetCode: setCode,
                    CollectorNumber: collectorNumber,
                    CardType: currentSection,
                    TrainerSubType: subType,
                    IsTechCard: isTech
                ));
            }
            else if (parts.Length >= 2 && int.TryParse(parts[0], out var fallbackQty))
            {
                var name = string.Join(" ", parts.Skip(1)).Trim();
                TrainerSubType? subType = currentSection == CardType.Trainer ? ClassifyTrainer(name) : null;
                bool isTech = IsCandidateTechCard(name, fallbackQty);
                if (isTech) suggestedTechs.Add(name);

                cards.Add(new CardDto(
                    Name: name,
                    Quantity: fallbackQty,
                    SetCode: "",
                    CollectorNumber: "",
                    CardType: currentSection,
                    TrainerSubType: subType,
                    IsTechCard: isTech
                ));
            }
        }

        var pokemonCount = cards.Where(c => c.CardType == CardType.Pokemon).Sum(c => c.Quantity);
        var trainerCount = cards.Where(c => c.CardType == CardType.Trainer).Sum(c => c.Quantity);
        var energyCount = cards.Where(c => c.CardType == CardType.Energy).Sum(c => c.Quantity);
        var total = pokemonCount + trainerCount + energyCount;

        string? warningMessage = null;
        if (total != 60)
        {
            warningMessage = $"Aviso: A lista contém {total} cartas (o padrão de torneio é exatamente 60).";
        }

        return new PtcglParseResult(
            Success: cards.Count > 0,
            ErrorMessage: warningMessage,
            TotalCards: total,
            PokemonCount: pokemonCount,
            TrainerCount: trainerCount,
            EnergyCount: energyCount,
            Cards: cards,
            SuggestedTechCards: suggestedTechs.ToList()
        );
    }

    private static TrainerSubType ClassifyTrainer(string cardName)
    {
        if (KnownSupporters.Any(s => cardName.Contains(s, StringComparison.OrdinalIgnoreCase)))
            return TrainerSubType.Supporter;

        if (KnownStadiums.Any(s => cardName.Contains(s, StringComparison.OrdinalIgnoreCase)))
            return TrainerSubType.Stadium;

        if (KnownTools.Any(t => cardName.Contains(t, StringComparison.OrdinalIgnoreCase)))
            return TrainerSubType.Tool;

        return TrainerSubType.Item;
    }

    private static bool IsCandidateTechCard(string cardName, int quantity)
    {
        if (KnownCompetitiveTechs.Any(k => cardName.Contains(k, StringComparison.OrdinalIgnoreCase)))
            return true;

        // In competitive Pokémon TCG, 1-of cards are often tech cards
        if (quantity == 1 && !cardName.StartsWith("Basic", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }
}
