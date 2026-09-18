using Microsoft.Maui.Controls;

namespace Avlu.Views;

public partial class SplashPage : ContentPage
{
    public SplashPage()
    {
        InitializeComponent();
        // 1.5 saniye sonra LoginPage'e geç.
        Dispatcher.DispatchDelayed(System.TimeSpan.FromMilliseconds(1500), async () =>
        {
            await Shell.Current.GoToAsync("//login");
        });
    }
}
