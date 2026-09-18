using Microsoft.Maui.Controls;
namespace Avlu.ResidentApp.Views;
public partial class ProfilePage : ContentPage
{
    public ProfilePage() { InitializeComponent(); }
    private async void OnChangePropertyTapped(object sender, System.EventArgs e) => await Shell.Current.GoToAsync("property-select");
    private async void OnLogoutTapped(object sender, System.EventArgs e) => await Shell.Current.GoToAsync("///login");
}
