using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Avlu.Controls;

/// <summary>
/// Avlu özel input alanı: etiket + prefix + entry + opsiyonel sağ ikon.
/// Focus durumunda kenarlık lacivert/pirinç'e geçer + yumuşak outer shadow ring.
/// </summary>
public class AvluField : VerticalStackLayout
{
    public static readonly BindableProperty LabelProperty =
        BindableProperty.Create(nameof(Label), typeof(string), typeof(AvluField), string.Empty);
    public static readonly BindableProperty PrefixProperty =
        BindableProperty.Create(nameof(Prefix), typeof(string), typeof(AvluField), string.Empty);
    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(AvluField), string.Empty);
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(AvluField), string.Empty, BindingMode.TwoWay);
    public static readonly BindableProperty IsPasswordProperty =
        BindableProperty.Create(nameof(IsPassword), typeof(bool), typeof(AvluField), false);
    public static readonly BindableProperty KeyboardProperty =
        BindableProperty.Create(nameof(Keyboard), typeof(Keyboard), typeof(AvluField), Microsoft.Maui.Keyboard.Default);

    public string Label { get => (string)GetValue(LabelProperty); set => SetValue(LabelProperty, value); }
    public string Prefix { get => (string)GetValue(PrefixProperty); set => SetValue(PrefixProperty, value); }
    public string Placeholder { get => (string)GetValue(PlaceholderProperty); set => SetValue(PlaceholderProperty, value); }
    public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
    public bool IsPassword { get => (bool)GetValue(IsPasswordProperty); set => SetValue(IsPasswordProperty, value); }
    public Keyboard Keyboard { get => (Keyboard)GetValue(KeyboardProperty); set => SetValue(KeyboardProperty, value); }

    private readonly Label _labelView;
    private readonly Border _border;
    private readonly Entry _entry;
    private readonly Label _prefixView;

    public AvluField()
    {
        Spacing = 6;

        _labelView = new Label
        {
            FontFamily = "Inter",
            FontSize = 12,
            CharacterSpacing = 0.6,
        };
        _labelView.SetAppThemeColor(Label.TextColorProperty, Color.FromArgb("#5C6A7E"), Color.FromArgb("#8A9AB3"));

        _prefixView = new Label
        {
            FontFamily = "InterSemiBold",
            FontSize = 15,
            VerticalTextAlignment = TextAlignment.Center,
            IsVisible = false,
        };
        _prefixView.SetAppThemeColor(Label.TextColorProperty, Color.FromArgb("#5C6A7E"), Color.FromArgb("#8A9AB3"));

        _entry = new Entry
        {
            FontFamily = "Inter",
            FontSize = 16,
            BackgroundColor = Colors.Transparent,
            VerticalOptions = LayoutOptions.Center,
        };
        _entry.SetAppThemeColor(Entry.TextColorProperty, Color.FromArgb("#0B1524"), Color.FromArgb("#EAEEF5"));
        _entry.SetAppThemeColor(Entry.PlaceholderColorProperty, Color.FromArgb("#5C6A7E"), Color.FromArgb("#8A9AB3"));

        var row = new HorizontalStackLayout { Spacing = 10, VerticalOptions = LayoutOptions.Center };
        row.Children.Add(_prefixView);
        row.Children.Add(_entry);

        _border = new Border
        {
            HeightRequest = 52,
            Padding = new Thickness(14, 0),
            StrokeThickness = 1.5,
            StrokeShape = new RoundRectangle { CornerRadius = 14 },
            Content = row,
        };
        _border.SetAppThemeColor(Border.BackgroundColorProperty, Colors.White, Color.FromArgb("#0AFFFFFF"));
        _border.SetAppThemeColor(Border.StrokeProperty, Color.FromArgb("#E4E8EF"), Color.FromArgb("#1FFFFFFF"));

        _entry.Focused += (_, __) => SetFocused(true);
        _entry.Unfocused += (_, __) => SetFocused(false);
        _entry.TextChanged += (_, e) => Text = e.NewTextValue;

        Children.Add(_labelView);
        Children.Add(_border);

        PropertyChanged += OnPropChanged;
    }

    private void OnPropChanged(object? s, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(Label): _labelView.Text = Label; break;
            case nameof(Prefix):
                _prefixView.Text = Prefix;
                _prefixView.IsVisible = !string.IsNullOrEmpty(Prefix);
                break;
            case nameof(Placeholder): _entry.Placeholder = Placeholder; break;
            case nameof(Text): if (_entry.Text != Text) _entry.Text = Text; break;
            case nameof(IsPassword): _entry.IsPassword = IsPassword; break;
            case nameof(Keyboard): _entry.Keyboard = Keyboard; break;
        }
    }

    private void SetFocused(bool on)
    {
        var current = Application.Current?.RequestedTheme == AppTheme.Dark;
        var focusColor = on
            ? (current ? Color.FromArgb("#C8A15A") : Color.FromArgb("#0E2A47"))
            : (current ? Color.FromArgb("#1FFFFFFF") : Color.FromArgb("#E4E8EF"));
        _border.Stroke = new SolidColorBrush(focusColor);
        // Yumuşak focus ring için Shadow kullanıyoruz
        _border.Shadow = on
            ? new Shadow { Brush = new SolidColorBrush(current ? Color.FromArgb("#1FC8A15A") : Color.FromArgb("#100E2A47")), Offset = new Point(0, 0), Radius = 10, Opacity = 1 }
            : null;
    }
}
