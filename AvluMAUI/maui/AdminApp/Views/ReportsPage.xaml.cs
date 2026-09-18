using Microsoft.Maui.Controls;
namespace Avlu.AdminApp.Views;
public partial class ReportsPage : ContentPage
{
    public ReportsPage()
    {
        InitializeComponent();
        DebtorsListRep.ItemsSource = new[]
        {
            new { Unit = "C Blok · D9", Owner = "Burak Öz", DebtDisplay = "6.250,00 ₺" },
            new { Unit = "A Blok · D12", Owner = "Ayşe Yıldız", DebtDisplay = "1.250,00 ₺" },
            new { Unit = "B Blok · D4", Owner = "Zeynep Arslan", DebtDisplay = "625,00 ₺" },
        };
    }

    private void OnTabChanged(object sender, CheckedChangedEventArgs e)
    {
        if (!e.Value || sender is not RadioButton rb) return;
        var idx = rb.Content?.ToString() switch { "Gelir/Gider" => 0, "Tahsilat Oranı" => 1, _ => 2 };
        IncomeExpenseTab.IsVisible = idx == 0;
        CollectionRateTab.IsVisible = idx == 1;
        DebtorsListRep.IsVisible = idx == 2;
    }
}
