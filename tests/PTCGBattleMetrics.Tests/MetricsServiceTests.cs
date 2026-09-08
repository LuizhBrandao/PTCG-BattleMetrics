using PTCGBattleMetrics.Application.Services;
using PTCGBattleMetrics.Domain.Entities;
using PTCGBattleMetrics.Domain.Enums;
using Xunit;

namespace PTCGBattleMetrics.Tests;

public class MetricsServiceTests
{
    private readonly MetricsService _service = new();

    [Fact]
    public void CalculateOverview_ComputesCorrectWinRatesAndGroupings()
    {
        // Arrange
        var deckId = Guid.NewGuid();
        var deck = new Deck { Id = deckId, Name = "Charizard ex", Archetype = "Charizard ex" };
        var matches = new List<Match>
        {
            new() { DeckId = deckId, Deck = deck, Result = MatchResult.Win, OpponentArchetype = "Lugia VSTAR" },
            new() { DeckId = deckId, Deck = deck, Result = MatchResult.Win, OpponentArchetype = "Gardevoir" },
            new() { DeckId = deckId, Deck = deck, Result = MatchResult.Loss, OpponentArchetype = "Gardevoir" },
            new() { DeckId = deckId, Deck = deck, Result = MatchResult.Tie, OpponentArchetype = "Raging Bolt" },
        };

        // Act
        var overview = _service.CalculateOverview(matches);

        // Assert
        Assert.Equal(4, overview.TotalMatches);
        Assert.Equal(2, overview.TotalWins);
        Assert.Equal(1, overview.TotalLosses);
        Assert.Equal(1, overview.TotalTies);
        Assert.Equal(50.0, overview.OverallWinRate); // 2/4 * 100 = 50.0%
        Assert.Equal(66.7, overview.NonTieWinRate);  // 2/3 * 100 = 66.7%
        Assert.Single(overview.DecksPerformance);
        Assert.Equal("Charizard ex", overview.DecksPerformance[0].DeckName);
    }

    [Fact]
    public void CalculateInitiative_ComputesFirstAndSecondWinRates()
    {
        // Arrange
        var matches = new List<Match>
        {
            new() { TurnOrder = TurnOrder.First, Result = MatchResult.Win, OpponentArchetype = "Lugia" },
            new() { TurnOrder = TurnOrder.First, Result = MatchResult.Win, OpponentArchetype = "Lugia" },
            new() { TurnOrder = TurnOrder.First, Result = MatchResult.Loss, OpponentArchetype = "Gardevoir" },
            new() { TurnOrder = TurnOrder.Second, Result = MatchResult.Loss, OpponentArchetype = "Lugia" },
            new() { TurnOrder = TurnOrder.Second, Result = MatchResult.Win, OpponentArchetype = "Gardevoir" },
        };

        // Act
        var initiative = _service.CalculateInitiative(matches);

        // Assert
        Assert.Equal(3, initiative.FirstTotal);
        Assert.Equal(2, initiative.FirstWins);
        Assert.Equal(66.7, initiative.FirstWinRate);

        Assert.Equal(2, initiative.SecondTotal);
        Assert.Equal(1, initiative.SecondWins);
        Assert.Equal(50.0, initiative.SecondWinRate);

        Assert.Equal(2, initiative.ArchetypeInitiatives.Count);
    }

    [Fact]
    public void CalculateMatchupMatrix_CategorizesRatingsCorrectly()
    {
        // Arrange
        var matches = new List<Match>
        {
            // Against Lugia: 3 Wins, 1 Loss -> 75% (Favorable)
            new() { OpponentArchetype = "Lugia VSTAR", Result = MatchResult.Win, PlayerPrizesRemaining = 0, OpponentPrizesRemaining = 4 },
            new() { OpponentArchetype = "Lugia VSTAR", Result = MatchResult.Win, PlayerPrizesRemaining = 0, OpponentPrizesRemaining = 2 },
            new() { OpponentArchetype = "Lugia VSTAR", Result = MatchResult.Win, PlayerPrizesRemaining = 0, OpponentPrizesRemaining = 1 },
            new() { OpponentArchetype = "Lugia VSTAR", Result = MatchResult.Loss, PlayerPrizesRemaining = 2, OpponentPrizesRemaining = 0 },

            // Against Gardevoir: 1 Win, 1 Loss -> 50% (Neutral)
            new() { OpponentArchetype = "Gardevoir", Result = MatchResult.Win, PlayerPrizesRemaining = 0, OpponentPrizesRemaining = 3 },
            new() { OpponentArchetype = "Gardevoir", Result = MatchResult.Loss, PlayerPrizesRemaining = 1, OpponentPrizesRemaining = 0 },

            // Against Dragapult: 0 Win, 2 Losses -> 0% (Unfavorable)
            new() { OpponentArchetype = "Dragapult ex", Result = MatchResult.Loss, PlayerPrizesRemaining = 4, OpponentPrizesRemaining = 0 },
            new() { OpponentArchetype = "Dragapult ex", Result = MatchResult.Loss, PlayerPrizesRemaining = 5, OpponentPrizesRemaining = 0 },
        };

        // Act
        var matrix = _service.CalculateMatchupMatrix(matches);

        // Assert
        Assert.Equal(3, matrix.Count);

        var lugia = matrix.First(m => m.OpponentArchetype == "Lugia VSTAR");
        Assert.Equal(MatchupRating.Favorable, lugia.Rating);
        Assert.Equal(75.0, lugia.WinRate);

        var garde = matrix.First(m => m.OpponentArchetype == "Gardevoir");
        Assert.Equal(MatchupRating.Neutral, garde.Rating);
        Assert.Equal(50.0, garde.WinRate);

        var dragapult = matrix.First(m => m.OpponentArchetype == "Dragapult ex");
        Assert.Equal(MatchupRating.Unfavorable, dragapult.Rating);
        Assert.Equal(0.0, dragapult.WinRate);
    }

