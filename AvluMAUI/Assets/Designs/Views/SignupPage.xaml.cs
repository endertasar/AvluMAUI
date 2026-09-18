using Microsoft.Maui.Controls;

namespace Avlu.Views;

public partial class SignupPage : ContentPage
{
    public SignupPage() { InitializeComponent(); }

    private async void OnBackTapped(object sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("..");

    private async void OnContinueTapped(object sender, System.EventArgs e)
    {
        // TODO: AuthService.RegisterAsync(...)
        await Shell.Current.GoToAsync("otp");
    }
}
