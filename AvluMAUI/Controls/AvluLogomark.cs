using Microsoft.Maui.Controls.Shapes;

namespace AvluMAUI.Controls;

public class AvluLogomark : Grid
{
    public static readonly BindableProperty MainColorProperty =
        BindableProperty.Create(nameof(MainColor), typeof(Color), typeof(AvluLogomark),
            Color.FromArgb("#0E2A47"), propertyChanged: (b, _, _) => ((AvluLogomark)b).ApplyColors());

    public static readonly BindableProperty AccentColorProperty =
        BindableProperty.Create(nameof(AccentColor), typeof(Color), typeof(AvluLogomark),
            Color.FromArgb("#C8A15A"), propertyChanged: (b, _, _) => ((AvluLogomark)b).ApplyColors());

    public static readonly BindableProperty SizeProperty =
        BindableProperty.Create(nameof(Size), typeof(double), typeof(AvluLogomark), 48.0,
            propertyChanged: (b, _, _) => ((AvluLogomark)b).ApplySize());

    public Color MainColor   { get => (Color)GetValue(MainColorProperty);   set => SetValue(MainColorProperty, value); }
    public Color AccentColor { get => (Color)GetValue(AccentColorProperty); set => SetValue(AccentColorProperty, value); }
    public double Size       { get => (double)GetValue(SizeProperty);       set => SetValue(SizeProperty, value); }

    private readonly Rectangle _frame;
    private readonly Microsoft.Maui.Controls.Shapes.Path _arch;
    private readonly Line _baseline;

    public AvluLogomark()
    {
        WidthRequest = 48; HeightRequest = 48;

        _frame = new Rectangle
        {
            StrokeThickness = 2.5,
            WidthRequest = 42, HeightRequest = 42,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions   = LayoutOptions.Center,
            RadiusX = 11, RadiusY = 11,
            Fill = Brush.Transparent
        };

        _arch = new Microsoft.Maui.Controls.Shapes.Path
        {
            StrokeThickness = 2.5,
            StrokeLineCap   = PenLineCap.Round,
            Data = (Geometry)new PathGeometryConverter()
                .ConvertFromInvariantString("M 16 34 V 24 A 8 8 0 0 1 32 24 V 34")!
        };

        _baseline = new Line
        {
            X1 = 14, Y1 = 34, X2 = 34, Y2 = 34,
            StrokeThickness = 2.5,
            StrokeLineCap   = PenLineCap.Round
        };

        Children.Add(_frame);
        Children.Add(_arch);
        Children.Add(_baseline);
        ApplyColors();
        ApplySize();
    }

    void ApplyColors()
    {
        _frame.Stroke    = new SolidColorBrush(MainColor);
        _arch.Stroke     = new SolidColorBrush(AccentColor);
        _baseline.Stroke = new SolidColorBrush(MainColor);
    }

    void ApplySize()
    {
        WidthRequest = Size; HeightRequest = Size;
        var inner = Size * 42.0 / 48.0;
        _frame.WidthRequest = inner; _frame.HeightRequest = inner;
        var scale = Size / 48.0;
        _arch.Scale     = scale;
        _baseline.Scale = scale;
    }
}
