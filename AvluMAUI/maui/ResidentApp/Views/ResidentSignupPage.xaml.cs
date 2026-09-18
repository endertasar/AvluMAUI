using Microsoft.Maui.Controls;
namespace Avlu.ResidentApp.Views;
public partial class ResidentSignupPage : ContentPage
{
    public ResidentSignupPage() { InitializeComponent(); }
    private async void OnBackTapped(object sender, System.EventArgs e) => await Shell.Current.GoToAsync("..");
    private async void OnContinueTapped(object sender, System.EventArgs e) => await Shell.Current.GoToAsync("property-select");
}
