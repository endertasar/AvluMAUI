using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace Avlu.Controls;

/// <summary>Durum rozeti: Bekliyor / Kısmi / Ödendi / Gecikmiş.</summary>
public class StatusPill : Border
{
    public enum Status { Bekliyor, Kismi, Odendi, Gecikmis }

    public static readonly BindableProperty StatusValueProperty =
        BindableProperty.Create(nameof(StatusValue), typeof(Status), typeof(StatusPill), Status.Bekliyor,
            propertyChanged: OnChanged);

    public Status StatusValue { get => (Status)GetValue(StatusValueProperty); set => SetValue(StatusValueProperty, value); }

    private readonly Label _label;

    public StatusPill()
    {
        Padding = new Thickness(10, 4);
        StrokeThickness = 0;
        StrokeShape = new RoundRectangle { CornerRadius = 999 };
        HorizontalOptions = LayoutOptions.Start;
        _label = new Label { FontFamily = "InterSemiBold", FontSize = 11 };
        Content = _label;
        Apply();
    }

    private static void OnChanged(BindableObject b, object o, object n) => ((StatusPill)b).Apply();

    private void Apply()
    {
        var (bg, fg, text) = StatusValue switch
        {
            Status.Bekliyor => (Color.FromArgb("#EEF0F4"), Color.FromArgb("#5C6A7E"), "Bekliyor"),
            Status.Kismi    => (Color.FromArgb("#FBEEDD"), Color.FromArgb("#B4700F"), "Kısmi"),
            Status.Odendi   => (Color.FromArgb("#E3F2E9"), Color.FromArgb("#2E7D57"), "Ödendi"),
            Status.Gecikmis => (Color.FromArgb("#FBEAE9"), Color.FromArgb("#B3261E"), "Gecikmiş"),
            _ => (Colors.Transparent, Colors.Black, ""),
        };
        BackgroundColor = bg;
        _label.TextColor = fg;
        _label.Text = text;
        // Dark mode tonları: tüm renkler zaten okunabilir kontrastta; koyu temada
        // hafif saydamlaştırmak istersen AppThemeBinding ile bg'yi override edebilirsin.
    }
}
