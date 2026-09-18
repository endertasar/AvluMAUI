using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace Avlu.Controls;

/// <summary>Panel özet kartı: etiket + büyük değer + opsiyonel alt metin.</summary>
public class SummaryCard : Border
{
    public static readonly BindableProperty LabelProperty =
        BindableProperty.Create(nameof(Label), typeof(string), typeof(SummaryCard), string.Empty, propertyChanged: OnChanged);
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(SummaryCard), string.Empty, propertyChanged: OnChanged);
    public static readonly BindableProperty SubProperty =
        BindableProperty.Create(nameof(Sub), typeof(string), typeof(SummaryCard), string.Empty, propertyChanged: OnChanged);
    public static readonly BindableProperty AccentProperty =
        BindableProperty.Create(nameof(Accent), typeof(bool), typeof(SummaryCard), false, propertyChanged: OnChanged);

    public string Label { get => (string)GetValue(LabelProperty); set => SetValue(LabelProperty, value); }
    public string Value { get => (string)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    public string Sub { get => (string)GetValue(SubProperty); set => SetValue(SubProperty, value); }
    public bool Accent { get => (bool)GetValue(AccentProperty); set => SetValue(AccentProperty, value); }

    private readonly Label _labelView, _valueView, _subView;

    public SummaryCard()
    {
        Padding = new Thickness(16);
        StrokeThickness = 1;
        StrokeShape = new RoundRectangle { CornerRadius = 16 };
        this.SetAppThemeColor(Border.BackgroundColorProperty, Colors.White, Color.FromArgb("#111C2E"));
        this.SetAppThemeColor(Border.StrokeProperty, Color.FromArgb("#E4E8EF"), Color.FromArgb("#223148"));

        _labelView = new Label { FontFamily = "Inter", FontSize = 11.5, FontAttributes = FontAttributes.Bold };
        _labelView.SetAppThemeColor(Microsoft.Maui.Controls.Label.TextColorProperty, Color.FromArgb("#5C6A7E"), Color.FromArgb("#8A9AB3"));

        _valueView = new Label { FontFamily = "InterSemiBold", FontSize = 20, Margin = new Thickness(0, 8, 0, 0) };
        _valueView.SetAppThemeColor(Microsoft.Maui.Controls.Label.TextColorProperty, Color.FromArgb("#0B1524"), Color.FromArgb("#EAEEF5"));

        _subView = new Label { FontFamily = "Inter", FontSize = 11.5, Margin = new Thickness(0, 4, 0, 0) };
        _subView.SetAppThemeColor(Microsoft.Maui.Controls.Label.TextColorProperty, Color.FromArgb("#5C6A7E"), Color.FromArgb("#8A9AB3"));

        Content = new VerticalStackLayout { Children = { _labelView, _valueView, _subView } };
        Apply();
    }

    private static void OnChanged(BindableObject b, object o, object n) => ((SummaryCard)b).Apply();

    private void Apply()
    {
        _labelView.Text = Label;
        _valueView.Text = Value;
        _subView.Text = Sub;
        _subView.IsVisible = !string.IsNullOrEmpty(Sub);
        if (Accent) _valueView.TextColor = Color.FromArgb("#C8A15A");
    }
}
