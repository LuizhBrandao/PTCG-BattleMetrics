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

public partial class HistoryViewModel : ViewModelBase
{
    private readonly IMauiBattleMetricsService _dataService;
    private List<MatchResponse> _allMatches = new();

    [ObservableProperty]
    private ObservableCollection<MatchResponse> _matches = new();

    [ObservableProperty]
    private string _searchTerm = string.Empty;

    // Edit Modal State
    [ObservableProperty]
    private bool _showEditModal;

    [ObservableProperty]
    private MatchResponse? _editingMatch;

    [ObservableProperty]
    private ObservableCollection<DeckResponse> _decks = new();

    [ObservableProperty]
    private DeckResponse? _selectedEditDeck;

    [ObservableProperty]
    private string _editOpponentArchetype = string.Empty;

    [ObservableProperty]
    private MatchResult _editResult = MatchResult.Win;

    [ObservableProperty]
    private int? _editRoundNumber;

    [ObservableProperty]
    private int? _editTableNumber;

    [ObservableProperty]
    private string? _editOpponentName;

    [ObservableProperty]
    private string? _editOpponentPopId;

    [ObservableProperty]
    private string? _editStartingActivePokemon;

    [ObservableProperty]
    private string? _editTacticalNotes;

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
                (m.TournamentName != null && m.TournamentName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                (m.OpponentName != null && m.OpponentName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))).ToList();

        Matches = new ObservableCollection<MatchResponse>(filtered);
    }

    [RelayCommand]
    public async Task DeleteMatchAsync(Guid id)
    {
        if (Microsoft.Maui.Controls.Application.Current?.Windows.Count > 0 && 
            Microsoft.Maui.Controls.Application.Current.Windows[0].Page != null)
        {
            bool answer = await Microsoft.Maui.Controls.Application.Current.Windows[0].Page!.DisplayAlert(
                "Excluir Partida",
                "Deseja realmente excluir esta partida do histórico?",
                "Sim, Excluir",
                "Cancelar"
            );
            if (!answer) return;
        }

        IsBusy = true;
        await _dataService.DeleteMatchAsync(id);
        await LoadMatchesAsync();
        IsBusy = false;
    }

    [RelayCommand]
    public async Task OpenEditModalAsync(MatchResponse match)
    {
        EditingMatch = match;
        var deckList = await _dataService.GetDecksAsync();
        Decks = new ObservableCollection<DeckResponse>(deckList);
        SelectedEditDeck = Decks.FirstOrDefault(d => d.Id == match.DeckId) ?? Decks.FirstOrDefault();
        EditOpponentArchetype = match.OpponentArchetype;
        EditResult = match.Result;
        EditRoundNumber = match.RoundNumber;
        EditTableNumber = match.TableNumber;
        EditOpponentName = match.OpponentName;
        EditOpponentPopId = match.OpponentPopId;
        EditStartingActivePokemon = match.StartingActivePokemon;
        EditTacticalNotes = match.TacticalNotes;
        ShowEditModal = true;
    }

    [RelayCommand]
    public void SetEditResult(MatchResult result)
    {
        EditResult = result;
    }

    [RelayCommand]
    public void CloseEditModal()
    {
        ShowEditModal = false;
        EditingMatch = null;
    }

    [RelayCommand]
    public async Task SaveEditedMatchAsync()
    {
        if (EditingMatch == null || SelectedEditDeck == null || string.IsNullOrWhiteSpace(EditOpponentArchetype))
            return;

        IsBusy = true;
        var req = new UpdateMatchRequest(
            DeckId: SelectedEditDeck.Id,
            OpponentArchetype: EditOpponentArchetype.Trim(),
            Result: EditResult,
            TournamentId: EditingMatch.TournamentId,
            RoundNumber: EditRoundNumber,
            TableNumber: EditTableNumber,
            OpponentName: EditOpponentName?.Trim(),
            OpponentPopId: EditOpponentPopId?.Trim(),
            CoinFlipWon: EditingMatch.CoinFlipWon,
            TurnOrder: EditingMatch.TurnOrder,
            PlayerMulligans: EditingMatch.PlayerMulligans,
            OpponentMulligans: EditingMatch.OpponentMulligans,
            PlayerPrizesRemaining: EditingMatch.PlayerPrizesRemaining,
            OpponentPrizesRemaining: EditingMatch.OpponentPrizesRemaining,
            WinCondition: EditingMatch.WinCondition,
            StartingActivePokemon: EditStartingActivePokemon?.Trim(),
            TacticalNotes: EditTacticalNotes?.Trim(),
            TechCardsUsed: EditingMatch.TechCardsUsed
        );

        await _dataService.UpdateMatchAsync(EditingMatch.Id, req);
        ShowEditModal = false;
        EditingMatch = null;
        await LoadMatchesAsync();
        IsBusy = false;
    }
}
