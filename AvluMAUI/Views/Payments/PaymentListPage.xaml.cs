using AvluMAUI.ViewModels.Payments;

namespace AvluMAUI.Views.Payments;

public partial class PaymentListPage : ContentPage
{
    public PaymentListPage(PaymentListViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is PaymentListViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
