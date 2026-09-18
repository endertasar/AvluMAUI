using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Collections.Generic;
namespace Avlu.ResidentApp.Views;

// Basitlik için CollectionView.GroupHeaderTemplate ile ay bazlı gruplama önerilir.
// Burada tek CollectionView + IsGroupingEnabled kullanılabilir; örnek veri modeli:
public class PaymentItem { public string Label { get; set; } = ""; public string AmountDisplay { get; set; } = ""; public string Method { get; set; } = ""; public string Date { get; set; } = ""; }
public class PaymentGroup : List<PaymentItem> { public string Month { get; set; } = ""; public PaymentGroup(string month, IEnumerable<PaymentItem> items) : base(items) { Month = month; } }

public partial class PaymentsPage : ContentPage
{
    public PaymentsPage()
    {
        InitializeComponent();
        var groups = new List<PaymentGroup>
        {
            new("Temmuz 2026", new[] { new PaymentItem { Label = "2026 Temmuz Aidatı", AmountDisplay = "1.250,00 ₺", Method = "Havale", Date = "3 Tem" } }),
            new("Haziran 2026", new[] {
                new PaymentItem { Label = "2026 Haziran Aidatı", AmountDisplay = "1.250,00 ₺", Method = "Nakit", Date = "2 Haz" },
                new PaymentItem { Label = "Asansör Yenileme — 1/6 Taksit", AmountDisplay = "1.000,00 ₺", Method = "Havale", Date = "2 Haz" },
            }),
        };
        GroupedList.IsGrouped = true;
        GroupedList.ItemsSource = groups;
        GroupedList.GroupHeaderTemplate = new DataTemplate(() =>
        {
            var label = new Label { FontFamily = "InterSemiBold", FontSize = 13, Margin = new Thickness(0, 14, 0, 8) };
            label.SetBinding(Label.TextProperty, new Binding(nameof(PaymentGroup.Month)));
            return label;
        });
        GroupedList.ItemTemplate = new DataTemplate(() =>
        {
            var grid = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, Padding = new Thickness(14, 13), Margin = new Thickness(0, 0, 0, 8) };
            var stack = new VerticalStackLayout();
            var title = new Label { FontFamily = "InterSemiBold", FontSize = 13.5 };
            title.SetBinding(Label.TextProperty, new Binding(nameof(PaymentItem.Label)));
            var sub = new Label { FontSize = 11.5, TextColor = Color.FromArgb("#5C6A7E") };
            sub.SetBinding(Label.TextProperty, new Binding(".", stringFormat: "{0}"));
            stack.Children.Add(title);
            var amount = new Label { FontFamily = "InterSemiBold", FontSize = 13.5, TextColor = Color.FromArgb("#2E7D57"), VerticalOptions = LayoutOptions.Center };
            amount.SetBinding(Label.TextProperty, new Binding(nameof(PaymentItem.AmountDisplay)));
            grid.Children.Add(stack);
            grid.Children.Add(amount); Grid.SetColumn(amount, 1);
            return grid;
        });
    }
}
