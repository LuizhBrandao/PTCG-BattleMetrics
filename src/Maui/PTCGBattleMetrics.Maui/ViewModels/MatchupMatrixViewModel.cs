using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PTCGBattleMetrics.Application.DTOs;
using PTCGBattleMetrics.Domain.Enums;
using PTCGBattleMetrics.Maui.Messages;
using PTCGBattleMetrics.Maui.Services.Data;
using PTCGBattleMetrics.Maui.ViewModels.Base;

namespace PTCGBattleMetrics.Maui.ViewModels;

public partial class MatchupMatrixViewModel : ViewModelBase
{
    private readonly IMauiBattleMetricsService _dataService;

    [ObservableProperty]
    private OverviewMetricsDto? _overview;

    [ObservableProperty]
    private ObservableCollection<MatchupMatrixItemDto> _matchups = new();

    [ObservableProperty]
    private InitiativeMetricsDto? _initiative;

    [ObservableProperty]
    private AdvancedTelemetryDto? _telemetry;

    [ObservableProperty]
    private ObservableCollection<DeckResponse> _decks = new();

    [ObservableProperty]
    private DeckResponse? _selectedDeckFilter;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsOverviewTab), nameof(IsMatrixTab), nameof(IsInitiativeTab), nameof(IsTelemetryTab))]
    private string _activeTab = "Overview"; // Overview, Matrix, Initiative, Telemetry

    public bool IsOverviewTab => ActiveTab == "Overview";
    public bool IsMatrixTab => ActiveTab == "Matrix";
    public bool IsInitiativeTab => ActiveTab == "Initiative";
    public bool IsTelemetryTab => ActiveTab == "Telemetry";

    public MatchupMatrixViewModel(IMauiBattleMetricsService dataService)
    {
        _dataService = dataService;
        Title = "📊 Métricas";

        // Listen for new matches to refresh metrics dynamically
        WeakReferenceMessenger.Default.Register<MatchLoggedMessage>(this, async (r, m) =>
        {
            await LoadDataAsync();
        });
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        IsBusy = true;
        var deckList = await _dataService.GetDecksAsync();
        Decks = new ObservableCollection<DeckResponse>(deckList);
        await LoadDataAsync();
        IsBusy = false;
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        Guid? deckId = SelectedDeckFilter?.Id;
        Overview = await _dataService.GetOverviewMetricsAsync(deckId);

        var list = await _dataService.GetMatchupMatrixAsync(deckId);
        Matchups = new ObservableCollection<MatchupMatrixItemDto>(list);

        Initiative = await _dataService.GetInitiativeMetricsAsync(deckId);
        Telemetry = await _dataService.GetAdvancedTelemetryAsync(deckId);
    }

    partial void OnSelectedDeckFilterChanged(DeckResponse? value)
    {
        _ = LoadDataAsync();
    }

    [RelayCommand]
    private void SetActiveTab(string tab)
    {
        ActiveTab = tab;
    }
}
