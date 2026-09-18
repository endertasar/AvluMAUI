using AvluMAUI.Helpers;
using AvluMAUI.ViewModels.Auth;

namespace AvluMAUI.Views.Auth;

public partial class LoginPage : ContentPage
{
    private readonly TokenManager _tokens;

    public LoginPage(LoginViewModel vm, TokenManager tokens)
    {
        InitializeComponent();
        BindingContext = vm;
        _tokens = tokens;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_tokens.HasTenantToken)
            await NavigationHelper.GoToMainAsync();
    }
}
