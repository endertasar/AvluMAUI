using AvluMAUI.ViewModels.Payments;

namespace AvluMAUI.Views.Payments;

public partial class PaymentCollectPage : ContentPage
{
    public PaymentCollectPage(PaymentCollectViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is PaymentCollectViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
