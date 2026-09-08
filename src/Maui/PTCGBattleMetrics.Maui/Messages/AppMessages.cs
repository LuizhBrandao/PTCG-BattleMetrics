using CommunityToolkit.Mvvm.Messaging.Messages;
using PTCGBattleMetrics.Application.DTOs;

namespace PTCGBattleMetrics.Maui.Messages;

public class MatchLoggedMessage : ValueChangedMessage<MatchResponse>
{
    public MatchLoggedMessage(MatchResponse value) : base(value)
    {
    }
}

public class ActiveDeckChangedMessage : ValueChangedMessage<Guid?>
{
    public ActiveDeckChangedMessage(Guid? value) : base(value)
    {
    }
}

public class ActiveTournamentChangedMessage : ValueChangedMessage<Guid?>
{
    public ActiveTournamentChangedMessage(Guid? value) : base(value)
    {
    }
}
