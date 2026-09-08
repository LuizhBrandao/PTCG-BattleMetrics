namespace PTCGBattleMetrics.Maui.Services.Settings;

public sealed class SettingsService : ISettingsService
{
    private const string ApiEndpointKey = "api_endpoint_base";
    // Default to the local network IP of the PC so the Galaxy S23 connects out of the box!
    private const string DefaultApiEndpoint = "http://192.168.15.92:5016";

    private const string ActiveDeckIdKey = "active_deck_id";
    private const string ActiveTournamentIdKey = "active_tournament_id";
    private const string OfflineModeKey = "offline_mode";

    public string ApiEndpointBase
    {
        get => Preferences.Get(ApiEndpointKey, DefaultApiEndpoint);
        set => Preferences.Set(ApiEndpointKey, value);
    }

    public Guid? ActiveDeckId
    {
        get
        {
            var str = Preferences.Get(ActiveDeckIdKey, string.Empty);
            return Guid.TryParse(str, out var id) ? id : null;
        }
        set
        {
            if (value.HasValue)
                Preferences.Set(ActiveDeckIdKey, value.Value.ToString());
            else
                Preferences.Remove(ActiveDeckIdKey);
        }
    }

    public Guid? ActiveTournamentId
    {
        get
        {
            var str = Preferences.Get(ActiveTournamentIdKey, string.Empty);
            return Guid.TryParse(str, out var id) ? id : null;
        }
        set
        {
            if (value.HasValue)
                Preferences.Set(ActiveTournamentIdKey, value.Value.ToString());
            else
                Preferences.Remove(ActiveTournamentIdKey);
        }
    }

    public bool OfflineMode
    {
        get => Preferences.Get(OfflineModeKey, false);
        set => Preferences.Set(OfflineModeKey, value);
    }
}
