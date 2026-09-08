using PTCGBattleMetrics.Application.DTOs;
using PTCGBattleMetrics.Domain.Entities;
using PTCGBattleMetrics.Domain.Enums;

namespace PTCGBattleMetrics.Application.Services;

public class MetricsService : IMetricsService
{
    public OverviewMetricsDto CalculateOverview(IEnumerable<Match> matches)
    {
        var list = matches.ToList();
        var total = list.Count;
        if (total == 0)
        {
            return new OverviewMetricsDto(
                TotalMatches: 0,
                TotalWins: 0,
                TotalLosses: 0,
                TotalTies: 0,
                OverallWinRate: 0.0,
                NonTieWinRate: 0.0,
                DecksPerformance: new List<DeckPerformanceDto>(),
                FormatsPerformance: new List<FormatPerformanceDto>()
            );
        }

        var wins = list.Count(m => m.Result == MatchResult.Win);
        var losses = list.Count(m => m.Result == MatchResult.Loss);
        var ties = list.Count(m => m.Result == MatchResult.Tie);

        var overallWr = Math.Round((double)wins / total * 100.0, 1);
        var nonTieTotal = wins + losses;
        var nonTieWr = nonTieTotal > 0 ? Math.Round((double)wins / nonTieTotal * 100.0, 1) : 0.0;

        // Group by Deck
        var deckGroups = list
            .GroupBy(m => new { m.DeckId, DeckName = m.Deck?.Name ?? "Deck Não Especificado", Archetype = m.Deck?.Archetype ?? "Geral" })
            .Select(g =>
            {
                var gTotal = g.Count();
                var gWins = g.Count(m => m.Result == MatchResult.Win);
                var gLosses = g.Count(m => m.Result == MatchResult.Loss);
                var gTies = g.Count(m => m.Result == MatchResult.Tie);
                var gWr = Math.Round((double)gWins / gTotal * 100.0, 1);
                return new DeckPerformanceDto(g.Key.DeckId, g.Key.DeckName, g.Key.Archetype, gTotal, gWins, gLosses, gTies, gWr);
            })
            .OrderByDescending(d => d.Matches)
            .ToList();

        // Group by Tournament Format / Category
        var formatGroups = list
            .Where(m => m.Tournament != null)
            .GroupBy(m => m.Tournament!.Format.ToString())
            .Select(g =>
            {
                var fTotal = g.Count();
                var fWins = g.Count(m => m.Result == MatchResult.Win);
                var fLosses = g.Count(m => m.Result == MatchResult.Loss);
                var fTies = g.Count(m => m.Result == MatchResult.Tie);
                var fWr = Math.Round((double)fWins / fTotal * 100.0, 1);
                return new FormatPerformanceDto(g.Key, fTotal, fWins, fLosses, fTies, fWr);
            })
            .OrderByDescending(f => f.Matches)
            .ToList();

        return new OverviewMetricsDto(total, wins, losses, ties, overallWr, nonTieWr, deckGroups, formatGroups);
    }

