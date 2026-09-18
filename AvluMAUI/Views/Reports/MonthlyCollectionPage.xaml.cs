using AvluMAUI.ViewModels.Reports;

namespace AvluMAUI.Views.Reports;

public partial class MonthlyCollectionPage : ContentPage
{
    public MonthlyCollectionPage(MonthlyCollectionViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is MonthlyCollectionViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
