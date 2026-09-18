using Microsoft.Maui.Controls;
namespace Avlu.AdminApp.Views;
public partial class ExtraPaymentsListPage : ContentPage
{
    public ExtraPaymentsListPage()
    {
        InitializeComponent();
        ExtraList.ItemsSource = new[]
        {
            new { Name = "Asansör Yenileme", CollectedDisplay = "%68", Summary = "6.000,00 ₺ × mülk · 6 taksit · Toplam 252.000,00 ₺", CollectedPct = 68.0 },
            new { Name = "Cephe Boyası", CollectedDisplay = "%32", Summary = "1.800,00 ₺ × mülk · 3 taksit · Toplam 75.600,00 ₺", CollectedPct = 32.0 },
        };
    }
    private async void OnAddTapped(object sender, System.EventArgs e) => await Shell.Current.GoToAsync("extra-form");
    private async void OnItemTapped(object sender, System.EventArgs e) => await Shell.Current.GoToAsync("extra-detail");
}
