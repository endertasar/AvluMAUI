using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace Avlu.Controls;

/// <summary>Tahsilat/Ek Ödeme ilerleme çubuğu.</summary>
public class ProgressBarView : Border
{
    public static readonly BindableProperty PercentProperty =
        BindableProperty.Create(nameof(Percent), typeof(double), typeof(ProgressBarView), 0.0, propertyChanged: OnChanged);

    public double Percent { get => (double)GetValue(PercentProperty); set => SetValue(PercentProperty, value); }

    private readonly BoxView _fill;
    private readonly Grid _track;

    public ProgressBarView()
    {
        HeightRequest = 6;
        StrokeThickness = 0;
        StrokeShape = new RoundRectangle { CornerRadius = 4 };
        this.SetAppThemeColor(Border.BackgroundColorProperty, Color.FromArgb("#F0F3F8"), Color.FromArgb("#14FFFFFF"));

        _fill = new BoxView { CornerRadius = 4, HorizontalOptions = LayoutOptions.Start };
        _fill.SetAppThemeColor(BoxView.ColorProperty, Color.FromArgb("#0E2A47"), Color.FromArgb("#C8A15A"));
        _track = new Grid { HeightRequest = 6 };
        _track.Children.Add(_fill);
        Content = _track;
        SizeChanged += (_, __) => Apply();
    }

    private static void OnChanged(BindableObject b, object o, object n) => ((ProgressBarView)b).Apply();

    private void Apply()
    {
        var w = Width > 0 ? Width : 200;
        _fill.WidthRequest = w * System.Math.Clamp(Percent, 0, 100) / 100.0;
    }
}
