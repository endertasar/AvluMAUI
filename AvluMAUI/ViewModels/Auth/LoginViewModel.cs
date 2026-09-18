using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvluMAUI.Helpers;
using AvluMAUI.Models.Auth;
using AvluMAUI.Services.Interfaces;

namespace AvluMAUI.ViewModels.Auth;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _auth;
    private readonly TokenManager _tokens;

    [ObservableProperty] private string  _siteUsername  = string.Empty;
    [ObservableProperty] private string  _username      = string.Empty;
    [ObservableProperty] private string  _password      = string.Empty;
    [ObservableProperty] private bool    _isBusy        = false;
    [ObservableProperty] private string? _errorMessage;

    public LoginViewModel(IAuthService auth, TokenManager tokens)
    {
        _auth   = auth;
        _tokens = tokens;
    }

    [RelayCommand]
    private static async Task GoToSignupAsync() =>
        await Shell.Current.GoToAsync("signup");

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(SiteUsername) ||
            string.IsNullOrWhiteSpace(Username)     ||
            string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Tüm alanlar zorunludur.";
            return;
        }

        IsBusy       = true;
        ErrorMessage = null;

        try
        {
            var result = await _auth.LoginAdminAsync(new AdminLoginRequest
            {
                SiteUsername = SiteUsername.Trim(),
                Username     = Username.Trim(),
                Password     = Password
            });

            if (!result.Success || result.Data is null)
            {
                ErrorMessage = result.Message ?? "Giriş başarısız.";
                return;
            }

            await _tokens.SetTokensAsync(result.Data.AccessToken, result.Data.RefreshToken);
            await NavigationHelper.GoToMainAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }
}
