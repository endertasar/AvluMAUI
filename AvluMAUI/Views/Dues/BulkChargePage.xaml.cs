using AvluMAUI.ViewModels.Dues;

namespace AvluMAUI.Views.Dues;

public partial class BulkChargePage : ContentPage
{
    public BulkChargePage(BulkChargeViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is BulkChargeViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
