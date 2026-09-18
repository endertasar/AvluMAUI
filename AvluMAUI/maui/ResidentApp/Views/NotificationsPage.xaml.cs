using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
namespace Avlu.ResidentApp.Views;
public partial class NotificationsPage : ContentPage
{
    public NotificationsPage()
    {
        InitializeComponent();
        var unreadBg = Color.FromArgb("#EFE3C6");
        var readBg = Colors.White;
        NotifList.ItemsSource = new[]
        {
            new { Title = "Ağustos Aidat Hatırlatması", Body = "Ağustos ayı aidat ödemeniz 5 Ağustos'a kadar bekleniyor.", Time = "2 sa önce", Unread = true, CardBg = unreadBg },
            new { Title = "Su Kesintisi Duyurusu", Body = "Yarın 09:00-13:00 arası bakım nedeniyle su kesintisi olacaktır.", Time = "1 gün önce", Unread = true, CardBg = unreadBg },
            new { Title = "Tahsilat Alındı", Body = "2026 Haziran aidatınız için ödemeniz alınmıştır.", Time = "3 gün önce", Unread = false, CardBg = readBg },
        };
    }
    private async void OnNotificationTapped(object sender, System.EventArgs e) => await Shell.Current.GoToAsync("notification-detail");
}
