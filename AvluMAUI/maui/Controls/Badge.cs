using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace Avlu.Controls;

/// <summary>Küçük etiket rozeti (Daire/Dükkan/Otopark, Owner/Alt Kullanıcı vb.)</summary>
public class Badge : Border
{
    public enum Tone { Neutral, Navy, Owner }

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(Badge), string.Empty, propertyChanged: OnChanged);
    public static readonly BindableProperty ToneValueProperty =
        BindableProperty.Create(nameof(ToneValue), typeof(Tone), typeof(Badge), Tone.Neutral, propertyChanged: OnChanged);

    public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
    public Tone ToneValue { get => (Tone)GetValue(ToneValueProperty); set => SetValue(ToneValueProperty, value); }

    private readonly Label _label;

    public Badge()
    {
        Padding = new Thickness(9, 3);
        StrokeThickness = 0;
        StrokeShape = new RoundRectangle { CornerRadius = 7 };
        HorizontalOptions = LayoutOptions.Start;
        _label = new Label { FontFamily = "InterSemiBold", FontSize = 10.5 };
        Content = _label;
        Apply();
    }

    private static void OnChanged(BindableObject b, object o, object n) => ((Badge)b).Apply();

    private void Apply()
    {
        var dark = Application.Current?.RequestedTheme == AppTheme.Dark;
        var (bgL, bgD, fgL, fgD) = ToneValue switch
        {
            Tone.Navy  => (Color.FromArgb("#EFE3C6"), Color.FromArgb("#28C8A15A"), Color.FromArgb("#0E2A47"), Color.FromArgb("#C8A15A")),
            Tone.Owner => (Color.FromArgb("#EFE3C6"), Color.FromArgb("#2CC8A15A"), Color.FromArgb("#8A6412"), Color.FromArgb("#C8A15A")),
            _ => (Color.FromArgb("#F0F3F8"), Color.FromArgb("#14FFFFFF"), Color.FromArgb("#5C6A7E"), Color.FromArgb("#8A9AB3")),
        };
        BackgroundColor = dark ? bgD : bgL;
        _label.TextColor = dark ? fgD : fgL;
        _label.Text = Text;
    }
}
