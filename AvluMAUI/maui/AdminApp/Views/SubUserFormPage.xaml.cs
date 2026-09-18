using Microsoft.Maui.Controls;
namespace Avlu.AdminApp.Views;
public partial class SubUserFormPage : ContentPage
{
    public SubUserFormPage() { InitializeComponent(); }
    private async void OnSaveTapped(object sender, System.EventArgs e) => await Shell.Current.GoToAsync("..");
}
