using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvluMAUI.ViewModels.Auth;

public partial class ForgotPasswordViewModel : ObservableObject
{
    [ObservableProperty] private string  _phoneNumber  = string.Empty;
    [ObservableProperty] private bool    _isBusy       = false;
    [ObservableProperty] private string? _errorMessage;

    [RelayCommand]
    private async Task BackAsync()
        => await Shell.Current.GoToAsync("..");

    [RelayCommand]
    private async Task SendCodeAsync()
    {
        if (string.IsNullOrWhiteSpace(PhoneNumber))
        {
            ErrorMessage = "Telefon numarası zorunludur.";
            return;
        }

        IsBusy       = true;
        ErrorMessage = null;

        try
        {
            // TODO: IAuthService.SendOtpAsync(PhoneNumber)
            await Task.Delay(500);
            await Shell.Current.GoToAsync("otp");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
