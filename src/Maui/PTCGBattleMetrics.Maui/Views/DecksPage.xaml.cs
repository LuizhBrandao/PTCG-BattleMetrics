using PTCGBattleMetrics.Maui.ViewModels;

namespace PTCGBattleMetrics.Maui.Views;

public partial class DecksPage : ContentPage
{
    private readonly DecksViewModel _viewModel;

    public DecksPage(DecksViewModel viewModel)
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