    [Fact]
    public void CalculateAdvancedTelemetry_CalculatesCoinFlipPrizesMulligansAndTechImpact()
    {
        // Arrange
        var matches = new List<Match>
        {
            new()
            {
                Result = MatchResult.Win,
                CoinFlipWon = true,
                PlayerMulligans = 0,
                PlayerPrizesRemaining = 0,
                TechCardsUsed = new() { "Canceling Cologne" },
                WinCondition = WinCondition.PrizeKnockout
            },
            new()
            {
                Result = MatchResult.Win,
                CoinFlipWon = true,
                PlayerMulligans = 0,
                PlayerPrizesRemaining = 0,
                TechCardsUsed = new() { "Canceling Cologne", "Prime Catcher" },
                WinCondition = WinCondition.Concede
            },
            new()
            {
                Result = MatchResult.Loss,
                CoinFlipWon = false,
                PlayerMulligans = 2,
                PlayerPrizesRemaining = 2, // took 6 - 2 = 4 prizes (Close loss!)
                TechCardsUsed = new() { "Prime Catcher" },
                WinCondition = WinCondition.PrizeKnockout
            },
            new()
            {
                Result = MatchResult.Loss,
                CoinFlipWon = false,
                PlayerMulligans = 1,
                PlayerPrizesRemaining = 5, // took 6 - 5 = 1 prize (Blowout loss!)
                TechCardsUsed = new(),
                WinCondition = WinCondition.Concede
            }
        };

        // Act
        var telemetry = _service.CalculateAdvancedTelemetry(matches);

        // Assert Coin Flip
        Assert.Equal(2, telemetry.CoinFlip.WonCoinFlipCount);
        Assert.Equal(100.0, telemetry.CoinFlip.WonCoinFlipWinRate);
        Assert.Equal(0.0, telemetry.CoinFlip.LostCoinFlipWinRate);
        Assert.Equal(100.0, telemetry.CoinFlip.CoinFlipAdvantageDelta);

        // Assert Prize Losses (losses took 4 and 1 prizes -> avg (4+1)/2 = 2.5)
        Assert.Equal(2.5, telemetry.PrizeLosses.AvgPrizesTakenInLosses);
        Assert.Equal(1, telemetry.PrizeLosses.CloseLossesCount);
        Assert.Equal(1, telemetry.PrizeLosses.BlowoutLossesCount);

        // Assert Mulligans
        Assert.Equal(2, telemetry.Mulligans.ZeroMulliganMatches);
        Assert.Equal(100.0, telemetry.Mulligans.ZeroMulliganWinRate);
        Assert.Equal(1, telemetry.Mulligans.OneMulliganMatches);
        Assert.Equal(0.0, telemetry.Mulligans.OneMulliganWinRate);

        // Assert Tech Cards (Canceling Cologne was in 2 wins out of 2 matches = 100%)
        var cologne = telemetry.TechCardImpacts.FirstOrDefault(t => t.TechCardName == "Canceling Cologne");
        Assert.NotNull(cologne);
        Assert.Equal(100.0, cologne.WinRateWithCard);
        Assert.Equal(0.0, cologne.WinRateWithoutCard);
        Assert.Equal(100.0, cologne.ImpactDelta);

        // Assert Win Conditions
        Assert.Equal(2, telemetry.WinConditions.Count);
        Assert.Contains(telemetry.WinConditions, w => w.Condition == WinCondition.PrizeKnockout && w.Count == 2);
        Assert.Contains(telemetry.WinConditions, w => w.Condition == WinCondition.Concede && w.Count == 2);
    }
}
