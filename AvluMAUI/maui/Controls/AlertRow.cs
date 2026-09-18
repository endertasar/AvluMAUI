using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace Avlu.Controls;

/// <summary>Panel uyarı satırı (pirinç arka plan, ikon + metin + ok).</summary>
public class AlertRow : Border
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(AlertRow), string.Empty, propertyChanged: OnChanged);

    public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
    public event System.EventHandler? Tapped;

    private readonly Label _label;

    public AlertRow()
    {
        Padding = new Thickness(14, 12);
        StrokeThickness = 0;
        StrokeShape = new RoundRectangle { CornerRadius = 12 };
        this.SetAppThemeColor(Border.BackgroundColorProperty, Color.FromArgb("#EFE3C6"), Color.FromArgb("#1AC8A15A"));

        _label = new Label { FontFamily = "Inter", FontSize = 13, TextColor = Color.FromArgb("#8A6412"), VerticalTextAlignment = TextAlignment.Center };

        var grid = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 10 };
        var icon = new Label { Text = "⚠", FontSize = 15, VerticalOptions = LayoutOptions.Center };
        var chevron = new Label { Text = "›", FontSize = 16, TextColor = Color.FromArgb("#8A6412"), VerticalOptions = LayoutOptions.Center };
        grid.Children.Add(icon);
        grid.Children.Add(_label); Grid.SetColumn(_label, 1);
        grid.Children.Add(chevron); Grid.SetColumn(chevron, 2);
        Content = grid;

        GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => Tapped?.Invoke(this, System.EventArgs.Empty)) });

        PropertyChanged += (_, e) => { if (e.PropertyName == nameof(Text)) _label.Text = Text; };
    }

    private static void OnChanged(BindableObject b, object o, object n) { }
}
