using PTCGBattleMetrics.Application.DTOs;
using PTCGBattleMetrics.Application.Services;
using PTCGBattleMetrics.Domain.Entities;
using PTCGBattleMetrics.Domain.Enums;
using Xunit;

namespace PTCGBattleMetrics.Tests;

public class MatchCrudTests
{
    [Fact]
    public void UpdateMatch_CorrectlyInvertsResultAndRecalculatesPoints()
    {
        // Arrange
        var tourney = new Tournament { Id = Guid.NewGuid(), Name = "Regional Cup" };
        var deck = new Deck { Id = Guid.NewGuid(), Name = "Charizard ex" };
        var match = new Match
        {
            Id = Guid.NewGuid(),
            DeckId = deck.Id,
            TournamentId = tourney.Id,
            OpponentArchetype = "Lugia VSTAR",
            Result = MatchResult.Loss,
            RoundNumber = 1
        };
        tourney.Matches.Add(match);

        Assert.Equal(0, tourney.MatchPoints);
        Assert.Equal("0-1-0", tourney.RecordDisplay);

        // Act: Edit the match to Win
        match.Result = MatchResult.Win;
        match.RoundNumber = 2;
        match.OpponentArchetype = "Gardevoir ex";

        // Assert
        Assert.Equal(MatchResult.Win, match.Result);
        Assert.Equal(2, match.RoundNumber);
        Assert.Equal("Gardevoir ex", match.OpponentArchetype);
        Assert.Equal(3, tourney.MatchPoints);
        Assert.Equal("1-0-0", tourney.RecordDisplay);
    }

    [Fact]
    public void DeleteMatch_RemovesFromMetricsAndRecalculatesWinRate()
    {
        // Arrange
        var metricsService = new MetricsService();
        var matches = new List<Match>
        {
            new() { Id = Guid.NewGuid(), OpponentArchetype = "Lugia VSTAR", Result = MatchResult.Win },
            new() { Id = Guid.NewGuid(), OpponentArchetype = "Charizard ex", Result = MatchResult.Loss },
            new() { Id = Guid.NewGuid(), OpponentArchetype = "Miradon ex", Result = MatchResult.Loss }
        };

        var initialOverview = metricsService.CalculateOverview(matches);
        Assert.Equal(3, initialOverview.TotalMatches);
        Assert.Equal(33.3, initialOverview.OverallWinRate);

        // Act: Delete the incorrect loss match
        var matchToDelete = matches.Last();
        matches.Remove(matchToDelete);

        var updatedOverview = metricsService.CalculateOverview(matches);

        // Assert
        Assert.Equal(2, updatedOverview.TotalMatches);
        Assert.Equal(1, updatedOverview.TotalWins);
        Assert.Equal(1, updatedOverview.TotalLosses);
        Assert.Equal(50.0, updatedOverview.OverallWinRate);
    }
}
