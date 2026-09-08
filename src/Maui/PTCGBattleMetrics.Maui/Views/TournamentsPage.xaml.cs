using PTCGBattleMetrics.Maui.ViewModels;

namespace PTCGBattleMetrics.Maui.Views;

public partial class TournamentsPage : ContentPage
{
    private readonly TournamentsViewModel _viewModel;

    public TournamentsPage(TournamentsViewModel viewModel)
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
