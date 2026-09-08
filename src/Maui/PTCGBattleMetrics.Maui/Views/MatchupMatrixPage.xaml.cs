using PTCGBattleMetrics.Maui.ViewModels;

namespace PTCGBattleMetrics.Maui.Views;

public partial class MatchupMatrixPage : ContentPage
{
    private readonly MatchupMatrixViewModel _viewModel;

    public MatchupMatrixPage(MatchupMatrixViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}
