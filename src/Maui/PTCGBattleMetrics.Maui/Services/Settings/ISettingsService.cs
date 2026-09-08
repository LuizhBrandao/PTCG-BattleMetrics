namespace PTCGBattleMetrics.Maui.Services.Settings;

public interface ISettingsService
{
    string ApiEndpointBase { get; set; }
    Guid? ActiveDeckId { get; set; }
    Guid? ActiveTournamentId { get; set; }
    bool OfflineMode { get; set; }
}
