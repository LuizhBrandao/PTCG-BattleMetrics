using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PTCGBattleMetrics.Application.DTOs;
using PTCGBattleMetrics.Maui.Messages;
using PTCGBattleMetrics.Maui.Services.Data;
using PTCGBattleMetrics.Maui.ViewModels.Base;

namespace PTCGBattleMetrics.Maui.ViewModels;

public partial class HistoryViewModel : ViewModelBase
{
    private readonly IMauiBattleMetricsService _dataService;
    private List<MatchResponse> _allMatches = new();

    [ObservableProperty]
    private ObservableCollection<MatchResponse> _matches = new();

    [ObservableProperty]
    private string _searchTerm = string.Empty;

    public HistoryViewModel(IMauiBattleMetricsService dataService)
    {
        _dataService = dataService;
        Title = "📜 Histórico";

        WeakReferenceMessenger.Default.Register<MatchLoggedMessage>(this, async (r, m) =>
        {
            await LoadMatchesAsync();
        });
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        IsBusy = true;
        await LoadMatchesAsync();
        IsBusy = false;
    }

    [RelayCommand]
    public async Task LoadMatchesAsync()
    {
        _allMatches = await _dataService.GetMatchesAsync();
        ApplyFilter();
    }

    partial void OnSearchTermChanged(string value)
    {
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var filtered = string.IsNullOrWhiteSpace(SearchTerm)
            ? _allMatches
            : _allMatches.Where(m =>
                m.OpponentArchetype.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                (m.DeckName != null && m.DeckName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                (m.OpponentName != null && m.OpponentName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))).ToList();

        Matches = new ObservableCollection<MatchResponse>(filtered);
    }
}
