using System.Globalization;

namespace AvluMAUI.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var boolVal = value switch
        {
            bool b   => b,
            string s => !string.IsNullOrEmpty(s),
            _        => false
        };
        var invert = parameter as string == "invert";
        return invert ? !boolVal : boolVal;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool b && b;
}
