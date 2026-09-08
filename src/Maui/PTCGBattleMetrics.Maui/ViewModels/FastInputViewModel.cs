using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PTCGBattleMetrics.Application.DTOs;
using PTCGBattleMetrics.Domain.Entities;
using PTCGBattleMetrics.Domain.Enums;
using PTCGBattleMetrics.Maui.Messages;
using PTCGBattleMetrics.Maui.Services.Data;
using PTCGBattleMetrics.Maui.Services.Settings;
using PTCGBattleMetrics.Maui.ViewModels.Base;

namespace PTCGBattleMetrics.Maui.ViewModels;

public partial class FastInputViewModel : ViewModelBase
{
    private readonly IMauiBattleMetricsService _dataService;
    private readonly ISettingsService _settings;

    [ObservableProperty]
    private ObservableCollection<DeckResponse> _decks = new();

    [ObservableProperty]
    private DeckResponse? _selectedDeck;

    [ObservableProperty]
    private ObservableCollection<MetaArchetype> _metaArchetypes = new();

    [ObservableProperty]
    private string _selectedArchetype = string.Empty;

    [ObservableProperty]
    private MatchResult _selectedResult = MatchResult.Win;

    // Active tournament banner
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasActiveTournament))]
    private TournamentResponse? _activeTournament;

    public bool HasActiveTournament => ActiveTournament != null;

    // Progressive Disclosure: Detailed Telemetry
    [ObservableProperty]
    private bool _showTelemetry;

    [ObservableProperty]
    private int? _roundNumber;

    [ObservableProperty]
    private int? _tableNumber;

    [ObservableProperty]
    private string? _opponentName;

    [ObservableProperty]
    private string? _opponentPopId;

    [ObservableProperty]
    private bool? _coinFlipWon;

    [ObservableProperty]
    private TurnOrder? _turnOrder;

    [ObservableProperty]
    private int _playerMulligans = 0;

    [ObservableProperty]
    private int _opponentMulligans = 0;

    [ObservableProperty]
    private int _playerPrizesRemaining = 0;

    [ObservableProperty]
    private int _opponentPrizesRemaining = 6;

    [ObservableProperty]
    private WinCondition _winCondition = WinCondition.PrizeKnockout;

    [ObservableProperty]
    private string? _startingActivePokemon;

    [ObservableProperty]
    private string? _tacticalNotes;

    [ObservableProperty]
    private ObservableCollection<string> _availableTechCards = new();

    [ObservableProperty]
    private ObservableCollection<string> _selectedTechCards = new();

    // Feedback message
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasStatusMessage))]
    private string? _statusMessage;

    public bool HasStatusMessage => !string.IsNullOrWhiteSpace(StatusMessage);

    [ObservableProperty]
    private int _pendingSyncCount;

    [ObservableProperty]
    private bool _isOnline;

    public FastInputViewModel(IMauiBattleMetricsService dataService, ISettingsService settings)
    {
        _dataService = dataService;
        _settings = settings;
        Title = "⚡ Torneio";

        // Subscribe to messenger events (Chapter 5)
        WeakReferenceMessenger.Default.Register<ActiveDeckChangedMessage>(this, async (r, m) =>
        {
            await LoadDecksAsync();
        });

        WeakReferenceMessenger.Default.Register<ActiveTournamentChangedMessage>(this, async (r, m) =>
        {
            await LoadTournamentAsync();
        });
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        IsBusy = true;
        await LoadDecksAsync();
        await LoadArchetypesAsync();
        await LoadTournamentAsync();
        UpdateStatus();
        IsBusy = false;
    }

    private async Task LoadDecksAsync()
    {
        var deckList = await _dataService.GetDecksAsync();
        Decks = new ObservableCollection<DeckResponse>(deckList);

        var activeId = _settings.ActiveDeckId;
        if (activeId.HasValue)
        {
            SelectedDeck = Decks.FirstOrDefault(d => d.Id == activeId.Value) ?? Decks.FirstOrDefault();
        }
        else
        {
            SelectedDeck = Decks.FirstOrDefault();
        }

        UpdateAvailableTechCards();
    }

    private async Task LoadArchetypesAsync()
    {
        var list = await _dataService.GetArchetypesAsync();
        MetaArchetypes = new ObservableCollection<MetaArchetype>(list);
    }

    private async Task LoadTournamentAsync()
    {
        var activeTourneyId = _settings.ActiveTournamentId;
        if (activeTourneyId.HasValue)
        {
            var tourneys = await _dataService.GetTournamentsAsync();
            ActiveTournament = tourneys.FirstOrDefault(t => t.Id == activeTourneyId.Value);

            if (ActiveTournament != null)
            {
                var matches = await _dataService.GetMatchesAsync(tournamentId: ActiveTournament.Id);
                RoundNumber = matches.Count + 1;
            }
        }
        else
        {
            ActiveTournament = null;
        }
    }

    partial void OnSelectedDeckChanged(DeckResponse? value)
    {
        if (value != null)
        {
            _settings.ActiveDeckId = value.Id;
        }
        UpdateAvailableTechCards();
    }

    private void UpdateAvailableTechCards()
    {
        AvailableTechCards.Clear();
        SelectedTechCards.Clear();
        if (SelectedDeck?.TechCards != null)
        {
            foreach (var t in SelectedDeck.TechCards)
            {
                AvailableTechCards.Add(t);
            }
        }
    }

    [RelayCommand]
    private void SelectArchetype(string archetype)
    {
        SelectedArchetype = archetype;
    }

    [RelayCommand]
    private void SetResult(MatchResult result)
    {
        SelectedResult = result;
    }

    [RelayCommand]
    private void ToggleTelemetry()
    {
        ShowTelemetry = !ShowTelemetry;
    }

    [RelayCommand]
    private void ToggleTechCard(string tech)
    {
        if (SelectedTechCards.Contains(tech))
            SelectedTechCards.Remove(tech);
        else
            SelectedTechCards.Add(tech);
    }

    [RelayCommand]
    private void IncrementPlayerMulligans() => PlayerMulligans++;

    [RelayCommand]
    private void DecrementPlayerMulligans() => PlayerMulligans = Math.Max(0, PlayerMulligans - 1);

    [RelayCommand]
    private void IncrementOpponentMulligans() => OpponentMulligans++;

    [RelayCommand]
    private void DecrementOpponentMulligans() => OpponentMulligans = Math.Max(0, OpponentMulligans - 1);

    [RelayCommand]
    public async Task SaveMatchAsync()
    {
        if (SelectedDeck == null || string.IsNullOrWhiteSpace(SelectedArchetype))
        {
            StatusMessage = "Selecione o deck e o arquétipo enfrentado!";
            return;
        }

        IsBusy = true;

        var request = new CreateMatchRequest(
            DeckId: SelectedDeck.Id,
            OpponentArchetype: SelectedArchetype.Trim(),
            Result: SelectedResult,
            TournamentId: ActiveTournament?.Id,
            RoundNumber: RoundNumber,
            TableNumber: TableNumber,
            OpponentName: OpponentName,
            OpponentPopId: OpponentPopId,
            CoinFlipWon: CoinFlipWon,
            TurnOrder: TurnOrder,
            PlayerMulligans: PlayerMulligans,
            OpponentMulligans: OpponentMulligans,
            PlayerPrizesRemaining: ShowTelemetry ? PlayerPrizesRemaining : (SelectedResult == MatchResult.Win ? 0 : 6),
            OpponentPrizesRemaining: ShowTelemetry ? OpponentPrizesRemaining : (SelectedResult == MatchResult.Loss ? 0 : 6),
            WinCondition: ShowTelemetry ? WinCondition : null,
            StartingActivePokemon: StartingActivePokemon,
            TacticalNotes: TacticalNotes,
            TechCardsUsed: SelectedTechCards.ToList(),
            CreatedAt: DateTimeOffset.UtcNow
        );

        var saved = await _dataService.SaveMatchAsync(request);

        StatusMessage = $"✓ Partida salva com sucesso! ({saved.Result} vs {saved.OpponentArchetype})";

        // Reset for next round
        if (RoundNumber.HasValue) RoundNumber++;
        SelectedArchetype = string.Empty;
        OpponentName = null;
        OpponentPopId = null;
        StartingActivePokemon = null;
        TacticalNotes = null;
        SelectedTechCards.Clear();
        PlayerMulligans = 0;
        OpponentMulligans = 0;
        ShowTelemetry = false;

        UpdateStatus();
        IsBusy = false;
    }

    [RelayCommand]
    public async Task SyncPendingAsync()
    {
        IsBusy = true;
        var synced = await _dataService.SyncPendingMatchesAsync();
        StatusMessage = synced > 0 ? $"✓ {synced} partidas sincronizadas!" : "Nenhuma partida pendente.";
        UpdateStatus();
        IsBusy = false;
    }

    private void UpdateStatus()
    {
        PendingSyncCount = _dataService.PendingSyncCount;
        IsOnline = _dataService.IsOnline;
    }
}
