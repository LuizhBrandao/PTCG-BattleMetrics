using PTCGBattleMetrics.Domain.Enums;
using PTCGBattleMetrics.Maui.ViewModels;

namespace PTCGBattleMetrics.Maui.Views;

public partial class FastInputPage : ContentPage
{
    private readonly FastInputViewModel _viewModel;

    public FastInputPage(FastInputViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }

    private void OnCoinWonClicked(object? sender, EventArgs e)
    {
        _viewModel.CoinFlipWon = true;
    }

    private void OnCoinLostClicked(object? sender, EventArgs e)
    {
        _viewModel.CoinFlipWon = false;
    }

    private void OnFirstClicked(object? sender, EventArgs e)
    {
        _viewModel.TurnOrder = TurnOrder.First;
    }

    private void OnSecondClicked(object? sender, EventArgs e)
    {
        _viewModel.TurnOrder = TurnOrder.Second;
    }
}
