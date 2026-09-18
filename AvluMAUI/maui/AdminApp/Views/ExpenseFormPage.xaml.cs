using Microsoft.Maui.Controls;
namespace Avlu.AdminApp.Views;
public partial class ExpenseFormPage : ContentPage
{
    public ExpenseFormPage() { InitializeComponent(); CategoryPicker.SelectedIndex = 0; }
    private async void OnSaveTapped(object sender, System.EventArgs e) => await Shell.Current.GoToAsync("..");
}
