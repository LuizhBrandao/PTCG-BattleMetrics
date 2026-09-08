using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PTCGBattleMetrics.Application.DTOs;
using PTCGBattleMetrics.Application.Services;
using PTCGBattleMetrics.Domain.Enums;
using PTCGBattleMetrics.Maui.Messages;
using PTCGBattleMetrics.Maui.Services.Data;
using PTCGBattleMetrics.Maui.Services.Settings;
using PTCGBattleMetrics.Maui.Validations;
using PTCGBattleMetrics.Maui.Validations.Rules;
using PTCGBattleMetrics.Maui.ViewModels.Base;

namespace PTCGBattleMetrics.Maui.ViewModels;

public partial class DecksViewModel : ViewModelBase
{
    private readonly IMauiBattleMetricsService _dataService;
    private readonly ISettingsService _settings;

    [ObservableProperty]
    private ObservableCollection<DeckResponse> _decks = new();

    [ObservableProperty]
    private Guid? _activeDeckId;

    [ObservableProperty]
    private bool _showImportModal;

    [ObservableProperty]
    private ValidatableObject<string> _importDeckName = new();

    [ObservableProperty]
    private string _importArchetype = string.Empty;

    [ObservableProperty]
    private string _importVersion = "v1.0";

    [ObservableProperty]
    private string _rawListText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasParsedList))]
    private PtcglParseResult? _parseResult;

    public bool HasParsedList => ParseResult != null && ParseResult.Cards.Count > 0;

    [ObservableProperty]
    private ObservableCollection<string> _suggestedTechCards = new();

    [ObservableProperty]
    private ObservableCollection<string> _selectedTechCards = new();

    public DecksViewModel(IMauiBattleMetricsService dataService, ISettingsService settings)
    {
        _dataService = dataService;
        _settings = settings;
        Title = "🃏 Decks & Listas";

        ImportDeckName.Validations.Add(new IsNotNullOrEmptyRule<string>
        {
            ValidationMessage = "O nome do deck é obrigatório"
        });
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        IsBusy = true;
        await LoadDecksAsync();
        IsBusy = false;
    }

    private async Task LoadDecksAsync()
    {
        var list = await _dataService.GetDecksAsync();
        Decks = new ObservableCollection<DeckResponse>(list);
        ActiveDeckId = _settings.ActiveDeckId;
    }

    [RelayCommand]
    private void ToggleImportModal()
    {
        ShowImportModal = !ShowImportModal;
    }

    [RelayCommand]
    private void ParseList()
    {
        if (string.IsNullOrWhiteSpace(RawListText))
        {
            ParseResult = null;
            SuggestedTechCards.Clear();
            SelectedTechCards.Clear();
            return;
        }

        ParseResult = PtcglDeckParser.Parse(RawListText);

        SuggestedTechCards.Clear();
        SelectedTechCards.Clear();
        foreach (var tech in ParseResult.SuggestedTechCards)
        {
            SuggestedTechCards.Add(tech);
            SelectedTechCards.Add(tech);
        }

        if (string.IsNullOrWhiteSpace(ImportDeckName.Value) && ParseResult.Cards.Count > 0)
        {
            var mainPokemon = ParseResult.Cards.FirstOrDefault(c => c.CardType == CardType.Pokemon && c.Quantity >= 2);
            if (mainPokemon != null)
            {
                ImportDeckName.Value = $"{mainPokemon.Name} Deck";
                if (string.IsNullOrWhiteSpace(ImportArchetype))
                    ImportArchetype = mainPokemon.Name;
            }
        }
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
    public async Task SaveImportedDeckAsync()
    {
        ImportDeckName.Validate();
        if (!ImportDeckName.IsValid || ParseResult == null || ParseResult.Cards.Count == 0)
            return;

        IsBusy = true;

        var request = new CreateDeckRequest(
            Name: ImportDeckName.Value!.Trim(),
            Archetype: string.IsNullOrWhiteSpace(ImportArchetype) ? "Geral" : ImportArchetype.Trim(),
            Version: string.IsNullOrWhiteSpace(ImportVersion) ? "v1.0" : ImportVersion.Trim(),
            RawList: RawListText,
            Cards: ParseResult.Cards,
            TechCards: SelectedTechCards.ToList()
        );

        var created = await _dataService.CreateDeckAsync(request);
        if (created != null)
        {
            await LoadDecksAsync();
            ShowImportModal = false;
            RawListText = string.Empty;
            ImportDeckName.Value = string.Empty;
            ImportArchetype = string.Empty;
            ParseResult = null;
            SelectedTechCards.Clear();
        }

        IsBusy = false;
    }

    [RelayCommand]
    private void SetActiveDeck(Guid deckId)
    {
        _settings.ActiveDeckId = deckId;
        ActiveDeckId = deckId;
        WeakReferenceMessenger.Default.Send(new ActiveDeckChangedMessage(deckId));
    }
}
