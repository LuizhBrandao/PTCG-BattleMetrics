using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PTCGBattleMetrics.Maui.Services.Data;
using PTCGBattleMetrics.Maui.Services.Settings;
using PTCGBattleMetrics.Maui.ViewModels.Base;

namespace PTCGBattleMetrics.Maui.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsService _settings;
    private readonly IMauiBattleMetricsService _dataService;

    [ObservableProperty]
    private string _apiEndpoint = string.Empty;

    [ObservableProperty]
    private bool _offlineMode;

    [ObservableProperty]
    private int _pendingSyncCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasConnectionStatus))]
    private string? _connectionStatus;

    public bool HasConnectionStatus => !string.IsNullOrEmpty(ConnectionStatus);

    [ObservableProperty]
    private bool _isConnectionSuccess;

    public SettingsViewModel(ISettingsService settings, IMauiBattleMetricsService dataService)
    {
        _settings = settings;
        _dataService = dataService;
        Title = "⚙️ Configurações";
    }

    [RelayCommand]
    public void Initialize()
    {
        ApiEndpoint = _settings.ApiEndpointBase;
        OfflineMode = _settings.OfflineMode;
        PendingSyncCount = _dataService.PendingSyncCount;
    }

    [RelayCommand]
    public async Task SaveSettingsAsync()
    {
        if (!string.IsNullOrWhiteSpace(ApiEndpoint))
        {
            _settings.ApiEndpointBase = ApiEndpoint.Trim();
        }
        _settings.OfflineMode = OfflineMode;

        await TestConnectionAsync();
    }

    [RelayCommand]
    public async Task TestConnectionAsync()
    {
        IsBusy = true;
        ConnectionStatus = "Testando conexão...";

        try
        {
            var archetypes = await _dataService.GetArchetypesAsync();
            if (_dataService.IsOnline)
            {
                IsConnectionSuccess = true;
                ConnectionStatus = $"✓ Conectado com sucesso ao servidor ({archetypes.Count} arquétipos)";
            }
            else
            {
                IsConnectionSuccess = false;
                ConnectionStatus = "⚠️ Não foi possível conectar ao servidor (usando dados locais)";
            }
        }
        catch (Exception ex)
        {
            IsConnectionSuccess = false;
            ConnectionStatus = $"Erro ao conectar: {ex.Message}";
        }

        PendingSyncCount = _dataService.PendingSyncCount;
        IsBusy = false;
    }

    [RelayCommand]
    public async Task SyncPendingMatchesAsync()
    {
        IsBusy = true;
        var count = await _dataService.SyncPendingMatchesAsync();
        PendingSyncCount = _dataService.PendingSyncCount;
        ConnectionStatus = count > 0
            ? $"✓ {count} partidas sincronizadas com o backend!"
            : "Nenhuma partida pendente de sincronização.";
        IsBusy = false;
    }
}
