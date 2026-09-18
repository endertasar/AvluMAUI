using System.Globalization;

namespace AvluMAUI.Converters;

/// <summary>
/// Formats a decimal as Turkish lira: e.g. 1250.5 → "₺1.250,50"
/// </summary>
public class AmountToStringConverter : IValueConverter
{
    private static readonly CultureInfo TrCulture = new("tr-TR");

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal d) return d.ToString("C", TrCulture);
        if (value is double  dbl) return ((decimal)dbl).ToString("C", TrCulture);
        if (value is float   f)   return ((decimal)f  ).ToString("C", TrCulture);
        return "₺0,00";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var str = value?.ToString()?.Replace("₺", "").Trim();
        return decimal.TryParse(str, NumberStyles.Any, TrCulture, out var result) ? result : 0m;
    }
}
