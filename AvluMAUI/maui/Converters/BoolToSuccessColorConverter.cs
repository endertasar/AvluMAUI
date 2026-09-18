using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Globalization;

namespace Avlu.Converters;

/// <summary>bool → yeşil (başarı) / lacivert-ink (nötr). Tahsilat listelerinde +/- tutar rengi için.</summary>
public class BoolToSuccessColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var positive = value is bool b && b;
        var dark = Application.Current?.RequestedTheme == AppTheme.Dark;
        return positive ? Color.FromArgb("#2E7D57") : (dark ? Color.FromArgb("#EAEEF5") : Color.FromArgb("#0B1524"));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
