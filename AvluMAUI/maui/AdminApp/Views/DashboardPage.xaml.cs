using Microsoft.Maui.Controls;
using Avlu.AdminApp.ViewModels;
using Avlu.Controls;

namespace Avlu.AdminApp.Views;

public partial class DashboardPage : ContentPage
{
    private readonly DashboardViewModel _vm;

    public DashboardPage(DashboardViewModel vm)
    {
        _vm = vm;
        InitializeComponent();
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadCommand.ExecuteAsync(null);

        if (_vm.ErrorMessage is not null)
            StateHost.CurrentState = DataStateView.ViewState.Error;
        else
            StateHost.CurrentState = DataStateView.ViewState.Content;
    }

    private async void OnDuesAlertTapped(object sender, System.EventArgs e)
        => await Shell.Current.GoToAsync("dues");

    private async void OnDebtorsAlertTapped(object sender, System.EventArgs e)
        => await Shell.Current.GoToAsync("reports");
}
