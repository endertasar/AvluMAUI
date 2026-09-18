using AvluMAUI.ViewModels.Auth;

namespace AvluMAUI.Views.Auth;

public partial class TenantSelectPage : ContentPage
{
    public TenantSelectPage(TenantSelectViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is TenantSelectViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
