using PTCGBattleMetrics.Application.Services;
using PTCGBattleMetrics.Domain.Entities;
using Xunit;

namespace PTCGBattleMetrics.Tests;

public class ArchetypeOrderingTests
{
    [Fact]
    public void GetLimitlessTop10_ReturnsExactly10ArchetypesInExpectedOrder()
    {
        var top10 = ArchetypeHelper.GetLimitlessTop10();

        Assert.Equal(10, top10.Count);
        Assert.Equal("Dragapult", top10[0].Name);
        Assert.Equal("Mega Excadrill", top10[1].Name);
        Assert.Equal("Alakazam", top10[2].Name);
        Assert.Equal("Slowking", top10[3].Name);
        Assert.Equal("N's Zoroark", top10[4].Name);
        Assert.Equal("Festival Lead", top10[5].Name);
        Assert.Equal("Dhelmise", top10[6].Name);
        Assert.Equal("Marnie's Grimmsnarl", top10[7].Name);
        Assert.Equal("Mega Lucario", top10[8].Name);
        Assert.Equal("Toucannon", top10[9].Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetOrderedArchetypes_WhenNoLastFaced_ReturnsBaseList(string? lastFaced)
    {
        var baseList = ArchetypeHelper.GetLimitlessTop10();
        var ordered = ArchetypeHelper.GetOrderedArchetypes(baseList, lastFaced);

        Assert.Equal(10, ordered.Count);
        Assert.Equal("Dragapult", ordered[0].Name);
    }

    [Fact]
    public void GetOrderedArchetypes_WhenLastFacedIsInTop10_PutsItAtFirstPositionWithoutDuplicates()
    {
        var baseList = ArchetypeHelper.GetLimitlessTop10();
        var ordered = ArchetypeHelper.GetOrderedArchetypes(baseList, "Slowking");

        Assert.Equal(10, ordered.Count);
        Assert.Equal("Slowking", ordered[0].Name);
        Assert.Equal("Dragapult", ordered[1].Name);
        Assert.Equal(1, ordered.Count(a => a.Name == "Slowking"));
    }

    [Fact]
    public void GetOrderedArchetypes_WhenLastFacedIsCaseInsensitive_MatchesCorrectly()
    {
        var baseList = ArchetypeHelper.GetLimitlessTop10();
        var ordered = ArchetypeHelper.GetOrderedArchetypes(baseList, "slowking");

        Assert.Equal(10, ordered.Count);
        Assert.Equal("Slowking", ordered[0].Name);
        Assert.Equal(1, ordered.Count(a => a.Name == "Slowking"));
    }

    [Fact]
    public void GetOrderedArchetypes_WhenLastFacedIsCustom_PutsItAtFirstPositionWithTop10Following()
    {
        var baseList = ArchetypeHelper.GetLimitlessTop10();
        var ordered = ArchetypeHelper.GetOrderedArchetypes(baseList, "Ceruledge ex");

        Assert.Equal(11, ordered.Count);
        Assert.Equal("Ceruledge ex", ordered[0].Name);
        Assert.Equal("Dragapult", ordered[1].Name);
        Assert.Equal("Mega Excadrill", ordered[2].Name);
    }
}
