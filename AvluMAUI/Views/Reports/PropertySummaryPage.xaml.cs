using AvluMAUI.ViewModels.Reports;

namespace AvluMAUI.Views.Reports;

public partial class PropertySummaryPage : ContentPage
{
    public PropertySummaryPage(PropertySummaryViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is PropertySummaryViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
