using Microsoft.Maui.Controls;
namespace Avlu.AdminApp.Views;
public partial class ExtraPaymentDetailPage : ContentPage
{
    public ExtraPaymentDetailPage()
    {
        InitializeComponent();
        RowsList.ItemsSource = new[]
        {
            new { Unit = "A12", InstallmentDisplay = "6/6 taksit", Status = Avlu.Controls.StatusPill.Status.Odendi },
            new { Unit = "A14", InstallmentDisplay = "3/6 taksit", Status = Avlu.Controls.StatusPill.Status.Kismi },
            new { Unit = "B04", InstallmentDisplay = "0/6 taksit", Status = Avlu.Controls.StatusPill.Status.Bekliyor },
            new { Unit = "C09", InstallmentDisplay = "1/6 taksit", Status = Avlu.Controls.StatusPill.Status.Gecikmis },
        };
    }
}
