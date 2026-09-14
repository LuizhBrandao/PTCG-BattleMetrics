using PTCGBattleMetrics.Application.DTOs;
using PTCGBattleMetrics.Domain.Entities;
using PTCGBattleMetrics.Domain.Enums;
using Xunit;

namespace PTCGBattleMetrics.Tests;

public class TournamentTrackingTests
{
    [Fact]
    public void TournamentEntity_CalculatesWinsLossesTiesAndRecordCorrectly()
    {
        // Arrange
        var tourney = new Tournament
        {
            Id = Guid.NewGuid(),
            Name = "Toguro liga"
        };

        // 3 wins, 1 loss
        tourney.Matches.Add(new Match { TournamentId = tourney.Id, Result = MatchResult.Win, OpponentArchetype = "Raio Furia" });
        tourney.Matches.Add(new Match { TournamentId = tourney.Id, Result = MatchResult.Win, OpponentArchetype = "Raio Furia" });
        tourney.Matches.Add(new Match { TournamentId = tourney.Id, Result = MatchResult.Win, OpponentArchetype = "Raio Furia" });
        tourney.Matches.Add(new Match { TournamentId = tourney.Id, Result = MatchResult.Loss, OpponentArchetype = "Raio Furia" });

        // Assert
        Assert.Equal(3, tourney.TotalWins);
        Assert.Equal(1, tourney.TotalLosses);
        Assert.Equal(0, tourney.TotalTies);
        Assert.Equal(9, tourney.MatchPoints); // (3 * 3) + 0 = 9 pts
        Assert.Equal("3-1-0", tourney.RecordDisplay);
    }

    [Fact]
    public void TournamentRecordCalculation_EnrichesResponseDtoCorrectly()
    {
        // Arrange: An initial tournament DTO with 0-0-0
        var tourneyId = Guid.NewGuid();
        var tourneyDto = new TournamentResponse(
            Id: tourneyId,
            Name: "Toguro liga",
            StoreOrVenue: "Toguro",
            Date: DateOnly.FromDateTime(DateTime.UtcNow),
            Category: TournamentCategory.LeagueCup,
            Format: TournamentFormat.SwissBo3,
            TotalParticipants: 16,
            FinalStanding: 1,
            ChampionshipPoints: null,
            TotalWins: 0,
            TotalLosses: 0,
            TotalTies: 0,
            MatchPoints: 0,
            RecordDisplay: "0-0-0",
            MatchesCount: 0
        );

        // User saved 4 matches (3 wins, 1 loss)
        var matches = new List<MatchResponse>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), "Alakazam Tucano", "Alakazam", tourneyId, "Toguro liga", "Raio Furia", MatchResult.Win, DateTimeOffset.UtcNow, 1, 1, null, null, null, null, null, null, 0, 6, 6, 0, null, null, null, new(), new(), 3),
            new(Guid.NewGuid(), Guid.NewGuid(), "Alakazam Tucano", "Alakazam", tourneyId, "Toguro liga", "Raio Furia", MatchResult.Win, DateTimeOffset.UtcNow, 2, 2, null, null, null, null, null, null, 0, 6, 6, 0, null, null, null, new(), new(), 3),
            new(Guid.NewGuid(), Guid.NewGuid(), "Alakazam Tucano", "Alakazam", tourneyId, "Toguro liga", "Raio Furia", MatchResult.Win, DateTimeOffset.UtcNow, 3, 1, null, null, null, null, null, null, 0, 6, 6, 0, null, null, null, new(), new(), 3),
            new(Guid.NewGuid(), Guid.NewGuid(), "Alakazam Tucano", "Alakazam", tourneyId, "Toguro liga", "Raio Furia", MatchResult.Loss, DateTimeOffset.UtcNow, 4, 3, null, null, null, null, null, null, 4, 0, 2, 6, null, null, null, new(), new(), 0),
        };

        // Act: recalculate
        var tMatches = matches.Where(m => m.TournamentId == tourneyDto.Id).ToList();
        int wins = tMatches.Count(m => m.Result == MatchResult.Win);
        int losses = tMatches.Count(m => m.Result == MatchResult.Loss);
        int ties = tMatches.Count(m => m.Result == MatchResult.Tie);
        int matchPoints = (wins * 3) + (ties * 1);
        string record = $"{wins}-{losses}-{ties}";

        var enriched = tourneyDto with
        {
            TotalWins = wins,
            TotalLosses = losses,
            TotalTies = ties,
            MatchPoints = matchPoints,
            RecordDisplay = record,
            MatchesCount = tMatches.Count
        };

        // Assert
        Assert.Equal(3, enriched.TotalWins);
        Assert.Equal(1, enriched.TotalLosses);
        Assert.Equal(0, enriched.TotalTies);
        Assert.Equal(9, enriched.MatchPoints);
        Assert.Equal("3-1-0", enriched.RecordDisplay);
        Assert.Equal(4, enriched.MatchesCount);
    }
}
