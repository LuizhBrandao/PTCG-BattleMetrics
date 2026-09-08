using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PTCGBattleMetrics.Application.DTOs;
using PTCGBattleMetrics.Domain.Enums;
using PTCGBattleMetrics.Maui.Messages;
using PTCGBattleMetrics.Maui.Services.Data;
using PTCGBattleMetrics.Maui.Services.Settings;
using PTCGBattleMetrics.Maui.Validations;
using PTCGBattleMetrics.Maui.Validations.Rules;
using PTCGBattleMetrics.Maui.ViewModels.Base;

namespace PTCGBattleMetrics.Maui.ViewModels;

public partial class TournamentsViewModel : ViewModelBase
{
    private readonly IMauiBattleMetricsService _dataService;
    private readonly ISettingsService _settings;

    [ObservableProperty]
    private ObservableCollection<TournamentResponse> _tournaments = new();

    [ObservableProperty]
    private Guid? _activeTournamentId;

    [ObservableProperty]
    private bool _showCreateModal;

    [ObservableProperty]
    private ValidatableObject<string> _tourneyName = new();

    [ObservableProperty]
    private string? _venue;

    [ObservableProperty]
    private DateTime _tourneyDate = DateTime.Today;

    [ObservableProperty]
    private TournamentCategory _category = TournamentCategory.LeagueCup;

    [ObservableProperty]
    private TournamentFormat _format = TournamentFormat.SwissBo3;

    [ObservableProperty]
    private int? _totalParticipants;

    [ObservableProperty]
    private int? _finalStanding;

    [ObservableProperty]
    private int? _championshipPoints;

    public TournamentsViewModel(IMauiBattleMetricsService dataService, ISettingsService settings)
    {
        _dataService = dataService;
        _settings = settings;
        Title = "🏆 Torneios";

        TourneyName.Validations.Add(new IsNotNullOrEmptyRule<string>
        {
            ValidationMessage = "O nome do torneio é obrigatório"
        });
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        IsBusy = true;
        await LoadTournamentsAsync();
        IsBusy = false;
    }

    private async Task LoadTournamentsAsync()
    {
        var list = await _dataService.GetTournamentsAsync();
        Tournaments = new ObservableCollection<TournamentResponse>(list);
        ActiveTournamentId = _settings.ActiveTournamentId;
    }

    [RelayCommand]
    private void ToggleCreateModal()
    {
        ShowCreateModal = !ShowCreateModal;
    }

    [RelayCommand]
    public async Task CreateTournamentAsync()
    {
        TourneyName.Validate();
        if (!TourneyName.IsValid)
            return;

        IsBusy = true;

        var request = new CreateTournamentRequest(
            Name: TourneyName.Value!.Trim(),
            StoreOrVenue: Venue?.Trim(),
            Date: DateOnly.FromDateTime(TourneyDate),
            Category: Category,
            Format: Format,
            TotalParticipants: TotalParticipants,
            FinalStanding: FinalStanding,
            ChampionshipPoints: ChampionshipPoints
        );

        var created = await _dataService.CreateTournamentAsync(request);
        if (created != null)
        {
            await LoadTournamentsAsync();
            ShowCreateModal = false;
            TourneyName.Value = string.Empty;
            Venue = null;
            TotalParticipants = null;
            FinalStanding = null;
            ChampionshipPoints = null;
        }

        IsBusy = false;
    }

    [RelayCommand]
    private void SetActiveTournament(Guid? id)
    {
        _settings.ActiveTournamentId = id;
        ActiveTournamentId = id;
        WeakReferenceMessenger.Default.Send(new ActiveTournamentChangedMessage(id));
    }
}
