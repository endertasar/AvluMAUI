using System.Globalization;

namespace AvluMAUI.Converters;

public class StatusToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var status = value as string ?? "";
        return status switch
        {
            "Paid"    => Color.FromArgb("#2E7D57"),  // Success green
            "Partial" => Color.FromArgb("#1565C0"),  // Blue
            "Pending" => Color.FromArgb("#E65100"),  // Orange
            "Overdue" => Color.FromArgb("#B3261E"),  // Danger red
            _         => Color.FromArgb("#6B7280")   // Gray
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
