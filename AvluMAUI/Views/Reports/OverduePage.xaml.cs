using AvluMAUI.ViewModels.Reports;

namespace AvluMAUI.Views.Reports;

public partial class OverduePage : ContentPage
{
    public OverduePage(OverdueViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is OverdueViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
