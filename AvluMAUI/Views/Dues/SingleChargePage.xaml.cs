using AvluMAUI.ViewModels.Dues;

namespace AvluMAUI.Views.Dues;

public partial class SingleChargePage : ContentPage
{
    public SingleChargePage(SingleChargeViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is SingleChargeViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
