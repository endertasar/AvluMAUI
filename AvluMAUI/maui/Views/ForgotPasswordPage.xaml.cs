using Microsoft.Maui.Controls;

namespace Avlu.Views;

public partial class ForgotPasswordPage : ContentPage
{
    public ForgotPasswordPage() { InitializeComponent(); }

    private async void OnBackTapped(object sender, System.EventArgs e)
        => await Shell.Current.GoToAsync("..");

    private async void OnSendCode(object sender, System.EventArgs e)
    {
        // TODO: AuthService.SendOtpAsync(phone)
        await Shell.Current.GoToAsync("otp");
    }
}