    public InitiativeMetricsDto CalculateInitiative(IEnumerable<Match> matches)
    {
        var list = matches.ToList();

        var firstMatches = list.Where(m => m.TurnOrder == TurnOrder.First).ToList();
        var secondMatches = list.Where(m => m.TurnOrder == TurnOrder.Second).ToList();

        var firstTotal = firstMatches.Count;
        var firstWins = firstMatches.Count(m => m.Result == MatchResult.Win);
        var firstLosses = firstMatches.Count(m => m.Result == MatchResult.Loss);
        var firstTies = firstMatches.Count(m => m.Result == MatchResult.Tie);
        var firstWr = firstTotal > 0 ? Math.Round((double)firstWins / firstTotal * 100.0, 1) : 0.0;

        var secondTotal = secondMatches.Count;
        var secondWins = secondMatches.Count(m => m.Result == MatchResult.Win);
        var secondLosses = secondMatches.Count(m => m.Result == MatchResult.Loss);
        var secondTies = secondMatches.Count(m => m.Result == MatchResult.Tie);
        var secondWr = secondTotal > 0 ? Math.Round((double)secondWins / secondTotal * 100.0, 1) : 0.0;

        // Archetype specific initiative
        var archetypeList = list
            .Where(m => !string.IsNullOrWhiteSpace(m.OpponentArchetype))
            .GroupBy(m => m.OpponentArchetype.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(g =>
            {
                var fList = g.Where(m => m.TurnOrder == TurnOrder.First).ToList();
                var sList = g.Where(m => m.TurnOrder == TurnOrder.Second).ToList();

                var fCount = fList.Count;
                var fWrArchetype = fCount > 0 ? Math.Round((double)fList.Count(m => m.Result == MatchResult.Win) / fCount * 100.0, 1) : 0.0;

                var sCount = sList.Count;
                var sWrArchetype = sCount > 0 ? Math.Round((double)sList.Count(m => m.Result == MatchResult.Win) / sCount * 100.0, 1) : 0.0;

                return new ArchetypeInitiativeDto(g.Key, fCount, fWrArchetype, sCount, sWrArchetype);
            })
            .OrderByDescending(a => a.FirstMatches + a.SecondMatches)
            .ToList();

        return new InitiativeMetricsDto(
            firstTotal, firstWins, firstLosses, firstTies, firstWr,
            secondTotal, secondWins, secondLosses, secondTies, secondWr,
            archetypeList
        );
    }

    public List<MatchupMatrixItemDto> CalculateMatchupMatrix(IEnumerable<Match> matches)
    {
        var list = matches.ToList();

        return list
            .Where(m => !string.IsNullOrWhiteSpace(m.OpponentArchetype))
            .GroupBy(m => m.OpponentArchetype.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(g =>
            {
                var matchesCount = g.Count();
                var wins = g.Count(m => m.Result == MatchResult.Win);
                var losses = g.Count(m => m.Result == MatchResult.Loss);
                var ties = g.Count(m => m.Result == MatchResult.Tie);
                var winRate = Math.Round((double)wins / matchesCount * 100.0, 1);

                MatchupRating rating = winRate >= 55.0
                    ? MatchupRating.Favorable
                    : (winRate >= 45.0 ? MatchupRating.Neutral : MatchupRating.Unfavorable);

                // Average prizes
                var playerPrizesList = g.Select(m => m.PlayerPrizesTaken).Where(p => p.HasValue).Select(p => p!.Value).ToList();
                var avgPlayerPrizes = playerPrizesList.Any() ? Math.Round(playerPrizesList.Average(), 1) : 0.0;

                var oppPrizesList = g.Select(m => m.OpponentPrizesTaken).Where(p => p.HasValue).Select(p => p!.Value).ToList();
                var avgOppPrizes = oppPrizesList.Any() ? Math.Round(oppPrizesList.Average(), 1) : 0.0;

                // 1st vs 2nd
                var firstMatches = g.Where(m => m.TurnOrder == TurnOrder.First).ToList();
                var goingFirstWr = firstMatches.Any()
                    ? Math.Round((double)firstMatches.Count(m => m.Result == MatchResult.Win) / firstMatches.Count * 100.0, 1)
                    : 0.0;

                var secondMatches = g.Where(m => m.TurnOrder == TurnOrder.Second).ToList();
                var goingSecondWr = secondMatches.Any()
                    ? Math.Round((double)secondMatches.Count(m => m.Result == MatchResult.Win) / secondMatches.Count * 100.0, 1)
                    : 0.0;

                return new MatchupMatrixItemDto(
                    OpponentArchetype: g.Key,
                    Matches: matchesCount,
                    Wins: wins,
                    Losses: losses,
                    Ties: ties,
                    WinRate: winRate,
                    Rating: rating,
                    AvgPlayerPrizesTaken: avgPlayerPrizes,
                    AvgOpponentPrizesTaken: avgOppPrizes,
                    GoingFirstWinRate: goingFirstWr,
                    GoingSecondWinRate: goingSecondWr
                );
            })
            .OrderByDescending(m => m.Matches)
            .ThenByDescending(m => m.WinRate)
            .ToList();
    }

    public AdvancedTelemetryDto CalculateAdvancedTelemetry(IEnumerable<Match> matches, IEnumerable<Deck>? decks = null)
    {
        var list = matches.ToList();

        // 1. Coin Flip
        var wonFlipMatches = list.Where(m => m.CoinFlipWon == true).ToList();
        var lostFlipMatches = list.Where(m => m.CoinFlipWon == false).ToList();

        var wonFlipCount = wonFlipMatches.Count;
        var wonFlipWins = wonFlipMatches.Count(m => m.Result == MatchResult.Win);
        var wonFlipWr = wonFlipCount > 0 ? Math.Round((double)wonFlipWins / wonFlipCount * 100.0, 1) : 0.0;

        var lostFlipCount = lostFlipMatches.Count;
        var lostFlipWins = lostFlipMatches.Count(m => m.Result == MatchResult.Win);
        var lostFlipWr = lostFlipCount > 0 ? Math.Round((double)lostFlipWins / lostFlipCount * 100.0, 1) : 0.0;

        var coinFlipDelta = Math.Round(wonFlipWr - lostFlipWr, 1);
        var coinFlipDto = new CoinFlipMetricsDto(wonFlipCount, wonFlipWins, wonFlipWr, lostFlipCount, lostFlipWins, lostFlipWr, coinFlipDelta);

        // 2. Prize Efficiency in Losses
        var lossMatches = list.Where(m => m.Result == MatchResult.Loss).ToList();
        var lossesWithPrizes = lossMatches
            .Select(m => m.PlayerPrizesTaken)
            .Where(p => p.HasValue)
            .Select(p => p!.Value)
            .ToList();

        var avgPrizesInLosses = lossesWithPrizes.Any() ? Math.Round(lossesWithPrizes.Average(), 2) : 0.0;
        var closeLosses = lossesWithPrizes.Count(p => p >= 4);      // 4 or 5 prizes
        var moderateLosses = lossesWithPrizes.Count(p => p is 2 or 3);
        var blowoutLosses = lossesWithPrizes.Count(p => p <= 1);     // 0 or 1 prize

        var prizeLossDto = new PrizeLossMetricsDto(
            AvgPrizesTakenInLosses: avgPrizesInLosses,
            TotalLossesEvaluated: lossesWithPrizes.Count,
            CloseLossesCount: closeLosses,
            ModerateLossesCount: moderateLosses,
            BlowoutLossesCount: blowoutLosses
        );

        // 3. Mulligan Correlation
        var matchesWithMulligans = list.Where(m => m.PlayerMulligans.HasValue).ToList();
        var avgMulligans = matchesWithMulligans.Any()
            ? Math.Round(matchesWithMulligans.Average(m => m.PlayerMulligans!.Value), 2)
            : 0.0;

        var zeroMulliganList = matchesWithMulligans.Where(m => m.PlayerMulligans == 0).ToList();
        var zeroWr = zeroMulliganList.Any()
            ? Math.Round((double)zeroMulliganList.Count(m => m.Result == MatchResult.Win) / zeroMulliganList.Count * 100.0, 1)
            : 0.0;

        var oneMulliganList = matchesWithMulligans.Where(m => m.PlayerMulligans == 1).ToList();
        var oneWr = oneMulliganList.Any()
            ? Math.Round((double)oneMulliganList.Count(m => m.Result == MatchResult.Win) / oneMulliganList.Count * 100.0, 1)
            : 0.0;

        var twoPlusMulliganList = matchesWithMulligans.Where(m => m.PlayerMulligans >= 2).ToList();
        var twoPlusWr = twoPlusMulliganList.Any()
            ? Math.Round((double)twoPlusMulliganList.Count(m => m.Result == MatchResult.Win) / twoPlusMulliganList.Count * 100.0, 1)
            : 0.0;

        var mulliganDto = new MulliganCorrelationDto(
            AvgPlayerMulligans: avgMulligans,
            ZeroMulliganMatches: zeroMulliganList.Count,
            ZeroMulliganWinRate: zeroWr,
            OneMulliganMatches: oneMulliganList.Count,
            OneMulliganWinRate: oneWr,
            TwoOrMoreMulligansMatches: twoPlusMulliganList.Count,
            TwoOrMoreMulligansWinRate: twoPlusWr
        );

        // 4. Tech Cards Impact
        // Collect all distinct tech cards from matches or deck tech cards
        var allTechCardNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var m in list)
        {
            foreach (var t in m.TechCardsUsed)
            {
                if (!string.IsNullOrWhiteSpace(t)) allTechCardNames.Add(t.Trim());
            }
        }
        if (decks != null)
        {
            foreach (var d in decks)
            {
                foreach (var t in d.TechCards)
                {
                    if (!string.IsNullOrWhiteSpace(t)) allTechCardNames.Add(t.Trim());
                }
            }
        }

        var techCardImpacts = new List<TechCardImpactDto>();
        foreach (var tech in allTechCardNames)
        {
            var withTechMatches = list.Where(m =>
                m.TechCardsUsed.Contains(tech, StringComparer.OrdinalIgnoreCase) ||
                (m.Deck != null && m.Deck.TechCards.Contains(tech, StringComparer.OrdinalIgnoreCase))
            ).ToList();

            var withoutTechMatches = list.Except(withTechMatches).ToList();

            var withCount = withTechMatches.Count;
            var withWins = withTechMatches.Count(m => m.Result == MatchResult.Win);
            var withWr = withCount > 0 ? Math.Round((double)withWins / withCount * 100.0, 1) : 0.0;

            var withoutCount = withoutTechMatches.Count;
            var withoutWins = withoutTechMatches.Count(m => m.Result == MatchResult.Win);
            var withoutWr = withoutCount > 0 ? Math.Round((double)withoutWins / withoutCount * 100.0, 1) : 0.0;

            var delta = Math.Round(withWr - withoutWr, 1);

            techCardImpacts.Add(new TechCardImpactDto(
                TechCardName: tech,
                MatchesWithCard: withCount,
                WinRateWithCard: withWr,
                MatchesWithoutCard: withoutCount,
                WinRateWithoutCard: withoutWr,
                ImpactDelta: delta
            ));
        }
        techCardImpacts = techCardImpacts.OrderByDescending(t => t.MatchesWithCard).ToList();

        // 5. Win Conditions Breakdown
        var totalWithCondition = list.Count(m => m.WinCondition.HasValue);
        var winConditionBreakdowns = Enum.GetValues<WinCondition>()
            .Select(cond =>
            {
                var count = list.Count(m => m.WinCondition == cond);
                var pct = totalWithCondition > 0 ? Math.Round((double)count / totalWithCondition * 100.0, 1) : 0.0;
                var displayName = cond switch
                {
                    WinCondition.PrizeKnockout => "Nocaute / 6 Prêmios",
                    WinCondition.Concede => "Concede (Rendição)",
                    WinCondition.DeckOut => "Deck Out",
                    WinCondition.TimeoutSuddenDeath => "Timeout / Morte Súbita",
                    WinCondition.PenaltyDQ => "Penalidade / DQ",
                    _ => cond.ToString()
                };
                return new WinConditionBreakdownDto(cond, displayName, count, pct);
            })
            .Where(b => b.Count > 0)
            .OrderByDescending(b => b.Count)
            .ToList();

        return new AdvancedTelemetryDto(
            CoinFlip: coinFlipDto,
            PrizeLosses: prizeLossDto,
            Mulligans: mulliganDto,
            TechCardImpacts: techCardImpacts,
            WinConditions: winConditionBreakdowns
        );
    }
}
