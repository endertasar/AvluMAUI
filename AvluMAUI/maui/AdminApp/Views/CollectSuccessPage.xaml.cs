using Microsoft.Maui.Controls;
namespace Avlu.AdminApp.Views;
public partial class CollectSuccessPage : ContentPage
{
    public CollectSuccessPage() { InitializeComponent(); }
    private async void OnNewCollection(object sender, System.EventArgs e) => await Shell.Current.GoToAsync("//collect");
    private async void OnBackToPanel(object sender, System.EventArgs e) => await Shell.Current.GoToAsync("//panel");
}
