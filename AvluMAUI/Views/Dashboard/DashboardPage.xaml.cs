using AvluMAUI.ViewModels.Dashboard;

namespace AvluMAUI.Views.Dashboard;

public partial class DashboardPage : ContentPage
{
    public DashboardPage(DashboardViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is DashboardViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
