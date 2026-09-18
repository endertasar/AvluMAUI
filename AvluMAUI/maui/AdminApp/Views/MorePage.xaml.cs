using Microsoft.Maui.Controls;
namespace Avlu.AdminApp.Views;
public partial class MorePage : ContentPage
{
    public MorePage() { InitializeComponent(); }
    private async void OnDuesTapped(object sender, System.EventArgs e) => await Shell.Current.GoToAsync("dues");
    private async void OnExtraTapped(object sender, System.EventArgs e) => await Shell.Current.GoToAsync("extra");
    private async void OnExpensesTapped(object sender, System.EventArgs e) => await Shell.Current.GoToAsync("expenses");
    private async void OnNotifyTapped(object sender, System.EventArgs e) => await Shell.Current.GoToAsync("notify");
    private async void OnSubUsersTapped(object sender, System.EventArgs e) => await Shell.Current.GoToAsync("subusers");
    private async void OnLogoutTapped(object sender, System.EventArgs e) => await Shell.Current.GoToAsync("///login");
}
