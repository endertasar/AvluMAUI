using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace Avlu.Controls;

/// <summary>
/// Mülk Sahibi / Yönetici seçimi — segmented control.
/// "pill" arka plan, seçili tab beyaz yüzey + ince gölge.
/// </summary>
public class RoleSegmented : Border
{
    public enum Role { Resident, Manager }

    public static readonly BindableProperty SelectedProperty =
        BindableProperty.Create(nameof(Selected), typeof(Role), typeof(RoleSegmented), Role.Resident,
            BindingMode.TwoWay, propertyChanged: OnSelectedChanged);

    public Role Selected { get => (Role)GetValue(SelectedProperty); set => SetValue(SelectedProperty, value); }
    public event System.EventHandler<Role>? SelectionChanged;

    private readonly Grid _grid;
    private readonly Border _residentBg, _managerBg;
    private readonly Label _residentLbl, _managerLbl;

    public RoleSegmented()
    {
        Padding = new Thickness(4);
        StrokeThickness = 0;
        StrokeShape = new RoundRectangle { CornerRadius = 12 };
        this.SetAppThemeColor(Border.BackgroundColorProperty, Color.FromArgb("#F0F3F8"), Color.FromArgb("#14FFFFFF"));

        _grid = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) },
            ColumnSpacing = 4,
            HeightRequest = 44,
        };

        _residentBg = BuildTabBg();
        _residentLbl = BuildTabLabel("Mülk Sahibi");
        _managerBg = BuildTabBg();
        _managerLbl = BuildTabLabel("Yönetici");

        var c0 = new Grid { Children = { _residentBg, _residentLbl } };
        var c1 = new Grid { Children = { _managerBg, _managerLbl } };
        Grid.SetColumn(c1, 1);

        c0.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => Selected = Role.Resident) });
        c1.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => Selected = Role.Manager) });

        _grid.Children.Add(c0);
        _grid.Children.Add(c1);
        Content = _grid;

        ApplySelection();
    }

    private static Border BuildTabBg()
    {
        var b = new Border
        {
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 9 },
            BackgroundColor = Colors.Transparent,
        };
        return b;
    }

    private static Label BuildTabLabel(string text)
    {
        var l = new Label
        {
            Text = text,
            FontFamily = "InterSemiBold",
            FontSize = 13,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
        };
        return l;
    }

    private static void OnSelectedChanged(BindableObject b, object o, object n) => ((RoleSegmented)b).ApplySelection();

    private void ApplySelection()
    {
        var dark = Application.Current?.RequestedTheme == AppTheme.Dark;
        var activeBg = dark ? Color.FromArgb("#111C2E") : Colors.White;
        var activeFg = dark ? Color.FromArgb("#EAEEF5") : Color.FromArgb("#0E2A47");
        var idleFg = dark ? Color.FromArgb("#8A9AB3") : Color.FromArgb("#5C6A7E");

        var residentActive = Selected == Role.Resident;
        _residentBg.BackgroundColor = residentActive ? activeBg : Colors.Transparent;
        _managerBg.BackgroundColor = !residentActive ? activeBg : Colors.Transparent;
        _residentLbl.TextColor = residentActive ? activeFg : idleFg;
        _managerLbl.TextColor = !residentActive ? activeFg : idleFg;

        _residentBg.Shadow = residentActive ? new Shadow { Brush = new SolidColorBrush(Color.FromArgb("#140E2A47")), Offset = new Point(0, 1), Radius = 3, Opacity = 1 } : null;
        _managerBg.Shadow = !residentActive ? new Shadow { Brush = new SolidColorBrush(Color.FromArgb("#140E2A47")), Offset = new Point(0, 1), Radius = 3, Opacity = 1 } : null;

        SelectionChanged?.Invoke(this, Selected);
    }
}
