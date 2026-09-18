using Microsoft.Maui.Controls;
namespace Avlu.AdminApp.Views;
public partial class ExpensesListPage : ContentPage
{
    public ExpensesListPage()
    {
        InitializeComponent();
        BindingContext = new { Filters = new[] { "Bu Ay", "Bakım", "Peyzaj", "Fatura" } };
        ExpensesList.ItemsSource = new[]
        {
            new { DateShort = "14 Tem", Category = "Bakım", Description = "Asansör Bakımı", AmountDisplay = "3.200,00 ₺" },
            new { DateShort = "13 Tem", Category = "Peyzaj", Description = "Bahçe Düzenleme", AmountDisplay = "850,00 ₺" },
            new { DateShort = "02 Tem", Category = "Temizlik", Description = "Ortak Alan Temizliği", AmountDisplay = "2.100,00 ₺" },
        };
    }
    private async void OnAddTapped(object sender, System.EventArgs e) => await Shell.Current.GoToAsync("expense-form");
}
