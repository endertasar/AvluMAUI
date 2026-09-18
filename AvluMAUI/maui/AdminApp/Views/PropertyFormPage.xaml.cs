using Microsoft.Maui.Controls;

namespace Avlu.AdminApp.Views;

public partial class PropertyFormPage : ContentPage
{
    public PropertyFormPage() { InitializeComponent(); TypePicker.SelectedIndex = 0; }
    private async void OnSaveTapped(object sender, System.EventArgs e) => await Shell.Current.GoToAsync("..");
}
