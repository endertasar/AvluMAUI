using Microsoft.Maui.Controls;
using Avlu.Controls;
namespace Avlu.ResidentApp.Views;
public partial class DebtsPage : ContentPage
{
    public DebtsPage()
    {
        InitializeComponent();
        DuesList.ItemsSource = new[]
        {
            new { Period = "2026 Temmuz", AmountDisplay = "1.250,00 ₺", Status = StatusPill.Status.Gecikmis },
            new { Period = "2026 Haziran", AmountDisplay = "1.250,00 ₺", Status = StatusPill.Status.Odendi },
            new { Period = "2026 Mayıs", AmountDisplay = "1.250,00 ₺", Status = StatusPill.Status.Odendi },
        };
        ExtrasList.ItemsSource = new[]
        {
            new { Name = "Asansör Yenileme — 2/6 Taksit", AmountDisplay = "1.000,00 ₺", Status = StatusPill.Status.Bekliyor },
        };
    }
}
