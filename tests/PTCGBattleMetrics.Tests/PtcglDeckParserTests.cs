using PTCGBattleMetrics.Application.Services;
using PTCGBattleMetrics.Domain.Enums;
using Xunit;

namespace PTCGBattleMetrics.Tests;

public class PtcglDeckParserTests
{
    private const string StandardCharizardDeck = """
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

    [Fact]
    public void Parse_StandardDeck_Returns60CardsAndCorrectBreakdowns()
    {
        // Act
        var result = PtcglDeckParser.Parse(StandardCharizardDeck);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorMessage);
        Assert.Equal(60, result.TotalCards);
        Assert.Equal(17, result.PokemonCount);
        Assert.Equal(36, result.TrainerCount);
        Assert.Equal(7, result.EnergyCount);
    }

    [Fact]
    public void Parse_TrainerSubtypes_CorrectlyClassifiesSupportersStadiumsToolsItems()
    {
        // Act
        var result = PtcglDeckParser.Parse(StandardCharizardDeck);

        // Assert
        var arven = result.Cards.FirstOrDefault(c => c.Name == "Arven");
        Assert.NotNull(arven);
        Assert.Equal(TrainerSubType.Supporter, arven.TrainerSubType);

        var artazon = result.Cards.FirstOrDefault(c => c.Name == "Artazon");
        Assert.NotNull(artazon);
        Assert.Equal(TrainerSubType.Stadium, artazon.TrainerSubType);

        var forestSeal = result.Cards.FirstOrDefault(c => c.Name == "Forest Seal Stone");
        Assert.NotNull(forestSeal);
        Assert.Equal(TrainerSubType.Tool, forestSeal.TrainerSubType);

        var ultraBall = result.Cards.FirstOrDefault(c => c.Name == "Ultra Ball");
        Assert.NotNull(ultraBall);
        Assert.Equal(TrainerSubType.Item, ultraBall.TrainerSubType);
    }

    [Fact]
    public void Parse_TechCards_IdentifiesCompetitiveTechs()
    {
        // Act
        var result = PtcglDeckParser.Parse(StandardCharizardDeck);

        // Assert
        Assert.Contains(result.SuggestedTechCards, t => t.Equals("Prime Catcher", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.SuggestedTechCards, t => t.Equals("Forest Seal Stone", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.SuggestedTechCards, t => t.Equals("Lost Vacuum", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.SuggestedTechCards, t => t.Equals("Counter Catcher", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.SuggestedTechCards, t => t.Equals("Fezandipiti ex", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Parse_PortugueseHeaders_ParsesSuccessfully()
    {
        var ptDeck = """
            Pokémon: 2
            4 Charmander MEW 4
            3 Charizard ex OBF 125

            Treinador: 2
            4 Arven OBF 186
            4 Ultra Ball SVI 196

            Energia: 1
            6 Basic {R} Energy SVE 2
            """;

        // Act
        var result = PtcglDeckParser.Parse(ptDeck);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(7, result.PokemonCount);
        Assert.Equal(8, result.TrainerCount);
        Assert.Equal(6, result.EnergyCount);
        Assert.Equal(21, result.TotalCards);
        Assert.Contains("Aviso: A lista contém 21 cartas", result.ErrorMessage);
    }

    [Fact]
    public void Parse_EmptyText_ReturnsFailure()
    {
        // Act
        var result = PtcglDeckParser.Parse("");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(0, result.TotalCards);
    }
}
