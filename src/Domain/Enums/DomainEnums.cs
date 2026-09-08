namespace PTCGBattleMetrics.Domain.Enums;

public enum MatchResult
{
    Win = 1,
    Loss = 2,
    Tie = 3
}

public enum GameResult
{
    Win = 1,
    Loss = 2,
    Tie = 3
}

public enum TurnOrder
{
    First = 1,
    Second = 2
}

public enum WinCondition
{
    PrizeKnockout = 1,
    Concede = 2,
    DeckOut = 3,
    TimeoutSuddenDeath = 4,
    PenaltyDQ = 5
}

public enum TournamentCategory
{
    Casual = 1,
    LeagueChallenge = 2,
    LeagueCup = 3,
    Regional = 4,
    SpecialEvent = 5,
    International = 6
}

public enum TournamentFormat
{
    SwissBo1 = 1,
    SwissBo3 = 2,
    TopCut = 3
}

public enum CardType
{
    Pokemon = 1,
    Trainer = 2,
    Energy = 3
}

public enum TrainerSubType
{
    Item = 1,
    Supporter = 2,
    Tool = 3,
    Stadium = 4
}

public enum MatchupRating
{
    Favorable = 1,
    Neutral = 2,
    Unfavorable = 3
}
