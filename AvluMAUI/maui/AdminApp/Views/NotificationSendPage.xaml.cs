using Microsoft.Maui.Controls;
namespace Avlu.AdminApp.Views;
public partial class NotificationSendPage : ContentPage
{
    public NotificationSendPage()
    {
        InitializeComponent();
        SentList.ItemsSource = new[]
        {
            new { Title = "Temmuz Aidat Hatırlatması", Date = "10 Tem", CountDisplay = "42 alıcı" },
            new { Title = "Su Kesintisi Duyurusu", Date = "3 Tem", CountDisplay = "42 alıcı" },
        };
    }
    private async void OnSendTapped(object sender, System.EventArgs e) => await DisplayAlert("Gönderildi", "Bildirim gönderildi.", "Tamam");
}
