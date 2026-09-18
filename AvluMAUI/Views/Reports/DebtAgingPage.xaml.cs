using AvluMAUI.ViewModels.Reports;

namespace AvluMAUI.Views.Reports;

public partial class DebtAgingPage : ContentPage
{
    public DebtAgingPage(DebtAgingViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is DebtAgingViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
